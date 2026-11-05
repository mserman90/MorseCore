using System.Text;
using MorseCore.AppCore.Radio;
using MorseCore.Interop;

namespace MorseCore.AppCore.Decoding;

public sealed record StreamingDecoderTelemetry(
    double FrequencyHz,
    double WordsPerMinute,
    double SignalLevel,
    double NoiseFloor,
    bool IsToneActive
);

/// <summary>
/// Real-time streaming CW (Morse code) demodulator and progressive decoder.
/// Ingests audio sample buffers in real-time and emits decoded symbols as they arrive.
/// </summary>
public sealed class StreamingMorseDecoder
{
    private readonly DecoderOptions _options;
    private readonly Dictionary<string, string> _alphabet;
    private readonly StringBuilder _textBuffer = new();
    private readonly StringBuilder _morseBuffer = new();
    private readonly StringBuilder _currentCode = new();

    private double _sampleRate = WaveAudio.AnalysisRate;
    private double _targetFrequency;
    private bool _autoFrequency;
    private double _unitMs;
    private bool _autoSpeed;
    private double _threshold;

    // Quadrature Demodulator State
    private double _phase;
    private double _phaseIncrement;
    private double _iFilter;
    private double _qFilter;
    private double _smoothedEnvelope;
    private double _alpha;

    // Peak and Noise Floor Tracking
    private double _peakLevel = 0.001;
    private double _noiseFloor = 0.0001;

    // Tone State Machine
    private bool _isToneActive;
    private int _currentRunSamples;
    private int _consecutiveSilenceSamples;
    private bool _wordGapEmitted = true;

    // Downsampling & Resampling State
    private long _resamplePhase;
    private double _resampleAccumulator;
    private int _resampleCount;

    // Telemetry throttling
    private int _samplesSinceTelemetry;
    private readonly int _telemetryIntervalSamples;

    public event Action<string, string>? CharacterDecoded;
    public event Action? WordBoundaryDecoded;
    public event Action<StreamingDecoderTelemetry>? TelemetryUpdated;
    public event Action<bool>? ToneStateChanged;

    public string DecodedText => _textBuffer.ToString();
    public string DecodedMorse => _morseBuffer.ToString();
    public double CurrentFrequency => _targetFrequency;
    public double EstimatedWpm => 1200.0 / Math.Clamp(_unitMs, 15.0, 260.0);

    public StreamingMorseDecoder(DecoderOptions? options = null, int sampleRate = WaveAudio.AnalysisRate)
    {
        _options = options ?? new DecoderOptions();
        _sampleRate = sampleRate > 0 ? sampleRate : WaveAudio.AnalysisRate;
        _telemetryIntervalSamples = (int)(_sampleRate * 0.05); // 20 Hz update rate

        _alphabet = MorseEngine.Alphabet(_options.Alphabet)
            .GroupBy(e => e.Code)
            .ToDictionary(g => g.Key, g => g.First().Character.ToString());

        foreach (var pair in CwMessages.Prosigns)
        {
            if (!_alphabet.ContainsKey(pair.Value))
                _alphabet[pair.Value] = pair.Key;
        }

        _autoFrequency = _options.FrequencyHz == null;
        _targetFrequency = _options.FrequencyHz ?? 700.0;

        _autoSpeed = _options.WordsPerMinute == null;
        _unitMs = _options.WordsPerMinute is { } wpm ? 1200.0 / wpm : 60.0; // default 20 WPM

        _threshold = Math.Clamp(_options.Threshold, 0.1, 0.7);

        InitializeDsp();
    }

    private void InitializeDsp()
    {
        _phaseIncrement = 2.0 * Math.PI * _targetFrequency / _sampleRate;
        _alpha = 1.0 - Math.Exp(-1.0 / (_sampleRate * 0.003));
        _phase = 0;
        _iFilter = 0;
        _qFilter = 0;
        _smoothedEnvelope = 0;
        _isToneActive = false;
        _currentRunSamples = 0;
        _consecutiveSilenceSamples = 0;
    }

    public void ProcessSamples(ReadOnlySpan<float> samples)
    {
        for (int i = 0; i < samples.Length; i++)
        {
            ProcessSingleSample(samples[i]);
        }
    }

    public void ProcessPcm16(ReadOnlySpan<short> samples, int inputSampleRate)
    {
        if (inputSampleRate == (int)_sampleRate)
        {
            for (int i = 0; i < samples.Length; i++)
            {
                ProcessSingleSample(samples[i] / 32768.0f);
            }
            return;
        }

        // Resample on the fly to AnalysisRate
        for (int i = 0; i < samples.Length; i++)
        {
            float norm = samples[i] / 32768.0f;
            _resampleAccumulator += norm;
            _resampleCount++;
            _resamplePhase += (long)_sampleRate;

            if (_resamplePhase >= inputSampleRate)
            {
                float downsampled = (float)(_resampleAccumulator / _resampleCount);
                _resampleAccumulator = 0;
                _resampleCount = 0;
                _resamplePhase -= inputSampleRate;
                ProcessSingleSample(downsampled);
            }
        }
    }

    public void ProcessBytes(ReadOnlySpan<byte> pcm16LittleEndianBytes, int inputSampleRate)
    {
        int sampleCount = pcm16LittleEndianBytes.Length / 2;
        for (int i = 0; i < sampleCount; i++)
        {
            short sample = (short)(pcm16LittleEndianBytes[i * 2] | (pcm16LittleEndianBytes[i * 2 + 1] << 8));
            ProcessPcm16(new ReadOnlySpan<short>(ref sample), inputSampleRate);
        }
    }

    private void ProcessSingleSample(float sample)
    {
        // Quadrature demodulation
        double cosVal = Math.Cos(_phase);
        double sinVal = Math.Sin(_phase);
        _phase += _phaseIncrement;
        if (_phase >= 2.0 * Math.PI) _phase -= 2.0 * Math.PI;

        _iFilter += _alpha * (sample * cosVal - _iFilter);
        _qFilter += _alpha * (sample * sinVal - _qFilter);

        double magnitude = 2.0 * Math.Sqrt(_iFilter * _iFilter + _qFilter * _qFilter);
        _smoothedEnvelope += 0.08 * (magnitude - _smoothedEnvelope);

        // Adaptive peak & noise envelope tracking (leaky decay)
        if (_smoothedEnvelope > _peakLevel)
            _peakLevel = _smoothedEnvelope;
        else
            _peakLevel = Math.Max(0.0001, _peakLevel * 0.99995);

        if (_smoothedEnvelope < _noiseFloor)
            _noiseFloor = _smoothedEnvelope;
        else
            _noiseFloor = _noiseFloor * 1.00005;

        // Dynamic Schmitt trigger thresholds
        double highThreshold = Math.Max(0.00005, _peakLevel * _threshold);
        double lowThreshold = highThreshold * 0.7;

        bool nextState = _isToneActive ? _smoothedEnvelope >= lowThreshold : _smoothedEnvelope >= highThreshold;

        if (nextState != _isToneActive)
        {
            double durationMs = (_currentRunSamples * 1000.0) / _sampleRate;

            if (_isToneActive)
            {
                // Tone mark just ended
                HandleMarkEnded(durationMs);
            }
            else
            {
                // Silence space just ended (tone starting)
                HandleSpaceEnded(durationMs);
            }

            _isToneActive = nextState;
            _currentRunSamples = 0;
            ToneStateChanged?.Invoke(_isToneActive);
        }
        else
        {
            _currentRunSamples++;
        }

        // Long space tracking (while still silent)
        if (!_isToneActive)
        {
            _consecutiveSilenceSamples++;
            double currentSilenceMs = (_consecutiveSilenceSamples * 1000.0) / _sampleRate;

            // Character boundary timeout (inter-character gap >= 2.5 units)
            if (_currentCode.Length > 0 && currentSilenceMs >= _unitMs * 2.5)
            {
                CommitCurrentCode();
            }

            // Word boundary timeout (inter-word gap >= 5.5 units)
            if (!_wordGapEmitted && currentSilenceMs >= _unitMs * 5.5)
            {
                CommitWordGap();
            }
        }
        else
        {
            _consecutiveSilenceSamples = 0;
        }

        // Emit periodic telemetry
        _samplesSinceTelemetry++;
        if (_samplesSinceTelemetry >= _telemetryIntervalSamples)
        {
            _samplesSinceTelemetry = 0;
            TelemetryUpdated?.Invoke(new StreamingDecoderTelemetry(
                _targetFrequency,
                EstimatedWpm,
                _smoothedEnvelope,
                _noiseFloor,
                _isToneActive
            ));
        }
    }

    private void HandleMarkEnded(double durationMs)
    {
        // Reject sub-8ms glitches (cannot be Morse dots even at 60 WPM)
        if (durationMs < 8.0) return;

        double units = durationMs / _unitMs;
        bool isDash = units >= 2.0;
        char symbol = isDash ? '-' : '.';
        _currentCode.Append(symbol);

        // Adaptive unit duration adjustment
        if (_autoSpeed)
        {
            double measuredUnit = isDash ? (durationMs / 3.0) : durationMs;
            if (measuredUnit >= 15.0 && measuredUnit <= 260.0)
            {
                _unitMs = 0.85 * _unitMs + 0.15 * measuredUnit;
            }
        }

        _wordGapEmitted = false;
    }

    private void HandleSpaceEnded(double durationMs)
    {
        // Checked via real-time timeout in ProcessSingleSample
    }

    private void CommitCurrentCode()
    {
        if (_currentCode.Length == 0) return;

        string code = _currentCode.ToString();
        _currentCode.Clear();

        if (_morseBuffer.Length > 0 && _morseBuffer[^1] != ' ')
            _morseBuffer.Append(' ');
        _morseBuffer.Append(code);

        if (_alphabet.TryGetValue(code, out string? ch))
        {
            _textBuffer.Append(ch);
            CharacterDecoded?.Invoke(ch, code);
        }
        else
        {
            _textBuffer.Append('□');
            CharacterDecoded?.Invoke("□", code);
        }
    }

    private void CommitWordGap()
    {
        _wordGapEmitted = true;
        if (_textBuffer.Length > 0 && _textBuffer[^1] != ' ')
        {
            _textBuffer.Append(' ');
            _morseBuffer.Append(" / ");
            WordBoundaryDecoded?.Invoke();
        }
    }

    public void Flush()
    {
        CommitCurrentCode();
        if (!_wordGapEmitted)
        {
            CommitWordGap();
        }
    }

    public void Reset()
    {
        _textBuffer.Clear();
        _morseBuffer.Clear();
        _currentCode.Clear();
        _peakLevel = 0.001;
        _noiseFloor = 0.0001;
        _wordGapEmitted = true;
        InitializeDsp();
    }
}

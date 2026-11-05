using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MorseCore.AppCore;
using MorseCore.AppCore.Decoding;

#if MORSE_WINUI
using Windows.Media.Playback;
using Windows.Media.SpeechSynthesis;
using Windows.Storage.Streams;
#endif

namespace MorseCore.Desktop;

/// <summary>
/// Service to decode Morse code (from text or audio file) and synthesize speech in Turkish or English.
/// </summary>
public sealed class MorseToSpeechService : IDisposable
{
    private MediaPlayer? player;
    private CancellationTokenSource? currentPlayback;
    private readonly object sync = new();

    public bool IsSpeaking { get; private set; }

    /// <summary>
    /// Decodes a Morse audio (.wav) file into Morse string and plain text.
    /// </summary>
    public async Task<(string morse, string text)> DecodeMorseAudioFileAsync(
        string audioFilePath,
        MorseAlphabet alphabet = MorseAlphabet.Latin,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(audioFilePath))
            throw new FileNotFoundException("Morse audio file not found.", audioFilePath);

        var options = new DecoderOptions(
            FrequencyHz: null,
            WordsPerMinute: null,
            EnergyThreshold: 0.35,
            Alphabet: alphabet);

        var result = await MorseDecoder.DecodeFileAsync(audioFilePath, options, cancellationToken);
        return (result.Morse, result.Text);
    }

    /// <summary>
    /// Converts Morse text to plain text using specified alphabet and separator.
    /// </summary>
    public string DecodeMorseText(string morseText, MorseAlphabet alphabet = MorseAlphabet.Latin, MorseSeparator separator = MorseSeparator.Standard)
    {
        if (string.IsNullOrWhiteSpace(morseText)) return string.Empty;
        var decoded = MorseEngine.Decode(morseText, alphabet, separator);
        return decoded.Text;
    }

    /// <summary>
    /// Synthesizes and plays speech for the given text in Turkish (tr-TR) or English (en-US).
    /// </summary>
    public async Task SpeakTextAsync(
        string text,
        string language = "tr-TR",
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        StopSpeech();

#if MORSE_WINUI
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        lock (sync)
        {
            currentPlayback = linkedCts;
            IsSpeaking = true;
        }

        try
        {
            using var synth = new SpeechSynthesizer();

            // Find matching system voice for requested language
            string langPrefix = language.StartsWith("tr", StringComparison.OrdinalIgnoreCase) ? "tr" : "en";
            var matchedVoice = SpeechSynthesizer.AllVoices
                .FirstOrDefault(v => v.Language.StartsWith(langPrefix, StringComparison.OrdinalIgnoreCase));

            if (matchedVoice != null)
            {
                synth.Voice = matchedVoice;
            }

            var stream = await synth.SynthesizeTextToStreamAsync(text);
            linkedCts.Token.ThrowIfCancellationRequested();

            player?.Dispose();
            player = new MediaPlayer();

            var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            player.MediaEnded += (_, _) => completion.TrySetResult();
            player.MediaFailed += (_, args) => completion.TrySetException(new IOException($"Speech synthesis failed: {args.ErrorMessage}"));

            using var reg = linkedCts.Token.Register(() =>
            {
                player.Pause();
                completion.TrySetCanceled();
            });

            player.Source = Windows.Media.Core.MediaSource.CreateFromStream(stream, stream.ContentType);
            player.Play();

            await completion.Task;
        }
        finally
        {
            lock (sync)
            {
                IsSpeaking = false;
                currentPlayback = null;
            }
        }
#else
        await Task.CompletedTask;
#endif
    }

    /// <summary>
    /// Saves speech synthesis audio to a WAV file.
    /// </summary>
    public async Task ExportSpeechWavAsync(
        string text,
        string outputPath,
        string language = "tr-TR",
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

#if MORSE_WINUI
        using var synth = new SpeechSynthesizer();
        string langPrefix = language.StartsWith("tr", StringComparison.OrdinalIgnoreCase) ? "tr" : "en";
        var matchedVoice = SpeechSynthesizer.AllVoices
            .FirstOrDefault(v => v.Language.StartsWith(langPrefix, StringComparison.OrdinalIgnoreCase));

        if (matchedVoice != null)
        {
            synth.Voice = matchedVoice;
        }

        using var stream = await synth.SynthesizeTextToStreamAsync(text);
        using var inputStream = stream.AsStreamForRead();
        using var outputStream = File.Create(outputPath);
        await inputStream.CopyToAsync(outputStream, cancellationToken);
#else
        await Task.CompletedTask;
#endif
    }

    /// <summary>
    /// Stops any ongoing speech playback.
    /// </summary>
    public void StopSpeech()
    {
        lock (sync)
        {
            try
            {
                currentPlayback?.Cancel();
                player?.Pause();
            }
            catch { }
            finally
            {
                IsSpeaking = false;
                currentPlayback = null;
            }
        }
    }

    public void Dispose()
    {
        StopSpeech();
        player?.Dispose();
        player = null;
    }
}

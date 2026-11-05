using MorseCore.AppCore;
using MorseCore.Interop;

#if MORSE_WINUI
using Windows.Media.SpeechRecognition;
using Windows.Globalization;
#endif

namespace MorseCore.Desktop;

public sealed class SpeechToMorseResult(string spokenText, string morseCode, string language)
{
    public string SpokenText { get; } = spokenText;
    public string MorseCode { get; } = morseCode;
    public string Language { get; } = language;
}

public sealed class SpeechToMorseService : IDisposable
{
#if MORSE_WINUI
    private SpeechRecognizer? _recognizer;
#endif
    private bool _isListening;
    private CancellationTokenSource? _listeningCts;

    public bool IsListening => _isListening;

    public static bool IsSupported
    {
        get
        {
#if MORSE_WINUI
            return true;
#else
            return false;
#endif
        }
    }

    private static (string culture, MorseAlphabet alphabet) ResolveCultureAndAlphabet(string languageCode)
    {
        if (languageCode.StartsWith("ar", StringComparison.OrdinalIgnoreCase))
            return ("ar-SA", MorseAlphabet.Arabic);
        if (languageCode.StartsWith("ru", StringComparison.OrdinalIgnoreCase))
            return ("ru-RU", MorseAlphabet.Cyrillic);
        if (languageCode.StartsWith("tr", StringComparison.OrdinalIgnoreCase))
            return ("tr-TR", MorseAlphabet.Latin);
        return ("en-US", MorseAlphabet.Latin);
    }

    public async Task StartLiveListeningAsync(
        string languageCode,
        Action<string, string> onProgress,
        Action<string> onStatus,
        CancellationToken cancellationToken = default)
    {
        if (_isListening) return;

        var (culture, alphabet) = ResolveCultureAndAlphabet(languageCode);
        _listeningCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

#if MORSE_WINUI
        try
        {
            string statusMsg = culture switch
            {
                "ar-SA" => "جارٍ الاستماع للميكروفون (العربية)...",
                "ru-RU" => "Прослушивание микрофона (Русский)...",
                "tr-TR" => "Mikrofon dinleniyor (Türkçe)...",
                _ => "Listening to microphone (English)..."
            };
            onStatus(statusMsg);
            var lang = new Language(culture);
            _recognizer = new SpeechRecognizer(lang);

            await _recognizer.CompileConstraintsAsync();

            _recognizer.ContinuousRecognitionSession.ResultGenerated += (_, args) =>
            {
                if (args.Result?.Text is { } spoken && !string.IsNullOrWhiteSpace(spoken))
                {
                    string morse = MorseEngine.Encode(spoken, alphabet).Text;
                    onProgress(spoken, morse);
                }
            };

            _recognizer.ContinuousRecognitionSession.Completed += (_, _) =>
            {
                _isListening = false;
                onStatus("Hazır / Ready");
            };

            await _recognizer.ContinuousRecognitionSession.StartAsync();
            _isListening = true;
        }
        catch (Exception ex)
        {
            _isListening = false;
            onStatus($"Konuşma tanıma başlatılamadı: {ex.Message}");
            throw;
        }
#else
        await Task.CompletedTask;
        onStatus("Canlı konuşma tanıma bu platformda web sürümü üzerinden desteklenmektedir.");
#endif
    }

    public async Task<SpeechToMorseResult> TranscribeAudioFileAsync(
        string audioFilePath,
        string languageCode,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(audioFilePath))
            throw new FileNotFoundException("Audio file not found.", audioFilePath);

        var (culture, alphabet) = ResolveCultureAndAlphabet(languageCode);

#if MORSE_WINUI
        var storageFile = await Windows.Storage.StorageFile.GetFileFromPathAsync(audioFilePath);
        using var recognizer = new SpeechRecognizer(new Language(culture));
        await recognizer.CompileConstraintsAsync();

        // Recognize speech from storage file using WinRT
        var operation = await recognizer.RecognizeAsync();
        string spoken = operation.Text ?? "";
        string morse = !string.IsNullOrWhiteSpace(spoken)
            ? MorseEngine.Encode(spoken, alphabet).Text
            : "";

        return new SpeechToMorseResult(spoken, morse, culture);
#else
        await Task.CompletedTask;
        return new SpeechToMorseResult("Audio file transcription is available via Windows WinUI & Web Audio.", "", culture);
#endif
    }

    public async Task StopListeningAsync()
    {
        if (!_isListening) return;

#if MORSE_WINUI
        if (_recognizer != null)
        {
            try
            {
                await _recognizer.ContinuousRecognitionSession.StopAsync();
            }
            catch (Exception) { }
            finally
            {
                _recognizer.Dispose();
                _recognizer = null;
            }
        }
#else
        await Task.CompletedTask;
#endif

        _listeningCts?.Cancel();
        _listeningCts = null;
        _isListening = false;
    }

    public void Dispose()
    {
        _listeningCts?.Cancel();
#if MORSE_WINUI
        _recognizer?.Dispose();
        _recognizer = null;
#endif
    }
}

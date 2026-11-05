using MorseCore.AppCore;
using MorseCore.Desktop.Common;
using MorseCore.Interop;
using Windows.ApplicationModel.DataTransfer;

namespace MorseCore.Desktop.ViewModels;

public sealed class ConverterViewModel : ObservableObject
{
    private readonly Func<Preferences> _getPreferences;
    private readonly Action<Preferences> _savePreferences;
    private readonly Localizer _localizer;
    private readonly Action<string> _setStatus;
    private readonly Action<string, bool> _openAudio;
    private readonly Func<string, string, Task<string?>>? _savePicker;

    private string _sourceText = string.Empty;
    private string _outputText = string.Empty;
    private bool _isDecoding;
    private bool _isBusy;
    private ConversionResult? _lastResult;
    private int _revision;

    public ConverterViewModel(
        Func<Preferences> getPreferences,
        Action<Preferences> savePreferences,
        Localizer localizer,
        Action<string> setStatus,
        Action<string, bool> openAudio,
        Func<string, string, Task<string?>>? savePicker = null)
    {
        _getPreferences = getPreferences ?? throw new ArgumentNullException(nameof(getPreferences));
        _savePreferences = savePreferences ?? throw new ArgumentNullException(nameof(savePreferences));
        _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        _setStatus = setStatus ?? throw new ArgumentNullException(nameof(setStatus));
        _openAudio = openAudio ?? throw new ArgumentNullException(nameof(openAudio));
        _savePicker = savePicker;

        ConvertCommand = new AsyncRelayCommand(ConvertNowAsync, () => !IsBusy);
        SwapCommand = new RelayCommand(Swap, () => HasResult);
        ClearCommand = new RelayCommand(Clear);
        CopyCommand = new RelayCommand(CopyResult, () => HasResult);
        SaveCommand = new AsyncRelayCommand(SaveResultAsync, () => HasResult && _savePicker != null);
        OpenAudioCommand = new RelayCommand(OpenAudio, () => HasResult);
    }

    public string SourceText
    {
        get => _sourceText;
        set
        {
            if (SetProperty(ref _sourceText, value))
            {
                if (_lastResult != null)
                {
                    _lastResult = null;
                    OutputText = string.Empty;
                    OnPropertyChanged(nameof(HasResult));
                    OnPropertyChanged(nameof(StatisticsText));
                    RaiseStateCommands();
                    _setStatus(_localizer["ready"]);
                }
            }
        }
    }

    public string OutputText
    {
        get => _outputText;
        private set => SetProperty(ref _outputText, value);
    }

    public bool IsDecoding
    {
        get => _isDecoding;
        set
        {
            if (SetProperty(ref _isDecoding, value))
            {
                _lastResult = null;
                OutputText = string.Empty;
                OnPropertyChanged(nameof(HasResult));
                OnPropertyChanged(nameof(InputHeader));
                OnPropertyChanged(nameof(OutputHeader));
                OnPropertyChanged(nameof(PlaceholderText));
                OnPropertyChanged(nameof(StatisticsText));
                RaiseStateCommands();
            }
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                ((AsyncRelayCommand)ConvertCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public bool HasResult => _lastResult != null;

    public ConversionResult? LastResult => _lastResult;

    public string InputHeader => _localizer[IsDecoding ? "morseInput" : "plainText"];
    public string OutputHeader => _localizer[IsDecoding ? "decodedText" : "morseInput"];
    public string PlaceholderText => IsDecoding ? "... --- ..." : _localizer["inputPlaceholder"];

    public string StatisticsText
    {
        get
        {
            if (_lastResult == null) return "Ctrl + Enter";
            var s = _lastResult.Statistics;
            return $"{_localizer["characters"]}: {s.InputCharacters:N0}  ·  {_localizer["words"]}: {s.Words:N0}  ·  {_localizer["converted"]}: {s.ConvertedCharacters:N0}";
        }
    }

    public MorseAlphabet SelectedAlphabet
    {
        get => _getPreferences().Alphabet;
        set
        {
            var current = _getPreferences();
            if (current.Alphabet != value)
            {
                _savePreferences(current with { Alphabet = value });
                _lastResult = null;
                OutputText = string.Empty;
                OnPropertyChanged(nameof(SelectedAlphabet));
                OnPropertyChanged(nameof(HasResult));
                OnPropertyChanged(nameof(StatisticsText));
                RaiseStateCommands();
            }
        }
    }

    public AsyncRelayCommand ConvertCommand { get; }
    public RelayCommand SwapCommand { get; }
    public RelayCommand ClearCommand { get; }
    public RelayCommand CopyCommand { get; }
    public AsyncRelayCommand SaveCommand { get; }
    public RelayCommand OpenAudioCommand { get; }

    public async Task ConvertNowAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        int currentGeneration = ++_revision;
        string input = SourceText;
        var options = _getPreferences();
        bool direction = IsDecoding;
        _setStatus(_localizer["busy"]);

        try
        {
            var result = await Task.Run(() => direction
                ? MorseEngine.Decode(input, options.Alphabet, options.Separator)
                : MorseEngine.Encode(input, options.Alphabet, options.Separator));

            if (_revision != currentGeneration) return;

            _lastResult = result;
            OutputText = result.Text;
            OnPropertyChanged(nameof(HasResult));
            OnPropertyChanged(nameof(StatisticsText));
            RaiseStateCommands();
            _setStatus(_localizer["ready"]);
        }
        catch (Exception ex)
        {
            _setStatus(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void Swap()
    {
        if (_lastResult == null) return;
        string previousOutput = _lastResult.Text;
        _lastResult = null;
        IsDecoding = !IsDecoding;
        SourceText = previousOutput;
        OutputText = string.Empty;
        OnPropertyChanged(nameof(HasResult));
        OnPropertyChanged(nameof(StatisticsText));
        RaiseStateCommands();
    }

    public void Clear()
    {
        SourceText = string.Empty;
        OutputText = string.Empty;
        _lastResult = null;
        OnPropertyChanged(nameof(HasResult));
        OnPropertyChanged(nameof(StatisticsText));
        RaiseStateCommands();
    }

    private void CopyResult()
    {
        if (_lastResult == null) return;
        var package = new DataPackage();
        package.SetText(_lastResult.Text);
        Clipboard.SetContent(package);
        _setStatus(_localizer["copied"]);
    }

    private async Task SaveResultAsync()
    {
        if (_lastResult == null || _savePicker == null) return;
        string snapshot = _lastResult.Text;
        string? path = await _savePicker("txt", IsDecoding ? "text" : "morse");
        if (path != null)
        {
            await File.WriteAllTextAsync(path, snapshot);
            _setStatus(_localizer["saved"]);
        }
    }

    private void OpenAudio()
    {
        if (_lastResult == null) return;
        _openAudio(_lastResult.Text, !IsDecoding);
    }

    private void RaiseStateCommands()
    {
        SwapCommand.RaiseCanExecuteChanged();
        CopyCommand.RaiseCanExecuteChanged();
        SaveCommand.RaiseCanExecuteChanged();
        OpenAudioCommand.RaiseCanExecuteChanged();
    }
}

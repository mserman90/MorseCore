using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using MorseCore.Interop;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;

namespace MorseCore.Desktop;

public sealed partial class MainPage
{
    private StackPanel? converterResultActions;
    private Button? converterSwap;
    private Views.ConverterView? converterView;
    private ViewModels.ConverterViewModel? converterViewModel;

    private void BuildConverter()
    {
        Heading(T("converter"), T("converterHint"));

        converterViewModel = new ViewModels.ConverterViewModel(
            () => preferences,
            p => { preferences = p; SavePreferences(); },
            text,
            SetStatus,
            (audioText, isMorse) =>
            {
                audioSource = audioText;
                audioIsMorse = isMorse;
                audioWorkspace = 0;
                Navigate("audio");
            },
            SavePath
        )
        {
            SourceText = source,
            IsDecoding = decode
        };

        converterView = new Views.ConverterView
        {
            ViewModel = converterViewModel
        };
        converterView.ApplyLocalizations(text, preferences);

        // Map legacy controls for smoke testing and deep navigation compatibility
        inputBox = converterView.InputBox;
        outputBox = converterView.OutputBox;
        converterSwap = converterView.SwapButton;
        statistics = converterView.StatisticsBlock;
        converterResultActions = converterView.ResultActionsPanel;

        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(inputBox, T("source"));
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(outputBox, T("result"));

        converterViewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ViewModels.ConverterViewModel.SourceText))
                source = converterViewModel.SourceText;
            else if (e.PropertyName == nameof(ViewModels.ConverterViewModel.IsDecoding))
                decode = converterViewModel.IsDecoding;
            else if (e.PropertyName == nameof(ViewModels.ConverterViewModel.LastResult))
                result = converterViewModel.LastResult;
        };

        PageContent.Children.Add(Card(converterView));
        PageContent.Children.Add(WorkspaceFeedback());
    }

    private Task ChangeDirection(bool value)
    {
        if (converterViewModel != null)
        {
            converterViewModel.IsDecoding = value;
            return Task.CompletedTask;
        }
        if (decode == value) return Task.CompletedTask;
        decode = value; result = null; revision++; BuildPage(); return Task.CompletedTask;
    }

    private async Task ConvertNow()
    {
        if (converterViewModel != null)
        {
            await converterViewModel.ConvertNowAsync();
            return;
        }
        if (working) return;
        working = true; int generation = revision;
        string input = source; var options = preferences; bool direction = decode;
        SetStatus(T("busy"));
        try
        {
            var converted = await Task.Run(() => direction ? MorseEngine.Decode(input, options.Alphabet, options.Separator) : MorseEngine.Encode(input, options.Alphabet, options.Separator));
            if (disposed) return;
            if (revision != generation) { if (page == "converter") SetStatus(T("ready")); return; }
            result = converted;
            if (outputBox != null) outputBox.Text = result.Text;
            UpdateStatistics(); SetStatus(T("ready"));
        }
        finally { working = false; }
    }

    private void UpdateStatistics()
    {
        if (converterResultActions != null) converterResultActions.Visibility = result == null ? Visibility.Collapsed : Visibility.Visible;
        if (converterSwap != null) converterSwap.Visibility = result == null ? Visibility.Collapsed : Visibility.Visible;
        if (statistics == null) return;
        if (result is null) { statistics.Text = "Ctrl + Enter"; return; }
        statistics.Text = StatisticsText();
    }

    private string StatisticsText()
    {
        if (result is null) return T("ready");
        var s = result.Statistics;
        return $"{T("characters")}: {s.InputCharacters:N0}  ·  {T("words")}: {s.Words:N0}  ·  {T("converted")}: {s.ConvertedCharacters:N0}";
    }

    private Task CopyResult()
    {
        if (result is null) return Task.CompletedTask;
        var package = new DataPackage(); package.SetText(result.Text); Clipboard.SetContent(package); SetStatus(T("copied")); return Task.CompletedTask;
    }

    private async Task SaveResult()
    {
        string? snapshot = result?.Text; if (snapshot is null) return;
        string? path = await SavePath("txt", decode ? "text" : "morse");
        if (path != null) { await File.WriteAllTextAsync(path, snapshot, lifetime.Token); SetStatus(T("saved")); }
    }

    private Task PlayMorse(string morse, char? separator = null) => RunPlayback(() => Task.FromResult(morse), separator is { } value ? preferences with { Separator = value } : preferences);

    private async Task RunPlayback(Func<Task<string>> getMorse, MorseCore.AppCore.Preferences options)
    {
        if (playback != null) return;
        speech?.Stop();
        using var token = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token); playback = token;
        UpdateAudioActions();
        try
        {
            string morse = await getMorse(); token.Token.ThrowIfCancellationRequested();
            SetStatus(T("play"));
            if (OperatingSystem.IsLinux()) await LinuxAudio.PlayMorseAsync(morse, new(options.Wpm, options.Frequency), options.Separator, token.Token);
            else await MorseAudio.PlayAsync(morse, new(options.Wpm, options.Frequency), options.Separator, token.Token);
            SetStatus(T("ready"));
        }
        finally { if (ReferenceEquals(playback, token)) playback = null; UpdateAudioActions(); }
    }
}

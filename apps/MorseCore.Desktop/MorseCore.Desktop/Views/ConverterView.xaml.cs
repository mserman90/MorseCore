using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using MorseCore.AppCore;
using MorseCore.Desktop.ViewModels;
using MorseCore.Interop;
using Windows.System;

namespace MorseCore.Desktop.Views;

public sealed partial class ConverterView : UserControl
{
    private ConverterViewModel? _viewModel;
    private readonly FontFamily _monoFont = new(OperatingSystem.IsLinux() ? "monospace" : "Consolas");

    public ConverterView()
    {
        InitializeComponent();

        var shortcut = new KeyboardAccelerator { Key = VirtualKey.Enter, Modifiers = VirtualKeyModifiers.Control };
        shortcut.Invoked += async (_, e) =>
        {
            e.Handled = true;
            if (_viewModel != null) await _viewModel.ConvertNowAsync();
        };
        InputEditor.KeyboardAccelerators.Add(shortcut);
    }

    public ConverterViewModel? ViewModel
    {
        get => _viewModel;
        set
        {
            if (ReferenceEquals(_viewModel, value)) return;
            if (_viewModel != null) _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            _viewModel = value;
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged += OnViewModelPropertyChanged;
                UpdateFullView();
            }
        }
    }

    public TextBox InputBox => InputEditor;
    public TextBox OutputBox => OutputEditor;
    public Button SwapButton => SwapActionButton;
    public TextBlock StatisticsBlock => StatisticsDisplay;
    public StackPanel ResultActionsPanel => ResultActionsBar;

    public void ApplyLocalizations(Localizer text, Preferences preferences)
    {
        TextToMorseLabel.Text = text["textToMorse"];
        MorseToTextLabel.Text = text["morseToText"];
        InputHeaderLabel.Text = text[_viewModel?.IsDecoding == true ? "morseInput" : "plainText"];
        OutputHeaderLabel.Text = text[_viewModel?.IsDecoding == true ? "decodedText" : "morseInput"];
        CopyLabel.Text = text["copy"];
        SaveLabel.Text = text["save"];
        AudioLabel.Text = text["uxOpenAudio"];
        ConvertActionLabel.Text = text["convert"];
        SwapActionLabel.Text = text["swap"];
        NotesHeaderLabel.Text = text["conversionNotes"];
        NotesContentLabel.Text = text["normalizationNote"];

        AlphabetSelector.Items.Clear();
        foreach (var alphabet in Enum.GetValues<MorseAlphabet>())
        {
            AlphabetSelector.Items.Add(text["alphabet_" + alphabet]);
        }
        AlphabetSelector.SelectedIndex = (int)preferences.Alphabet;

        UpdateDirectionAndFonts(preferences);
    }

    private void UpdateDirectionAndFonts(Preferences preferences)
    {
        bool isDecoding = _viewModel?.IsDecoding ?? false;
        var plainDir = preferences.Alphabet == MorseAlphabet.Arabic ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

        InputEditor.FlowDirection = isDecoding ? FlowDirection.LeftToRight : plainDir;
        OutputEditor.FlowDirection = isDecoding ? plainDir : FlowDirection.LeftToRight;

        InputEditor.FontFamily = isDecoding ? _monoFont : (FontFamily)Application.Current.Resources["BodyFont"];
        OutputEditor.FontFamily = isDecoding ? (FontFamily)Application.Current.Resources["BodyFont"] : _monoFont;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ConverterViewModel.SourceText):
                if (InputEditor.Text != _viewModel?.SourceText)
                    InputEditor.Text = _viewModel?.SourceText ?? string.Empty;
                break;
            case nameof(ConverterViewModel.OutputText):
                OutputEditor.Text = _viewModel?.OutputText ?? string.Empty;
                break;
            case nameof(ConverterViewModel.HasResult):
                UpdateResultVisibility();
                break;
            case nameof(ConverterViewModel.StatisticsText):
                StatisticsDisplay.Text = _viewModel?.StatisticsText ?? "Ctrl + Enter";
                break;
            case nameof(ConverterViewModel.IsDecoding):
                UpdateDirectionUI();
                break;
        }
    }

    private void UpdateFullView()
    {
        if (_viewModel == null) return;
        InputEditor.Text = _viewModel.SourceText;
        OutputEditor.Text = _viewModel.OutputText;
        InputEditor.PlaceholderText = _viewModel.PlaceholderText;
        StatisticsDisplay.Text = _viewModel.StatisticsText;
        UpdateDirectionUI();
        UpdateResultVisibility();
    }

    private void UpdateDirectionUI()
    {
        if (_viewModel == null) return;
        bool isDecoding = _viewModel.IsDecoding;
        InputHeaderLabel.Text = _viewModel.InputHeader;
        OutputHeaderLabel.Text = _viewModel.OutputHeader;
        InputEditor.PlaceholderText = _viewModel.PlaceholderText;

        TextToMorseButton.Style = !isDecoding
            ? (Style)Application.Current.Resources["AccentButtonStyle"]
            : (Style)Application.Current.Resources["DefaultButtonStyle"];
        MorseToTextButton.Style = isDecoding
            ? (Style)Application.Current.Resources["AccentButtonStyle"]
            : (Style)Application.Current.Resources["DefaultButtonStyle"];
    }

    private void UpdateResultVisibility()
    {
        bool hasResult = _viewModel?.HasResult ?? false;
        ResultActionsBar.Visibility = hasResult ? Visibility.Visible : Visibility.Collapsed;
        SwapActionButton.Visibility = hasResult ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnToolbarSizeChanged(object sender, SizeChangedEventArgs e)
    {
        bool narrow = e.NewSize.Width < 570;
        Grid.SetRow(AlphabetSelector, narrow ? 1 : 0);
        Grid.SetColumn(AlphabetSelector, narrow ? 0 : 1);
        Grid.SetColumnSpan(AlphabetSelector, narrow ? 2 : 1);
        Grid.SetColumnSpan(DirectionPanel, narrow ? 2 : 1);
    }

    private void OnEditorsSizeChanged(object sender, SizeChangedEventArgs e)
    {
        bool narrow = e.NewSize.Width < 530;
        Grid.SetColumn(OutputPanel, narrow ? 0 : 1);
        Grid.SetRow(OutputPanel, narrow ? 1 : 0);
        Grid.SetColumnSpan(InputPanel, narrow ? 2 : 1);
        Grid.SetColumnSpan(OutputPanel, narrow ? 2 : 1);
    }

    private void OnDirectionTextToMorse(object sender, RoutedEventArgs e)
    {
        if (_viewModel != null) _viewModel.IsDecoding = false;
    }

    private void OnDirectionMorseToText(object sender, RoutedEventArgs e)
    {
        if (_viewModel != null) _viewModel.IsDecoding = true;
    }

    private void OnAlphabetChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_viewModel != null && AlphabetSelector.SelectedIndex >= 0)
        {
            _viewModel.SelectedAlphabet = (MorseAlphabet)AlphabetSelector.SelectedIndex;
        }
    }

    private void OnInputTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_viewModel != null && _viewModel.SourceText != InputEditor.Text)
        {
            _viewModel.SourceText = InputEditor.Text;
        }
    }

    private void OnClearClicked(object sender, RoutedEventArgs e) => _viewModel?.Clear();

    private void OnCopyClicked(object sender, RoutedEventArgs e) => _viewModel?.CopyCommand.Execute(null);

    private async void OnSaveClicked(object sender, RoutedEventArgs e)
    {
        if (_viewModel != null) await _viewModel.SaveCommand.ExecuteAsync(null);
    }

    private void OnAudioClicked(object sender, RoutedEventArgs e) => _viewModel?.OpenAudioCommand.Execute(null);

    private async void OnConvertClicked(object sender, RoutedEventArgs e)
    {
        if (_viewModel != null) await _viewModel.ConvertNowAsync();
    }

    private void OnSwapClicked(object sender, RoutedEventArgs e) => _viewModel?.Swap();
}

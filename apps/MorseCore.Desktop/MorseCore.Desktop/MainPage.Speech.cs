using MorseCore.AppCore;
using MorseCore.Interop;

namespace MorseCore.Desktop;

public sealed partial class MainPage
{
    private bool speaking;
    private CancellationTokenSource? speechRequest;
    private Button? speechPlayButton,speechStopButton;
    private bool speechVoiceInstalled;
    private void UpdateSpeechActions()
    {
        if(speechPlayButton!=null) speechPlayButton.IsEnabled=!speaking && speechVoiceInstalled && !string.IsNullOrWhiteSpace(audioSource);
        if(speechStopButton!=null) speechStopButton.Visibility=speaking?Visibility.Visible:Visibility.Collapsed;
    }
    private void BuildSpeech(Func<Task<string>> getText)
    {
        var stack=new StackPanel { Spacing=10 };
        var heading=Row(); heading.Children.Add(new FontIcon { Glyph="\uE767",FontSize=18,Foreground=Brush("AccentInkBrush") }); heading.Children.Add(Label(T("speech"),18));
        heading.Children.Add(Label(T("offlineSpeech"),12,true)); stack.Children.Add(heading);
        if(!SpeechService.IsSupported) { stack.Children.Add(Label(T("speechUnavailable"),13,true)); PageContent.Children.Add(Card(stack)); return; }
        string[] cultures=["tr-TR","en-US","de-DE","ru-RU","ar-SA"];
        var choices=new Grid { ColumnSpacing=12,RowSpacing=10 };
        for(int i=0;i<3;i++) choices.ColumnDefinitions.Add(new ColumnDefinition { Width=i<2 ? new GridLength(1,GridUnitType.Star) : GridLength.Auto });
        for(int i=0;i<3;i++) choices.RowDefinitions.Add(new RowDefinition { Height=GridLength.Auto });
        var language=Choice(T("speechLanguage"),Localizer.LanguageNames,Array.IndexOf(cultures,preferences.SpeechLanguage),_=> { }); language.MinWidth=0;
        var voices=new ComboBox { Header=T("voice"),MinWidth=0,HorizontalAlignment=HorizontalAlignment.Stretch };
        Grid.SetColumn(voices,1); choices.Children.Add(language); choices.Children.Add(voices);
        string home=OfflineSpeech.ResolveHome(preferences.SpeechHome);
        var info=Label("",12,true);
        void SelectVoices()
        {
            var available=OfflineSpeech.ForLanguage(preferences.SpeechLanguage);
            voices.ItemsSource=available.Select(v=>new ComboBoxItem { Content=v.Name,Tag=v }).ToArray();
            int index=available.ToList().FindIndex(v=>v.Id==preferences.VoiceId);
            voices.SelectedIndex=Math.Max(0,index);
        }
        voices.SelectionChanged+=(_,_)=>
        {
            if(voices.SelectedItem is not ComboBoxItem { Tag: OfflineVoice voice }) return;
            preferences=preferences with { VoiceId=voice.Id }; SavePreferences();
            info.Text=OfflineSpeech.IsInstalled(home,voice) ? T(voice.Language=="ar-SA" ? "arabicVoiceNote" : "speechLocalNote") : T("speechPackMissing");
            speechVoiceInstalled=OfflineSpeech.IsInstalled(home,voice); UpdateSpeechActions();
        };
        language.SelectionChanged+=(_,_)=>
        {
            if(language.SelectedIndex<0 || preferences.SpeechLanguage==cultures[language.SelectedIndex]) return;
            preferences=preferences with { SpeechLanguage=cultures[language.SelectedIndex],VoiceId="" }; SelectVoices();
        };
        SelectVoices();
        var speak=Action("speak",()=>SpeakText(getText),"\uE768",true);
        speechPlayButton=speak;
        var actions=Row(); actions.Spacing=4; actions.VerticalAlignment=VerticalAlignment.Bottom;
        speechStopButton=Action("stop",()=> { StopPlayback(); return Task.CompletedTask; },"\uE71A");
        actions.Children.Add(speak); actions.Children.Add(speechStopButton); UpdateSpeechActions();
        Grid.SetColumn(actions,2); choices.Children.Add(actions);
        choices.SizeChanged+=(_,e)=>
        {
            bool narrow=e.NewSize.Width<590;
            Grid.SetColumnSpan(language,narrow?3:1); Grid.SetRow(voices,narrow?1:0); Grid.SetColumn(voices,narrow?0:1); Grid.SetColumnSpan(voices,narrow?2:1);
            Grid.SetRow(actions,narrow?1:0);
        };
        stack.Children.Add(choices); stack.Children.Add(info);
        stack.Children.Add(Action("voicesManage",()=> { audioWorkspace=2; BuildPage(); return Task.CompletedTask; },"\uE713"));
        PageContent.Children.Add(Card(stack));
    }
    private async Task SpeakText(Func<Task<string>> getText)
    {
        if(speaking) return;
        speaking=true;
        UpdateSpeechActions();
        try
        {
            StopPlayback();
            using var cancellation=CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token); speechRequest=cancellation;
            var options=preferences;
            string home=OfflineSpeech.ResolveHome(options.SpeechHome);
            var voice=OfflineSpeech.Voices.FirstOrDefault(v=>v.Id==options.VoiceId);
            if(voice==null || !OfflineSpeech.IsInstalled(home,voice)) { SetStatus(T("speechPackMissing")); return; }
            string value=await getText();
            cancellation.Token.ThrowIfCancellationRequested();
            if(string.IsNullOrWhiteSpace(value)) { SetStatus(T("missingInput")); return; }
            if(value.Length>OfflineSpeech.MaxCharacters) { SetStatus(T("speechLimit")); return; }
            SetStatus(T("speechPreparing")); speech ??=new SpeechService();
            await speech.SpeakAsync(value,voice.Id,home,cancellation.Token,muted:SmokeDirectory!=null); SetStatus(T("ready"));
        }
        catch(IOException) { SetStatus(T("speechError")); }
        finally { speechRequest=null; speaking=false; UpdateSpeechActions(); }
    }

    private SpeechToMorseService? speechToMorseService;
    private bool speechToMorseListening;
    private string speechToMorseLang = "tr-TR";

    private void BuildSpeechToMorseSection()
    {
        var stack = new StackPanel { Spacing = 16 };
        var heading = Row();
        heading.Children.Add(new FontIcon { Glyph = "\uE720", FontSize = 20, Foreground = Brush("AccentInkBrush") });
        heading.Children.Add(Label(T("speechToMorseTitle"), 18));
        heading.Children.Add(Label(T("speechToMorseIntro"), 12, true));
        stack.Children.Add(heading);

        speechToMorseService ??= new SpeechToMorseService();

        var configRow = Row();
        configRow.Spacing = 16;
        var langChoices = new[] { "Türkçe (tr-TR)", "English (en-US)" };
        var langValues = new[] { "tr-TR", "en-US" };
        var langSelect = Choice(T("speechLanguage"), langChoices, Array.IndexOf(langValues, speechToMorseLang), i =>
        {
            if (i >= 0 && i < langValues.Length) speechToMorseLang = langValues[i];
        });
        configRow.Children.Add(langSelect);
        stack.Children.Add(configRow);

        var spokenBox = new TextBox
        {
            Header = T("spokenTextHeader"),
            PlaceholderText = T("spokenTextPlaceholder"),
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            Height = 110,
            FontSize = 16
        };

        var morseBox = new TextBox
        {
            Header = T("speechMorseOutputHeader"),
            PlaceholderText = "... --- ...",
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            Height = 110,
            FontFamily = MonoFont,
            FontSize = 18
        };

        var statusInfo = Label(T("speechStatusReady"), 12, true);

        void UpdateTranscription(string text)
        {
            _ = DispatcherQueue.TryEnqueue(() =>
            {
                spokenBox.Text = text;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    morseBox.Text = MorseEngine.Encode(text, preferences.Alphabet, preferences.Separator).Text;
                }
                else
                {
                    morseBox.Text = "";
                }
            });
        }

        spokenBox.TextChanged += (_, _) =>
        {
            if (!speechToMorseListening && !string.IsNullOrWhiteSpace(spokenBox.Text))
            {
                morseBox.Text = MorseEngine.Encode(spokenBox.Text, preferences.Alphabet, preferences.Separator).Text;
            }
        };

        Button? startLiveBtn = null;
        Button? stopLiveBtn = null;

        startLiveBtn = Action("startLiveSpeech", async () =>
        {
            if (speechToMorseListening) return;
            speechToMorseListening = true;
            startLiveBtn!.IsEnabled = false;
            stopLiveBtn!.IsEnabled = true;
            statusInfo.Text = T("speechStatusListening");
            SetStatus(T("speechStatusListening"));

            try
            {
                await speechToMorseService.StartLiveListeningAsync(
                    speechToMorseLang,
                    onPartialResult: partial => UpdateTranscription(partial),
                    onFinalResult: final => UpdateTranscription(final),
                    onError: err =>
                    {
                        _ = DispatcherQueue.TryEnqueue(() =>
                        {
                            statusInfo.Text = err;
                            SetStatus(err);
                        });
                    },
                    cancellationToken: lifetime.Token);
            }
            catch (Exception ex)
            {
                statusInfo.Text = ex.Message;
            }
        }, "\uE720", true);

        stopLiveBtn = Action("stopLiveSpeech", async () =>
        {
            if (!speechToMorseListening) return;
            await speechToMorseService.StopLiveListeningAsync();
            speechToMorseListening = false;
            startLiveBtn!.IsEnabled = true;
            stopLiveBtn!.IsEnabled = false;
            statusInfo.Text = T("speechStatusStopped");
            SetStatus(T("speechStatusStopped"));
        }, "\uE71A");
        stopLiveBtn.IsEnabled = false;

        var pickFileBtn = Action("transcribeFile", async () =>
        {
            if (speechToMorseListening) return;
            string? filePath = await PickOpenPath(["wav", "mp3", "m4a", "wma"]);
            if (filePath == null) return;

            statusInfo.Text = T("transcribingFile");
            SetStatus(T("transcribingFile"));
            try
            {
                string transcribed = await speechToMorseService.TranscribeAudioFileAsync(
                    filePath,
                    speechToMorseLang,
                    lifetime.Token);

                UpdateTranscription(transcribed);
                statusInfo.Text = T("transcriptionComplete");
                SetStatus(T("transcriptionComplete"));
            }
            catch (Exception ex)
            {
                statusInfo.Text = ex.Message;
                SetStatus(ex.Message);
            }
        }, "\uE8B7");

        var playMorseBtn = Action("playMorseFromSpeech", () =>
        {
            if (!string.IsNullOrWhiteSpace(morseBox.Text))
            {
                PlayMorse(morseBox.Text);
            }
            return Task.CompletedTask;
        }, "\uE768");

        var copyMorseBtn = Action("copy", () =>
        {
            if (!string.IsNullOrWhiteSpace(morseBox.Text))
            {
                ClipboardSet(morseBox.Text);
                SetStatus(T("copied"));
            }
            return Task.CompletedTask;
        }, "\uE8C8");

        var copyTextBtn = Action("copySpokenText", () =>
        {
            if (!string.IsNullOrWhiteSpace(spokenBox.Text))
            {
                ClipboardSet(spokenBox.Text);
                SetStatus(T("copied"));
            }
            return Task.CompletedTask;
        }, "\uE8C8");

        var rigTransmitBtn = Action("transmitToRig", async () =>
        {
            if (string.IsNullOrWhiteSpace(morseBox.Text)) return;
            if (serialKeyer == null || !serialKeyer.IsOpen)
            {
                SetStatus(T("rigNotConnected"));
                return;
            }
            SetStatus(T("rigTransmitting"));
            await serialKeyer.TransmitMorseAsync(morseBox.Text, (int)preferences.Wpm, lifetime.Token);
            SetStatus(T("ready"));
        }, "\uE710");

        var buttonBar = Row();
        buttonBar.Children.Add(startLiveBtn);
        buttonBar.Children.Add(stopLiveBtn);
        buttonBar.Children.Add(pickFileBtn);
        buttonBar.Children.Add(playMorseBtn);
        buttonBar.Children.Add(copyMorseBtn);
        buttonBar.Children.Add(copyTextBtn);
        buttonBar.Children.Add(rigTransmitBtn);

        stack.Children.Add(buttonBar);
        stack.Children.Add(spokenBox);
        stack.Children.Add(morseBox);
        stack.Children.Add(statusInfo);

        PageContent.Children.Add(Card(stack));
    }
}

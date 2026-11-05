using MorseCore.AppCore.Radio;
using Microsoft.UI.Xaml.Media;

namespace MorseCore.Desktop;

public sealed partial class MainPage
{
    private string glossaryQuery="";
    private int glossaryCategory;
    private void BuildGlossary()
    {
        Heading(T("glossary"),T("glossaryHint"));
        var filters=new StackPanel { Spacing=12 };
        var search=new TextBox { Header=T("searchTerms"),Text=glossaryQuery,MaxLength=80 };
        string[] categories=["allTerms","qcodes","abbreviations","prosigns","phonetics"];
        var choice=Choice(T("category"),categories.Select(T),glossaryCategory,_=> { }); filters.Children.Add(Columns(260,2,search,choice));
        PageContent.Children.Add(Card(filters));
        var entries=new StackPanel { Spacing=8 }; PageContent.Children.Add(entries);
        void Refresh()
        {
            entries.Children.Clear(); var cards=new List<FrameworkElement>(); string query=glossaryQuery.Trim();
            foreach(var term in CwLearning.Terms.Where(t=>(glossaryCategory==0 || t.Category==categories[glossaryCategory]) && (t.Code.Contains(query,StringComparison.OrdinalIgnoreCase) || T("term_"+t.Key).Contains(query,StringComparison.CurrentCultureIgnoreCase))))
            {
                var body=new StackPanel { Spacing=6 };
                var header=Row(); var code=Label(term.Code,18); code.FontFamily=MonoFont; code.FlowDirection=FlowDirection.LeftToRight;
                header.Children.Add(code); header.Children.Add(IconAction("play",()=>PlayMorse(CwMessages.Encode(term.Code),'/'),"\uE768"));
                body.Children.Add(header); body.Children.Add(Label(T("term_"+term.Key),14));
                var example=Label(term.Example,13,true); example.FlowDirection=FlowDirection.LeftToRight; body.Children.Add(example); cards.Add(Card(body));
            }
            if(glossaryCategory is 0 or 4)
                for(int i=0;i<CwLearning.Phonetics.Count;i++)
                {
                    string value=$"{(char)('A'+i)} · {CwLearning.Phonetics[i]}";
                    if(!value.Contains(query,StringComparison.OrdinalIgnoreCase)) continue;
                    var label=Label(value,15); label.FlowDirection=FlowDirection.LeftToRight; cards.Add(Card(label));
                }
            if(cards.Count==0) entries.Children.Add(Label(T("noResults"),14,true));
            else entries.Children.Add(Columns(320,2,cards.ToArray()));
        }
        search.TextChanged+=(_,_)=> { glossaryQuery=search.Text; Refresh(); };
        search.TextChanged+=(_,_)=> { glossaryQuery=search.Text; Refresh(); };
        choice.SelectionChanged+=(_,_)=> { glossaryCategory=choice.SelectedIndex; Refresh(); }; Refresh();
    }

    private string guideQuery = "";

    private void BuildUserGuide()
    {
        Heading(T("userGuide"), T("userGuideHint"));

        var searchBox = new TextBox
        {
            Header = T("searchTerms"),
            PlaceholderText = "Örn: konuşma, telsiz, decoder, WPM, wav...",
            Text = guideQuery,
            MaxLength = 80
        };
        PageContent.Children.Add(Card(searchBox));

        var entries = new StackPanel { Spacing = 14 };
        PageContent.Children.Add(entries);

        var guideSections = new (string icon, string title, string summary, string[] steps, string tips)[]
        {
            (
                "\uE8D4",
                "1. Çift Yönlü Çevirici (Metin ↔ Mors Kodu)",
                "Uluslararası ITU-R M.1677-1 standardına göre düz metni Mors koduna, Mors kodunu ise metne iki yönlü olarak anında çevirir.",
                [
                    "Giriş kutusuna metin veya Mors kodu (nokta ve çizgilerle) yazın.",
                    "Hız (WPM: 5-60) ve Ton Frekansı (Hz: 200-2000) ayarlarını ihtiyacınıza göre düzenleyin.",
                    "'Dinle' (Oynat) butonuna basarak CW ritmini dinleyin veya 'WAV Kaydet' ile ses dosyası olarak indirin.",
                    "Alfabe seçiminden Latin, Arapça veya Kiril Mors karakter tablolarına geçiş yapabilirsiniz."
                ],
                "İpucu: Kelimeler arasına eğik çizgi ('/') veya boşluk koyarak Mors kodunu kolayca ayrıştırabilirsiniz."
            ),
            (
                "\uE720",
                "2. Konuşmayı Mors'a Çevirme (Speech-to-Morse)",
                "Mikrofondan konuşulan sözcükleri veya yüklenen ses dosyalarını dinleyerek anında Mors koduna dönüştürür.",
                [
                    "Dil seçiminden 'Türkçe (tr-TR)' veya 'English (en-US)' dilini belirleyin.",
                    "'Dinlemeyi Başlat' butonuna tıklayarak mikrofona konuşun; konuştuğunuz kelimeler gerçek zamanlı olarak Mors koduna çevrilir.",
                    "Alternatif olarak 'Ses Dosyası Seç (.wav/.mp3)' ile kayıtlı konuşma sesinizi yükleyip transkribe edin.",
                    "Oluşan Mors kodunu 'Mors Dinle' butonuyla dinleyin veya 'Telsize Gönder' ile doğrudan CW vericinize aktarın."
                ],
                "İpucu: Gürültülü ortamlarda mikrofon seviyesini ayarlayarak daha net transkripsiyon elde edebilirsiniz."
            ),
            (
                "\uE768",
                "3. Mors Kodundan Konuşmaya (Morse-to-Speech / TTS)",
                "Yüklenen veya yazılan Mors kodunu çözerek Türkçe veya İngilizce doğal konuşma sesine (TTS) dönüştürür.",
                [
                    "Mors kodunu kutucuğa yapıştırın veya 'Mors Ses Dosyası Seç (.wav)' butonuna basarak ses kaydınızı yükleyin.",
                    "Yüklenen Mors sesi DSP analizinden geçirilerek otomatik metne dökülür.",
                    "Konuşma dilini ('Türkçe' veya 'English') seçin.",
                    "'Konuşmayı Başlat' butonuna tıklayarak çözümlenen metni sesli olarak dinleyin veya 'WAV Kaydet' ile konuşma sesini dışa aktarın."
                ],
                "İpucu: Windows yerel ses motoru ve ek offline Piper yapay zeka modelleriyle internet bağlantısı olmadan çalışır."
            ),
            (
                "\uE8D6",
                "4. Gerçek Zamanlı Akışlı CW Çözücü (Streaming DSP Decoder)",
                "Telsiz hoparlöründen veya mikrofon girişinden gelen gerçek zamanlı CW ses sinyallerini Goertzel algoritmasıyla deşifre eder.",
                [
                    "Ses çalışma alanından 'Akışlı Çözücü' (Streaming Decoder) sekmesine geçin.",
                    "Giriş aygıtı olarak telsiz ses kartınızı veya mikrofonunuzu seçin.",
                    "'Canlı Akış Çözümü' başlatıldığında sinyal enerjisi ve zamanlama eşikleri adaptif olarak hesaplanır.",
                    "Alınan karakterler ekranda anlık olarak belirecektir."
                ],
                "İpucu: Otomatik ton ve hız algılamasını açarak istasyonun ton frekansına kendini kilitlemesini sağlayabilirsiniz."
            ),
            (
                "\uE704",
                "5. Amatör Telsiz Entegrasyonu (Serial Rig Keyer)",
                "USB-Seri dönüştürücüler (FTDI, CH340, CP2102) aracılığıyla telsizinizi doğrudan anahtarlar.",
                [
                    "Telsizinizin CW/Key girişine bağlı seri portu (COM) seçin.",
                    "Anahtarlama pinini (DTR veya RTS) donanım devrenize göre belirleyin.",
                    "Metin veya Mors kodunu 'Telsize Gönder' butonuyla tam WPM zamanlamasında telsiz üzerinden yayına verin."
                ],
                "İpucu: Verici anahtarlaması yaparken donanımınızda optokuplör izolasyonu kullanılması tavsiye edilir."
            ),
            (
                "\uE7BE",
                "6. Çağrı İşareti Sorgulama & QSO Görüşme Günlüğü",
                "Amatör telsiz çağrı işaretlerini ITU standartlarına göre çözümler ve ADIF formatında görüşme kaydı tutar.",
                [
                    "Çağrı işareti sorgulama kutusuna çağrı işaretini yazarak (örn: TA1MC, K1ABC, DL2XYZ) ülkesini, ITU bölgesini ve kıtasını öğrenin.",
                    "Görüşme Asistanı ile RST sinyal raporunu, frekansı ve QSO detaylarını girin.",
                    "Görüşme günlüğünü standart ADIF formatında dışa aktararak LoTW, eQSL veya QRZ.com gibi platformlara aktarabilirsiniz."
                ],
                "İpucu: İstasyon profilleri sekmesinden kendi çağrı işaretinizi ve ruhsat sınıfınızı önceden kaydedebilirsiniz."
            ),
            (
                "\uE719",
                "7. CW Alıştırma Manevrası & Q-Kodları Kütüphanesi",
                "Mors ritminizi ve telsizcilik jargonu bilginizi pekiştirmeniz için interaktif pratik araçları.",
                [
                    "Manevra Pratiği (Keying) sekmesinde klavyeden Boşluk (Spacebar) tuşuna basılı tutarak manevra yapın.",
                    "Sözlük (Glossary) sekmesinde amatör telsizcilikte en çok kullanılan Q-Kodlarını (QTH, QSO, QSL, QRZ) ve işletme işaretlerini (<AR>, <SK>, <BT>) arayın ve seslerini dinleyin."
                ],
                "İpucu: Düzenli manevra alıştırması yapmak dit/dah oranınızı (1:3) ve harf arası boşluk disiplininizi geliştirir."
            )
        };

        void RenderGuide()
        {
            entries.Children.Clear();
            string q = guideQuery.Trim();

            var filtered = guideSections.Where(s =>
                string.IsNullOrEmpty(q) ||
                s.title.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                s.summary.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                s.tips.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                s.steps.Any(st => st.Contains(q, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            if (filtered.Count == 0)
            {
                entries.Children.Add(Label(T("noResults"), 14, true));
                return;
            }

            foreach (var sec in filtered)
            {
                var cardStack = new StackPanel { Spacing = 10 };

                var titleRow = Row();
                titleRow.Children.Add(new FontIcon { Glyph = sec.icon, FontSize = 20, Foreground = Brush("AccentInkBrush") });
                titleRow.Children.Add(Label(sec.title, 16));
                cardStack.Children.Add(titleRow);

                cardStack.Children.Add(Label(sec.summary, 13, true));

                var stepsStack = new StackPanel { Spacing = 6, Margin = new Thickness(6, 4, 0, 4) };
                for (int i = 0; i < sec.steps.Length; i++)
                {
                    var stepLabel = Label($"• {sec.steps[i]}", 13);
                    stepsStack.Children.Add(stepLabel);
                }
                cardStack.Children.Add(stepsStack);

                var tipLabel = Label($"💡 {sec.tips}", 12, true);
                tipLabel.Foreground = Brush("AccentInkBrush");
                cardStack.Children.Add(tipLabel);

                entries.Children.Add(Card(cardStack));
            }
        }

        searchBox.TextChanged += (_, _) =>
        {
            guideQuery = searchBox.Text;
            RenderGuide();
        };

        RenderGuide();
    }
    }
}

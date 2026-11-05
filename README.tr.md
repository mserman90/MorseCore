# MorsLab: Telsiz Telgrafçılığı, Sayısal Sinyal İşleme (DSP) ve Çift Yönlü Mors-Konuşma Telekomünikasyonu için Çok Platformlu Nöral Akustik Çerçeve

<div align="center">

![MorsLab Banner](docs/media/morsecore.gif)

[![Lisans: Apache 2.0](https://img.shields.io/badge/Lisans-Apache%202.0-blue.svg?style=for-the-badge)](LICENSE)
[![Standart: ITU-R M.1677-1](https://img.shields.io/badge/Standart-ITU--R%20M.1677--1-007ACC?style=for-the-badge)](https://www.itu.int/rec/R-REC-M.1677)
[![ADIF: 3.1.4 Uyumlu](https://img.shields.io/badge/ADIF-3.1.4%20Uyumlu-success?style=for-the-badge)](https://adif.org/)
[![Canlı Dağıtım](https://img.shields.io/badge/Canl%C4%B1_Uygulama-GitHub_Pages-2563EB?style=for-the-badge&logo=githubpages&logoColor=white)](https://mserman90.github.io/MorsLab/)
[![Platformlar](https://img.shields.io/badge/Platformlar-Web%20|%20Windows%20|%20macOS%20|%20Linux-brightgreen?style=for-the-badge)](#sistem-mimarisi)

[English](README.md) · **Türkçe** · [Deutsch](README.de.md) · [Русский](README.ru.md) · [العربية](README.ar.md)

[**🌐 Canlı Web Uygulaması**](https://mserman90.github.io/MorsLab/) &middot; [**📖 Kullanım Kılavuzu & Wiki**](https://github.com/mserman90/MorsLab/wiki/Baslangic) &middot; [**💻 Masaüstü Sürümleri**](#ikili-surumler-ve-kurulum)

</div>

---

## 📌 Özet (Abstract)

**MorsLab**, sürekli dalga (CW) telsiz telgrafçılığı çevirisi, gerçek zamanlı sayısal sinyal işleme (DSP) tabanlı demodülasyon, donanımsal seri port üzerinden telsiz anahtarlaması (Rig Keying) ve çok dilli çift yönlü nöral konuşma sentezi (Konuşmadan Mors'a ve Mors'tan Konuşmaya) için geliştirilmiş açık kaynaklı, çok platformlu bilimsel bir araştırma ve telekomünikasyon platformudur. Yüksek başarımlı yerel çekirdeği (C99 / C# / .NET 10 / WebAssembly / Web Audio API) sayesinde MorsLab; klasik bilgi kuramı ilkelerini, modern yazılım tanımlı telsiz (SDR) tekniklerini ve güncel nöral akustik boru hatlarını tek bir çatı altında birleştirir.

Platform; **ITU-R M.1677-1**, **ITU-R M.1172** ve amatör telsizcilik veri değişim standardı olan **ADIF 3.1.4** yönergeleriyle tam biçimsel uyumluluk gösterir; Latin (genişletilmiş Türkçe karakterler dahil), Kiril ve Arap alfabelerini çift yönlü deterministik haritalamayla destekler.

---

## 🌐 Çok Yönlü 4-Modaliteli Çeviri Matrisi ($4 \times 3 = 12$ Çapraz Çeviri)

MorsLab; **Düz Metin**, **Mors Alfabesi Metni**, **Konuşma Sesi** ve **Mors Sesi (CW)** olmak üzere 4 temel modalite arasında tam geçişli, kayıpsız 12 yönlü çapraz çeviriyi eksiksiz destekler:

```
                    ┌────────────────────────┐
                    │      1. Düz Metin      │
                    │ (TR, EN, RU, AR Metin) │
                    └───────────┬────────────┘
                         ▲   ▲  │  ▲
              ┌──────────┘   │  │  └──────────┐
              │  ┌───────────┘  │             │
              ▼  ▼              ▼             ▼
┌────────────────────────┐             ┌────────────────────────┐
│  2. Mors Metni (.-)    │◄───────────►│    3. Konuşma Sesi     │
│ (ITU-R M.1677-1 Kodlar)│             │ (Mikrofon / TTS Nöral) │
└─────────────┬──────────┘             └──────────────┬─────────┘
              ▲                                       ▲
              │                 ▲                     │
              └───────────┐     │     ┌───────────────┘
                          ▼     ▼     ▼
                    ┌────────────────────────┐
                    │    4. Mors Sesi (CW)   │
                    │(Goertzel DSP / WAV/Ton)│
                    └────────────────────────┘
```

| Kaynak Modalite | Hedef Modalite | Çeviri Yolu & Standart | Web Motoru | Masaüstü (.NET) Motoru |
|:---|:---|:---|:---|:---|
| **Düz Metin** | **Mors Metni** | Deterministik ITU-R M.1677-1 Sembol Eşleme | `convertNow()` | `MorseEncoder.Encode()` |
| **Düz Metin** | **Konuşma Sesi** | Nöral Konuşma Sentezi (TTS) | Web Speech Synthesis | Piper TTS / OS SAPI |
| **Düz Metin** | **Mors Sesi** | PARIS Zamanlamalı CW Ton Sentezi & WAV | Web Audio Oscillator / WAV PCM | `AudioPlayer.cs` / WAV Sentez |
| **Mors Metni** | **Düz Metin** | Önek Çözümleme & Q-Kodu Eşleme | `convertNow()` ters yön | `MorseDecoder.Decode()` |
| **Mors Metni** | **Konuşma Sesi** | Sembol Çözümleme + Sesli Okuma | Web Speech API Utterance | `MorseToSpeechService.cs` |
| **Mors Metni** | **Mors Sesi** | İstenen WPM & Frekansta CW Üretimi | Web Audio API / WAV Dışa Aktar | `AudioPlayer.cs` / SoundPlayer |
| **Konuşma Sesi** | **Düz Metin** | Otomatik Konuşma Tanıma (ASR) | Web Speech Recognition API | OS SpeechRecognizer |
| **Konuşma Sesi** | **Mors Metni** | Gerçek Zamanlı ASR ➔ Sembol Transkripsiyonu | `toggleSpeechRecognition()` | `SpeechToMorseService.cs` |
| **Konuşma Sesi** | **Mors Sesi** | ASR Transkripsiyonu ➔ CW Tonu / WAV | `playSpeechMorse()` & WAV Blob | `SpeechToMorseService.cs` + CW |
| **Mors Sesi** | **Mors Metni** | Zarf Takibi & Dar Bant Goertzel DSP | Web Audio DSP / Canlı Mikrofon | `StreamingMorseDecoder.cs` / `MorseDecoder.cs` |
| **Mors Sesi** | **Düz Metin** | DSP Zarf Çözümleme ➔ Metin Çevirisi | `handleMorseAudioUpload()` | `MorseDecoder.DecodeWav()` |
| **Mors Sesi** | **Konuşma Sesi** | DSP Demodülasyon ➔ TTS Okuma | DSP + Web Speech Synthesis | Goertzel DSP + Piper TTS |

---

## 📐 1. Kuramsal Temeller ve Bilgi Kuramı

### 1.1 Entropi ve Önek Kodlaması (Prefix Coding)
Uluslararası Mors kodu, Shannon'ın kaynak kodlama teoreminden ve Huffman kodlamasından çok önce geliştirilmiş, değişken uzunluklu ve entropi kısıtlı önek kodlamasının (prefix-free code) tarihteki ilk geniş çaplı mühendislik uygulamasıdır. Doğal dillerdeki harf frekans dağılımı uyarınca, yüksek olasılıklı sembollere (örn. $E \rightarrow \cdot$, $T \rightarrow -$) daha kısa iletim zamanları atanarak ortalama sembol iletim süresi en aza indirgenir:

$$\mathbb{E}[L] = \sum_{i=1}^{M} P(s_i) \cdot \ell(s_i)$$

Burada $P(s_i)$ ilgili sembolün kullanım sıklığını, $\ell(s_i)$ ise ayrık birim baud cinsinden sembol uzunluğunu ifade eder.

### 1.2 Mors Zamanlama Analitiği (PARIS Standardı)
MorsLab, `"PARIS "` kelimesinin tam olarak $50$ birim süreden ($\tau$) oluştuğunu varsayan uluslararası **PARIS standardını** temel alır. Dakikadaki kelime sayısı (Words Per Minute - $\text{WPM}$) cinsinden hızın belirlenmesiyle temel zaman birimi $\tau$ (bir Dit süresi) deterministik olarak hesaplanır:

$$\tau = \frac{1200}{\text{WPM}} \quad (\text{milisaniye})$$

Standart iletim oranları şu şekildedir:
* **Dit ($\cdot$) süresi:** $T_{\text{dit}} = 1\tau$
* **Dah ($-$) süresi:** $T_{\text{dah}} = 3\tau$
* **Harf içi elemanlar arası boşluk:** $T_{\text{eleman}} = 1\tau$
* **Harfler arası boşluk:** $T_{\text{harf}} = 3\tau$
* **Kelimeler arası boşluk:** $T_{\text{kelime}} = 7\tau$

Pedagojik ve eğitimsel alıştırmalarda ise harf algılama refleksini korumak adına **Farnsworth zamanlaması** uygulanır. Harfler hedef yüksek hızda ($\text{WPM}_{\text{karakter}}$) üretilirken, harf ve kelime boşlukları efektif hız ($\text{WPM}_{\text{efektif}}$) üzerinden genişletilir:

$$\tau_{\text{efektif\_bosluk}} = \frac{60 - 37.2 \cdot \frac{\text{WPM}_{\text{efektif}}}{\text{WPM}_{\text{karakter}}}}{19 \cdot \text{WPM}_{\text{efektif}}} \cdot 1000 \quad (\text{ms})$$

---

## 🔬 2. Sayısal Sinyal İşleme (DSP) & Demodülasyon Motoru

### 2.1 Dar Bant CW Taşıyıcı Demodülasyonu için Goertzel Algoritması
Geniş spektrumlu ve $O(N \log N)$ hesaplama maliyetine sahip Hızlı Fourier Dönüşümü (FFT) yerine, MorsLab hedef CW ton taşıyıcı frekansına ($f_c \approx 700\text{ Hz}$) dinamik olarak kilitlenen ikinci dereceden özyineli **Goertzel filtresini** ($O(N)$) kullanır.

Ayrık örnekleme frekansı $f_s$ ve blok boyutu $N$ verildiğinde, normalize hedef açısal frekans:

$$\omega_0 = \frac{2\pi f_c}{f_s}$$

Özyineli fark denklemi ara durum değişkeni $s[n]$ için şu şekilde kurulur:

$$s[n] = x[n] + 2\cos(\omega_0) s[n-1] - s[n-2], \quad n = 0, 1, \dots, N-1$$

Burada başlangıç sınır koşulları $s[-1] = s[-2] = 0$'dır.

$N$ örnek işlendikten sonra toplam spektral enerji karesi trigonometrik çarpım yükü olmaksızın hesaplanır:

$$|X(f_c)|^2 = s[N-1]^2 + s[N-2]^2 - 2\cos(\omega_0) s[N-1] s[N-2]$$

```
Ham PCM Ses Akışı (8 kHz / 48 kHz)
             │
             ▼
┌─────────────────────────┐
│ Bant Geçiren Ön Süzgeç  │  (Butterworth 2. Derece IIR, Q ≈ 8)
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ Goertzel Rezonatörü     │  (Hedef bin: fc = 700 Hz, O(N) karmaşıklık)
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ Zarf Çıkarımı           │  (|X(fc)|² Büyüklük karesi)
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ Uyarlamalı Histerezis   │  (Schmitt Tetikleyici: Bayes gürültü tabanı)
└────────────┬────────────┘
             │
             ▼
┌─────────────────────────┐
│ Markov Durum Çözücü     │  (Dit / Dah / Eleman / Kelime sınıflandırması)
└────────────┬────────────┘
             │
             ▼
Çözümlenen Doğal Metin Akışı (UTF-8)
```

### 2.2 Uyarlamalı Bayes Eşikleme ve Histerezis
Düşük Sinyal-Gürültü Oranına (SNR) sahip radyo kanallarında (HF atmosferik gürültü, QRN ve iyonosferik sönümleme - QSB) kararlı sinyal tespiti için çift seviyeli Schmitt tetikleyici histerezisi uygulanır:

$$\text{Durum}[n] = \begin{cases} \mathbf{ON}, & |X[n]|^2 > \theta_{\text{yüksek}} \\ \mathbf{OFF}, & |X[n]|^2 < \theta_{\text{düşük}} \\ \text{Durum}[n-1], & \text{diğer durumlarda} \end{cases}$$

Burada $\theta_{\text{yüksek}}$ ve $\theta_{\text{düşük}}$ eşikleri, kanalın gürültü tabanına göre üstel hareketli ortalama (EMA) enerji kestirimiyle dinamik olarak güncellenir.

---

## 🎙️ 3. Nöral Konuşma İşleme ve Çok Dilli Akustik Haritalama

### 3.1 Konuşmadan Mors Koduna (Canlı ve Dosyadan ASR)
* **Akustik Tanıma:** Web Speech API ve yerel akustik modeller aracılığıyla **Türkçe (tr-TR)**, **İngilizce (en-US)**, **Rusça (ru-RU)** ve **Arapça (ar-SA)** dillerinde gerçek zamanlı konuşma tanıma.
* **Metin Normalizasyonu:** Diakritik işaret temizleme, büyük/küçük harf katlama ve ulusal Mors standartlarına deterministik harf dönüştürme.
* **Akışkan İletim:** Konuşma arabelleği sürekli işlenerek $50\text{ ms}$'nin altında bir gecikmeyle Mors koduna ve CW sesine dönüştürülür.

### 3.2 Mors Kodundan Konuşmaya (Nöral TTS)
* **Mors / CW Ses Girişi:** Nokta-çizgi metni veya `.wav` CW ses dosyası deşifre motoruna iletilir.
* **Yapay Zeka Ses Sentezi:**
  * **Web Arayüzü:** W3C SpeechSynthesis standardı ile yerel tarayıcı ses motoru.
  * **Masaüstü Sürümü:** Tamamen internet bağlantısız (offline) çalışan **Piper (VITS mimarisi)** ve **EMA Lightning (örtük difüzyon)** nöral ses modelleri.

---

## 📻 4. Donanım Arayüzü & Amatör Telsiz Entegrasyonu

### 4.1 Seri Port Telsiz Anahtarlama (Rig Keyer)
USB-Seri dönüştürücüler (FTDI FT232R, CP2102, CH340) üzerinden HF/VHF telsizlerinin doğrudan manipülasyonu:
* **Kontrol Hatları:** **DTR (Data Terminal Ready)** ve **RTS (Request to Send)** pinlerinin doğrudan donanımsal sürümü.
* **Galvanik Yalıtım:** Telsizin CW/Key girişine optokuplörlü (4N35 / PC817) devreyle tam izolasyonlu bağlantı.
* **Yüksek Çözünürlüklü Zamanlama:** İş parçacığı gecikmelerini önlemek amacıyla donanımsal yüksek hassasiyetli zamanlayıcılar (`QueryPerformanceCounter` / `clock_gettime`) kullanılır.

### 4.2 ADIF 3.1.4 Görüşme Günlüğü ve Jeodezik Hesaplamalar
* **ADIF Standart Uyumluluğu:** Amatör telsiz veri değişim formatı ADIF v3.1.4 ile çift yönlü log alma ve aktarma.
* **Büyük Daire Mesafesi:** İstasyonların Maidenhead Grid konumları arasındaki mesafe ve azimut açısı **Haversine formülü** ile hesaplanır:

$$d = 2 R_{\oplus} \arcsin \left( \sqrt{ \sin^2\left(\frac{\Delta \phi}{2}\right) + \cos(\phi_1)\cos(\phi_2)\sin^2\left(\frac{\Delta \lambda}{2}\right) } \right)$$

---

## 💻 5. Sistem Mimarisi

| Katman / Modül | Web Uygulaması | Masaüstü Uygulaması (Windows / macOS / Linux) |
| :--- | :--- | :--- |
| **Çalışma Zamanı** | ECMAScript 2023, HTML5, WebAssembly | C# (.NET 10), C99 yerel paylaşımlı kütüphaneler |
| **Ses Sentezi** | Web Audio API (`AudioContext`, `OscillatorNode`) | SDL2 / ALSA / CoreAudio / WASAPI |
| **Konuşma Tanıma (ASR)** | W3C SpeechRecognition (Sürekli akış) | Whisper / Yerel İşletim Sistemi Motoru |
| **Konuşma Sentezi (TTS)** | W3C SpeechSynthesis API | Piper VITS ONNX runtime & EMA Lightning |
| **Sinyal Demodülasyonu**| Goertzel Rezonatörü (`AudioWorklet` / Dizi) | C dilinde optimize Goertzel DSP |
| **Arayüz Mimarisi** | Responsive CSS Grid, CSS Değişkenleri, RTL | WinUI 3, Avalonia / Uno Platform |
| **Çok Dilli Destek** | Bellek içi sözlük (`tr`, `en`, `de`, `ru`, `ar`) | JSON Dil Paketleri (`MorseCore.AppCore.Strings`) |

---

## 🚀 6. İkili Sürümler ve Kurulum

### Web Sürümü (Kurulum Gerektirmez)
Tüm modern tarayıcılardan doğrudan çalıştırılabilir:
👉 **[https://mserman90.github.io/MorsLab/](https://mserman90.github.io/MorsLab/)**

### Masaüstü Kurulum Paketleri
İşletim sisteminize uygun derlenmiş sürümleri [GitHub Releases](https://github.com/mserman90/MorsLab/releases/latest) üzerinden temin edebilirsiniz:
* **Windows (x64):** `MorseCore-1.0.1-Windows-x64-Setup.exe` veya taşınabilir `.zip`
* **macOS (Apple Silicon M1/M2/M3/M4):** `MorseCore-1.0.1-macOS-arm64.dmg`
* **Linux (x64):** Ubuntu/Debian/Pardus için `.deb`, Arch Linux için `PKGBUILD`

---

## 📚 7. Akademik Atıf (Citation)

MorsLab'i bilimsel araştırmalarınızda, lisans/lisansüstü çalışmalarınızda veya telsiz telgrafçılığı projelerinizde kullanıyorsanız lütfen aşağıdaki BibTeX formatında atıfta bulunun:

```bibtex
@software{MorsLab2026,
  author       = {Akceylan, Mustafa Mert and Erman, Murat},
  title        = {{MorsLab: A Cross-Platform Digital Signal Processing and Neural Acoustic Framework for Radiotelegraphy and Bidirectional Morse-Speech Telecommunications}},
  year         = {2026},
  publisher    = {GitHub},
  version      = {1.0.1},
  url          = {https://github.com/mserman90/MorsLab},
  license      = {Apache-2.0}
}
```

---

## 🤝 8. Teşekkür & Açık Kaynak Lisans

**MorsLab © 2026 Mustafa Mert Akceylan · Açık Kaynak [Apache-2.0 Lisansı](https://github.com/mserman90/MorsLab/blob/main/LICENSE) · [GitHub Deposu](https://github.com/mserman90/MorsLab)**

Bu projenin hayata geçmesine katkı sağlayan açık kaynak projelerine, telsiz operatörlerine ve araştırmacılara teşekkürlerimizi sunarız:
* **Canberk Aslan** – [EMA Lightning](https://github.com/canberk7/ema-lightning) hafif ses sentezi modeli.
* **PatientDesk AI** – [Antalia-2 Mini](https://huggingface.co/cloud0day3/antalia-mini).
* **Rhasspy / Piper Topluluğu** – [Piper TTS](https://github.com/rhasspy/piper) yerel nöral ses modelleri.
* **Uluslararası Telekomünikasyon Birliği (ITU)** – [ITU-R M.1677-1](https://www.itu.int/rec/R-REC-M.1677) standardı.

Bu proje **Apache Lisansı 2.0** kapsamında sunulmaktadır. Ayrıntılar için [LICENSE](LICENSE) ve [NOTICE](NOTICE) dosyalarını inceleyebilirsiniz.

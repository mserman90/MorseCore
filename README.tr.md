<div align="center">

![MorseCore](docs/media/morsecore.gif)

**Mesajını yaz. Morse'u dinle. Alıştırma yaparak öğren.**

[![İndir Windows](https://img.shields.io/badge/%C4%B0ndir-Windows-5e6a42?style=for-the-badge&logo=windows&logoColor=white)](https://github.com/mustafamertakceylan/MorseCore/releases/download/v1.0.1/MorseCore-1.0.1-Windows-x64-Setup.exe)
[![İndir macOS](https://img.shields.io/badge/macOS-Apple_Silicon-5e6a42?style=for-the-badge&logo=apple&logoColor=white)](https://github.com/mustafamertakceylan/MorseCore/releases/download/v1.0.1/MorseCore-1.0.1-macOS-arm64.dmg)
[![İndir Linux](https://img.shields.io/badge/Linux-x64-5e6a42?style=for-the-badge&logo=linux&logoColor=white)](https://github.com/mustafamertakceylan/MorseCore/releases/tag/v1.0.1)
[![1.0.1](https://img.shields.io/badge/v1.0.1-Yay%C4%B1nda-c58c59?style=for-the-badge)](https://github.com/mustafamertakceylan/MorseCore/releases/tag/v1.0.1)
[![Canlı Web Uygulaması](https://img.shields.io/badge/Canl%C4%B1_Web_Uygulamas%C4%B1-GitHub_Pages-2563EB?style=for-the-badge&logo=githubpages&logoColor=white)](https://mserman90.github.io/MorseCore/)

[English](README.md) · **Türkçe** · [Deutsch](README.de.md) · [Русский](README.ru.md) · [العربية](README.ar.md)

[**Canlı Web Uygulaması**](https://mserman90.github.io/MorseCore/) · [**Kullanım kılavuzu · Wiki**](https://github.com/mustafamertakceylan/MorseCore/wiki/Baslangic)

</div>

MorsLab; çift yönlü Mors kodu çevirisi, gerçek zamanlı DSP çözümü, amatör telsiz seri port anahtarlaması (Rig Keying) ve Türkçe & İngilizce çift yönlü konuşma (Konuşma-Mors ve Mors-Konuşma) özelliklerine sahip açık kaynaklı, çok platformlu (Web, Windows, macOS, Linux) bir Mors ekosistemidir.

---

## 🌟 Temel Özellikler & Modüller

| Modül | Açıklama | Platform |
| :--- | :--- | :--- |
| **Çift Yönlü Çevirici** | ITU-R M.1677-1 standardı ile Metin ↔ Mors, WPM/Ton ayarı, Latin, Kiril ve Arapça alfabeleri. | Web & Masaüstü |
| **🎙️ Konuşmayı Mors'a Çevir** | **Türkçe ve İngilizce** canlı mikrofon konuşmalarını ve ses dosyalarını anında Mors koduna dönüştürür. | Web & Masaüstü |
| **🗣️ Mors'tan Konuşmaya (TTS)** | Mors metnini veya `.wav` ses dosyasını çözüp **Türkçe veya İngilizce** doğal yapay zeka sesiyle okur. | Web & Masaüstü |
| **📡 Akışlı DSP Çözücü** | Goertzel algoritması ve adaptif enerji/hız eşikleriyle analog CW sinyallerini gerçek zamanlı çözer. | Masaüstü |
| **📻 Telsiz Anahtarlama (Rig Keyer)** | USB-Seri dönüştürücüler (FTDI/CH340) üzerinden DTR/RTS pinleriyle telsizi doğrudan anahtarlar. | Masaüstü |
| **🔍 Çağrı İşareti & QSO Günlüğü**| Çevrimdışı ITU önek çözümlemesi, DXCC/kıta doğrulaması ve ADIF formatında görüşme günlüğü. | Masaüstü |
| **🎯 İnteraktif Alıştırma Manevrası**| Dit/Dah oran ölçümlü düz manevra simülatörü, Koch metodu egzersizleri ve Q-kodları sözlüğü. | Web & Masaüstü |

---

## 📖 Hızlı Kullanım Kılavuzu

### 1. Metin ↔ Mors Çevirisi & Ses
1. Metin kutusuna mesajınızı veya Mors kodunu yazın.
2. Hız (5–60 WPM) ve Ton frekansını (200–2000 Hz) seçin.
3. **Dinle** düğmesine basarak gerçekçi CW tonuyla dinleyin veya **WAV İndir** ile ses dosyası olarak kaydedin.

### 2. Konuşmayı Mors'a Çevirme (Türkçe / İngilizce)
1. Konuşma dilini seçin: **Türkçe (tr-TR)** veya **English (en-US)**.
2. **Dinlemeyi Başlat** düğmesine basıp mikrofona konuşun; sözcükler anında Mors koduna çevrilir.
3. Veya kayıtlı bir ses dosyasını (**Ses Dosyası Seç**) yükleyin.
4. Oluşan Mors kodunu dinleyebilir veya doğrudan telsizinize gönderebilirsiniz.

### 3. Mors Kodundan Konuşmaya (Mors -> Ses)
1. Mors kodunu yapıştırın veya **Mors Ses Dosyası Seç (.wav)** ile CW kaydını yükleyin.
2. Çözülen metin anında ekranda belirir.
3. **Konuşmayı Başlat** düğmesine basarak Türkçe veya İngilizce doğal sesle dinleyin.

### 4. Telsiz Donanım Anahtarlaması (Seri Port)
1. Telsizinizin CW/Key girişini USB-Seri dönüştürücüye bağlayın.
2. Telsiz ayarlarından COM portunu ve tetikleme pinini (**DTR** veya **RTS**) seçin.
3. 'Telsize Gönder' ile mesajınızı mikrosaniye hassasiyetli zamanlamayla yayına verin.

---

[English](README.md) · **Türkçe** · [Deutsch](README.de.md) · [Русский](README.ru.md) · [العربية](README.ar.md)

[**Kullanım kılavuzu · Wiki**](https://github.com/mustafamertakceylan/MorseCore/wiki/Baslangic)

</div>

MorsLab; metni Morse koduna çevirmek, dinlemek ve alıştırma yapmak için ücretsiz bir Windows, macOS ve Linux uygulamasıdır. Mesajların bilgisayarında kalır.

![Türkçe MorseCore: solda metin, sağda Morse kodu](docs/media/converter.tr.png)

**macOS:** [DMG dosyasını](https://github.com/mustafamertakceylan/MorseCore/releases/download/v1.0.1/MorseCore-1.0.1-macOS-arm64.dmg) açıp MorseCore’u Applications klasörüne sürükle. Bu sürüm Apple noter onayına sahip değil; Gizlilik ve Güvenlik üzerinden açılışa izin vermen gerekebilir. [Mac kurulumu](docs/macos.md).

**Linux:** Ubuntu, Debian veya Pardus için `.deb`; Arch için taşınabilir arşiv ve `PKGBUILD` kullan. [Linux kurulumu](docs/linux.tr.md).

### Hemen başla

1. Windows’ta: **Setup’ı çalıştır**, ardından Başlat menüsünden MorseCore’u aç.
2. **Dönüştürücü** bölümüne mesajını yazıp **Dönüştür** düğmesine bas. Dinlemek için **Ses** bölümünü kullan.
3. Alfabe, alıştırmalar ve telsiz kısaltmaları için **Eğitim** bölümünü aç.

Kurmak istemiyorsan [taşınabilir ZIP’i](https://github.com/mustafamertakceylan/MorseCore/releases/download/v1.0.1/MorseCore-1.0.1-Windows-x64-Desktop.zip) çıkarıp **MorseCore.Desktop.exe** dosyasını aç. Bu Windows sürümü imzasızdır; Windows yayıncı uyarısı gösterebilir.

### Neler yapabilirsin?

- Metinleri ve metin dosyalarını iki yönde, tek tek veya topluca çevir.
- Morse dinle, WAV kaydet ve kendin göndererek ritmini ölç.
- WAV dosyasından veya mikrofondan temiz, tek tonlu Morse kaydını çöz.
- **Amatör telsiz** altında istasyonlarını yönet, mesaj hazırla, ADIF içe aktar ve çevrimdışı haritalı görüşme günlüğü tut. Cihaz bağlantısı planlanıyor; uygulama RF yayını yapmaz.
- EMA Lightning, Antalia-2 Mini ve Piper sesleriyle metni çevrimdışı seslendir. Sesi ilk kez internetle kur; sonrasında bilgisayarında dinle.

### Kendi dilinde, kendi temanda

**Türkçe · English · Deutsch · Русский · العربية**

Beş dilde menü ve isteğe bağlı seslendirme bulunur. Metin dönüşümü Latin/Türkçe, Rusça ve Arapça alfabelerini destekler. Arapçada menü sağa geçer; Morse soldan sağa kalır.

Ayarlar’dan krem açık temayı veya yeşil koyu temayı seç. Yavaş arka plan hareketi pencere etkin değilken durur; **Hareketi azalt** seçeneğiyle kapanır.

<details>
<summary><b>Koyu temayı ve görüşme haritasını gör</b></summary>

![Türkçe arayüzde yeşil koyu tema](docs/media/dark.tr.png)

![Türkçe görüşme günlüğü ve çevrimdışı harita](docs/media/map.tr.png)

</details>

**Windows x64, macOS Apple Silicon (M1 ve sonrası) ve Linux x64 için mevcut.** Ubuntu, Debian, Arch ve Pardus için [Linux kurulumuna](docs/linux.tr.md) bak.

### Teşekkürler ve lisans

[EMA Lightning](https://github.com/canberk7/ema-lightning) için **Canberk Aslan’a**, [Antalia-2 Mini](https://huggingface.co/cloud0day3/antalia-mini) için **PatientDesk AI ekibine** ve çevrimdışı seslendirmeyi sağlayan **Piper** katkıcılarına teşekkür ederiz.

[Apache 2.0](LICENSE) · [Katkılar ve lisanslar](NOTICE) · [Sürüm notları](CHANGELOG.md) · [Ses kurulumu](docs/offline-speech.tr.md) · [Kaynak koddan çalıştırma](docs/desktop.tr.md)

[Katkıda bulun](.github/CONTRIBUTING.md) · [Erişilebilirlik](.github/ACCESSIBILITY.md)

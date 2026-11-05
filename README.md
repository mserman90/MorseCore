<div align="center">

![MorseCore](docs/media/morsecore.gif)

**Write a message. Hear Morse. Learn by practicing.**

[![Download Windows](https://img.shields.io/badge/Download-Windows-5e6a42?style=for-the-badge&logo=windows&logoColor=white)](https://github.com/mustafamertakceylan/MorseCore/releases/download/v1.0.1/MorseCore-1.0.1-Windows-x64-Setup.exe)
[![Download macOS](https://img.shields.io/badge/macOS-Apple_Silicon-5e6a42?style=for-the-badge&logo=apple&logoColor=white)](https://github.com/mustafamertakceylan/MorseCore/releases/download/v1.0.1/MorseCore-1.0.1-macOS-arm64.dmg)
[![Download Linux](https://img.shields.io/badge/Linux-x64-5e6a42?style=for-the-badge&logo=linux&logoColor=white)](https://github.com/mustafamertakceylan/MorseCore/releases/tag/v1.0.1)
[![1.0.1](https://img.shields.io/badge/v1.0.1-Release-c58c59?style=for-the-badge)](https://github.com/mustafamertakceylan/MorseCore/releases/tag/v1.0.1)
[![Live Web Demo](https://img.shields.io/badge/Live_Web_App-GitHub_Pages-2563EB?style=for-the-badge&logo=githubpages&logoColor=white)](https://mserman90.github.io/MorseCore/)

**English** · [Türkçe](README.tr.md) · [Deutsch](README.de.md) · [Русский](README.ru.md) · [العربية](README.ar.md)

[**Live Web Application**](https://mserman90.github.io/MorseCore/) · [**User guide · Wiki**](https://github.com/mustafamertakceylan/MorseCore/wiki/Getting-Started)

</div>

MorsLab is an open-source, multiplatform suite (Web, Windows, macOS, Linux) for bidirectional Morse code conversion, real-time DSP decoding, amateur radio hardware rig keying, and bidirectional speech (Speech-to-Morse and Morse-to-Speech) in Turkish and English.

---

## 🌟 Key Features & Architecture

| Feature | Description | Platform |
| :--- | :--- | :--- |
| **Bidirectional Converter** | ITU-R M.1677-1 standard text ↔ Morse with WPM, pitch, Latin, Cyrillic, and Arabic alphabets. | Web & Desktop |
| **🎙️ Speech to Morse** | Real-time speech recognition for **Turkish & English** (live microphone & audio files) converting directly to Morse code. | Web & Desktop |
| **🗣️ Morse to Speech (TTS)** | Decodes Morse text or `.wav` CW audio files and reads them aloud in natural **Turkish or English** voice synthesis. | Web & Desktop |
| **📡 Streaming DSP Decoder** | Real-time Goertzel tone detection with adaptive energy/speed thresholds for decoding live CW audio streams. | Desktop |
| **📻 Serial Rig Keyer** | Direct transmitter keying via USB serial ports (FTDI, CH340, CP2102) utilizing DTR or RTS line toggling. | Desktop |
| **🔍 Callsign Lookup & Logbook**| Offline ITU prefix analysis, DXCC/continent resolution, and ADIF 3.1.4 compliant QSO contact logging. | Desktop |
| **🎯 Interactive Practice Keyer**| Straight key simulator with Dit/Dah ratio measurement, Koch method exercises, and Q-code glossary. | Web & Desktop |

---

## 📖 Quick Usage Guide

### 1. Bidirectional Conversion & CW Audio
1. Type your plain text or Morse code into the input box.
2. Select speed (5–60 WPM) and tone pitch (200–2000 Hz).
3. Click **Play** to listen with genuine CW sidetone, or click **Export WAV** to generate standalone standard audio files.

### 2. Speech to Morse (Türkçe / English)
1. Select your spoken language: **Türkçe (tr-TR)** or **English (en-US)**.
2. Click **Start Listening** to speak into your microphone. Words are transcribed and translated to Morse code in real-time.
3. Or load recorded speech audio (`.wav` / `.mp3`) using **Choose Audio File**.
4. Play the generated Morse or transmit directly to your amateur radio transceiver.

### 3. Morse to Speech (Text & Audio File -> Voice)
1. Paste Morse code or click **Choose Morse Audio File (.wav)** to decode an audio file.
2. The decoded message appears instantly in plain text.
3. Click **Speak Text** to listen via natural voice synthesis in Turkish or English.

### 4. Amateur Radio Rig Keying (Serial Port)
1. Connect your transmitter keying interface (optocoupler or transistor circuit) to a USB-to-Serial adapter.
2. Under **Radio Settings**, select your serial COM port and control pin (**DTR** or **RTS**).
3. Transmit messages with microsecond-accurate Morse timing.

---

**English** · [Türkçe](README.tr.md) · [Deutsch](README.de.md) · [Русский](README.ru.md) · [العربية](README.ar.md)

[**User guide · Wiki**](https://github.com/mustafamertakceylan/MorseCore/wiki/Getting-Started)

</div>

MorsLab is a free Windows, macOS and Linux app for converting text to Morse code, listening and practicing. Your messages stay on your computer.

![MorseCore in English: text on the left, Morse code on the right](docs/media/converter.en.png)

**macOS:** open the [DMG](https://github.com/mustafamertakceylan/MorseCore/releases/download/v1.0.1/MorseCore-1.0.1-macOS-arm64.dmg), then drag MorseCore to Applications. This release is not notarized; macOS may require approval in Privacy & Security. See [Mac installation](docs/macos.md).

**Linux:** choose the `.deb` for Ubuntu, Debian or Pardus, or the portable archive and `PKGBUILD` for Arch. See [Linux installation](docs/linux.md).

### Get started

1. On Windows: **Run Setup**, then open MorseCore from the Start menu.
2. Type a message in **Converter** and choose **Convert**. Use **Audio** to listen.
3. Open **Learning** for the alphabet, exercises and radio abbreviations.

Prefer no installation? Extract the [portable ZIP](https://github.com/mustafamertakceylan/MorseCore/releases/download/v1.0.1/MorseCore-1.0.1-Windows-x64-Desktop.zip) and open **MorseCore.Desktop.exe**. This Windows build is unsigned; Windows may show a publisher warning.

### What can you do?

- Convert text and text files in both directions, including batches.
- Listen to Morse, save WAV files and practice sending with timing feedback.
- Decode clean, single-tone Morse from WAV files or a microphone.
- Use **Amateur radio** for station profiles, message preparation and a contact log with ADIF import and an offline map. Hardware connection is planned; the app does not transmit RF.
- Read text aloud offline. Install a voice once with internet, then listen locally. Thanks to EMA Lightning, Antalia-2 Mini and Piper.

### Your language, your theme

**Türkçe · English · Deutsch · Русский · العربية**

Menus and optional speech voices cover all five languages. Text conversion supports Latin/Turkish, Russian and Arabic. Arabic navigation mirrors; Morse stays left to right.

Choose the cream light theme or the green dark theme in Settings. Gentle background motion pauses when inactive; **Reduce motion** turns it off.

<details>
<summary><b>See the dark theme and contact map</b></summary>

![English interface in the green dark theme](docs/media/dark.en.png)

![English contact log with the offline map](docs/media/map.en.png)

</details>

**Available: Windows x64, macOS Apple Silicon (M1 or newer) and Linux x64.** [Linux installation](docs/linux.md) covers Ubuntu, Debian, Arch and Pardus.

### Thanks & license

Thanks to **Canberk Aslan** for [EMA Lightning](https://github.com/canberk7/ema-lightning), **PatientDesk AI** for [Antalia-2 Mini](https://huggingface.co/cloud0day3/antalia-mini), and the **Piper** contributors for offline speech.

[Apache 2.0](LICENSE) · [Credits & licenses](NOTICE) · [Release notes](CHANGELOG.md) · [Voice setup](docs/offline-speech.md) · [Build from source](docs/desktop.md)

[Contribute](.github/CONTRIBUTING.md) · [Accessibility](.github/ACCESSIBILITY.md)

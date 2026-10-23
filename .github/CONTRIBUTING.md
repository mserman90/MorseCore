# Contributing / Katkıda Bulunma

## English

Bug reports, translations, clearer documentation and reproducible test results
are welcome. Read the [README](../README.md), [architecture](../docs/architecture.md)
and [Code of Conduct](CODE_OF_CONDUCT.md). Issue and PR descriptions can be
English or Turkish. Security reports follow [SECURITY.md](SECURITY.md).

### Report or propose

Check existing issues, then use a bug, audio or feature form. Include the app
version/commit, OS version, CPU architecture, steps and expected/actual results.
Use example text and synthetic station/contact records. Remove private paths,
credentials and personal data from screenshots and logs; do not upload a real
logbook or private recording. Discuss large changes before implementing them.

### Build and verify

Start with the [native API guide](../docs/native-api.md),
[Windows guide](../docs/desktop.md), [Linux guide](../docs/linux.md) or
[macOS build guide](../apps/MorseCore.macOS/README.md) for prerequisites.
From the repository root, native/interop verification uses:

```sh
cmake --preset interop
cmake --build --preset interop
ctest --preset interop
```

With .NET 10, run the shared AppCore smoke consumer against your built native
library (replace the final path with the actual DLL, .so or .dylib):

```sh
dotnet run --project tests/dotnet/MorseCore.AppCore.Smoke -c Release -- <native-library-path>
```

On a Mac, from `apps/MorseCore.macOS`, run `./script/build_and_run.sh --test`
for native and Swift bridge tests. Platform build/UI commands and their limits
are in the linked guides. Use isolated test preferences and synthetic data;
do not overwrite your installed application's settings or logbook.

Keep business rules in the shared C23 engine or AppCore. Keep platform-specific
window, picker and audio work in adapters. For shared changes, check every
affected platform; distinguish build success, automated UI checks and device QA.
State tests not run. Preserve third-party licenses and [NOTICE](../NOTICE).

### Pull requests

- Keep one focused topic; explain the problem, resulting behavior and validation.
- Use Conventional Commits (`fix(audio): …`); existing commit summaries use Turkish.
- Update affected documentation and tests, and use the PR template.
- Verify AI-assisted contributions yourself; describe limitations honestly.
- Include no model weights, build outputs, credentials or personal user data.
- Preserve published releases/tags and unrelated changes.

Source contributions are provided under [Apache-2.0](../LICENSE). Voice engines,
models and fonts retain their separate licenses.

## Türkçe

Hata bildirimi, çeviri, belge düzeltmesi ve tekrarlanabilir test sonuçları da
katkıdır. [README](../README.tr.md), [mimari](../docs/architecture.md) ve
[davranış kurallarını](CODE_OF_CONDUCT.md) okuyun. Issue ve PR açıklamaları
Türkçe veya İngilizce olabilir. Güvenlikte [SECURITY.md](SECURITY.md) yolunu izleyin.

Mevcut issue'ları kontrol edin; hata, ses veya özellik formunu kullanın. Sürüm,
işletim sistemi, işlemci mimarisi, tekrar adımları ve beklenen/gerçek sonucu
yazın. Örnek metin ve sentetik istasyon/kayıt kullanın. Kişisel yol, sır ve özel
verileri çıkarın; gerçek görüşme günlüğü veya özel ses kaydı yüklemeyin. Büyük
değişiklikleri uygulamadan önce tartışın.

Derleme gereksinimleri için [native](../docs/native-api.tr.md),
[Windows](../docs/desktop.tr.md), [Linux](../docs/linux.tr.md) ve
[macOS](../apps/MorseCore.macOS/README.md) rehberlerine bakın. Yukarıdaki CMake
komutları native/interop kontrolleridir; .NET 10 komutuna derlediğiniz native
kitaplığın yolunu verin. Mac'te `apps/MorseCore.macOS` dizininden
`./script/build_and_run.sh --test` native ve Swift köprü testlerini çalıştırır.

Testleri izole tercihler ve sentetik veriyle yapın; kurulu uygulamanızın
ayarlarını/günlüğünü ezmeyin. İş kurallarını ortak C23 motorunda veya AppCore'da,
platform işlerini adaptörlerde tutun. Etkilenen platformları kontrol edin;
derleme, otomatik arayüz testi ve cihaz testini ayırın. Çalıştırılmayanları yazın.

PR'ı tek konuya odaklayın; sorunu, sonucu ve doğrulamayı açıklayın. Conventional
Commits kullanın; mevcut özetler Türkçedir. Belgeleri/testleri güncelleyin ve PR
şablonunu kullanın. AI katkılarını kendiniz doğrulayın. Model ağırlığı, derleme
çıktısı, sır veya kişisel veri eklemeyin; yayımlanmış sürümleri ve ilgisiz işleri
koruyun. Katkılar [Apache-2.0](../LICENSE) kapsamındadır; üçüncü taraf lisansları
ve [NOTICE](../NOTICE) korunmalıdır.

# AbxPilot Planı

Enfeksiyon uzmanı gibi düşünen, ampirik antibiyotik seçimini gerekçesiyle gösteren
çapraz platform program. Temel hasta 70 kg sağlıklı erişkin. Önce Türkçe ve Türkiye,
sonra dünya.

Danışma kaydı: [giden](danisma/2026-09-21-fable-plan-giden.md) · [dönen](danisma/2026-09-21-fable-plan-donen.md)

## Ürün Fikri

- Kullanıcı sendrom seçer: pnömoni, idrar yolu, selülit...
- Altta 10-20 soru durur; hepsi isteğe bağlı, cevapsız soru varsayılanla işler.
- Her cevapta üstteki öneri canlı değişir ve **neyin neden değiştiği** animasyonla görünür.
- Üst kart: rejim, doz, yol, süre, "neden bu", alternatifler, elenenler ve nedeni, spektrum.
- Hasta verisi diske yazılmaz; oturum kapanınca silinir.

## Ekran Düzeni

| Bölge | İçerik |
|---|---|
| Üst çubuk | Standart Teknesyum çubuğu (`scaffold.js ustcubuk`), veri sürümü rozeti, dil; ortada kalıcı "klinik değerlendirme gerektirir" satırı (sığmazsa kısa sürüm, tam metin ipucu ve ekran okuyucu adı) |
| Sol ray | Sendrom listesi, arama |
| Başlık satırı | Sendrom adı, kılavuz seti düğmesi (kaynak bağlantısı), ayarlar |
| Üst sıra | Solda kart (birinci seçenek, doz/yol/süre, kaynak, gerekçe, alternatifler), sağda spektrum; eşit yükseklik, pencerenin en çok `CardHeightShare` kadarı, her biri kendi içinde kayar |
| Alt sıra | Tam genişlik soru ızgarası (`QuestionGrid`): 4/3/2/1 sütun (`QuestionColumnMin` 180, `QuestionColumnsMax` 4), her soru başlık + tek tıkla seçilen maddeler; tekli radyo, çoklu işaret kutusu |

Açılışta boş durum yok: son sendrom (yoksa TKP) varsayılanlarla açılır, birinci seçenek hemen görünür.
Madde seçimi öneriyi günceller; kart, spektrum ve soru kartları FLIP ile yer değiştirir.

Telefonda ve Android'de üst çubuk yok: klinik değerlendirme satırı kartın üstünde kalıcı durur;
kart üstte, spektrum ve sorular altta kayar.

## Animasyon Tasarımı

Hareket süs değil, açıklamanın kendisi. Süreler ve eğriler palet token'larından gelir.

- **Öneri farkı:** ViewModel eski ve yeni öneriyi karşılaştırır. Yalnız değişen alan canlanır:
  ilaç adı kayarak değişir, doz sayısı sayarak geçer, süre rozeti yanıp söner.
- **Neden izi:** Cevaplanan soru kartından öneri kartına kısa bir ışık yolu akar. Kullanıcı
  hangi cevabın neyi değiştirdiğini görür.
- **Elenen ilaç:** Alternatif listesinden solarak çıkar, "elendi: penisilin alerjisi IgE" etiketi kalır.
- **Spektrum şeridi:** Çubuklar yeni kapsama değerine yayılarak büyür ya da küçülür.
- **Soru açılması:** Koşullu soru, tetikleyen cevabın altından açılarak gelir.
- **Yükleme:** İskelet kart; döner çark yok.
- Motor senkron ve 50 ms altında kalmalı; yoksa animasyon kekeler.

## Mimari

```
AbxPilot.sln
├─ src/AbxPilot.Core      motor, veri modeli, kural değerlendirici (UI bağımsız)
├─ src/AbxPilot.Data      derlenmiş bilgi tabanı JSON, gömülü kaynak
├─ src/AbxPilot.UI        Avalonia paylaşılan görünümler, ViewModel, palet
├─ src/AbxPilot.Desktop   Windows/macOS/Linux başı
├─ src/AbxPilot.Android   Android başı; ilk günden derlenir, boş bile olsa
├─ tools/KbCompiler       CSV/YAML → şema doğrulama → JSON derleyici
├─ kb/                    insan tarafından düzenlenen bilgi tabanı kaynağı
└─ tests/                 motor, şema, altın vaka, kabuk standardı testleri
```

- Yığın: .NET 10 + Avalonia 12, CommunityToolkit.Mvvm. Görsel tema kitaplığı yok.
- Veri `AvaloniaResource` olarak gömülü; dosya yolu varsayımı yok, Android'de de çalışır.
- YAML çalışma anında okunmaz: derleyici JSON üretir, source-generated `System.Text.Json`
  okur. AOT ve trimming güvenli.
- Kabuk standardı testleri VidShrink'ten gelir: renk yalnız paletten, yazıya parıltı yok,
  yer tutucu metin yok, kendi başlık çubuğu.

## Bilgi Tabanı

Kod bilgiyi taşımaz; bilgi `kb/` altındadır ve her kayıt izlenebilirdir.

| Dosya | İçerik |
|---|---|
| `drugs/*.yaml` | INN ad, sınıf, spektrum, doz nesnesi, yol, biyoyararlanım, yan etki, etkileşim, gebelik |
| `pathogens/*.yaml` | Patojen, Gram, tipik direnç mekanizmaları |
| `regimens/*.yaml` | Rejim: ilaç, doz, rol bileşenleri |
| `syndromes/*.yaml` | Sendrom, olası patojenler, soru listesi |
| `questions/*.yaml` | Soru, seçenekler, varsayılan, görünürlük koşulu |
| `guidelines/<set>/set.yaml` + `*.csv` | Karar tabloları; klinisyen Excel'de düzenler; `extends` ile başka seti devralır |
| `spectrum/*.csv` | İlaç × patojen kapsam matrisi |
| `regions/<ülke>.yaml` | Direnç oranları, eşikler, ruhsatlı ilaçlar, ticari adlar |
| `i18n/<dil>/*.json` | Tüm görünür metinler anahtarla; derleyici birleştirir |
| `schema/*.schema.json` | JSON Schema 2020-12 doğrulama |
| `sources.yaml`, `scoring.yaml`, `kb.yaml` | Kaynaklar, puan ağırlıkları, sürüm |

- Doz nesnesi: `endikasyon × yol × renal bant`. İlk sürüm yalnız normal renal bandı doldurur.
- Penisilin alerjisi üç tip: IgE aracılı, gecikmiş, ağır deri reaksiyonu.
- Her kayıtta zorunlu: `id`, `version`, `source`, `section`, `source_date`, `reviewed_at`.
- `tools/AbxPilot.KbCompiler` `AbxPilot.Data` derlemesinde çalışır, tek `kb.json` üretir ve gömer; hata derlemeyi dosya ve satırla durdurur. Kaynak eşlemesi: `docs/kb-kaynaklar.md`.
- Gözden geçirme tarihi eskiyen kural kartta uyarı bandı taşır.
- Kılavuz metni kopyalanmaz; kural çıkarılır, kaynak bölüm numarası tutulur.

## Karar Motoru

Seçimi puan yapmaz; kılavuz tablosu ve kısıt yapar. Her adım gerekçe izine satır düşer.

1. Bağlam kurulur: sendrom, cevaplar, varsayılanlar, bölge, kılavuz seti.
2. Risk bayrakları türetilir: MRSA, Pseudomonas, ESBL. Eşik bölge dosyasından okunur.
3. Karar tablosunda ilk eşleşen satır sıralı aday rejimleri verir.
4. Sert kısıtlar adayı eler: alerji, gebelik, etkileşim, renal, ruhsat yokluğu.
5. Kalan ilk aday önerilir; diğerleri alternatif, elenenler gerekçesiyle listelenir.
6. Patojen kapsaması yalnız açıklama ve spektrum şeridi içindir, seçimi belirlemez.

### Karşılaştırma Puanı

Seçimi tablo yapar; puan yalnız aynı basamaktaki eşdeğer seçenekleri karşılaştırır.
Örnek: seftriakson mu sefepim mi, amoksisilin-klavulanat mı ampisilin-sulbaktam mı.

- Puan seçimi hiçbir zaman değiştirmez; tablonun sırası ile çelişirse tablo kazanır.
- Bileşenler ayrı tutulur: spektrum uyumu, gereksiz genişlik cezası (AWaRe basamağı),
  yan etki, doz kolaylığı, oral geçiş, bölgesel direnç, maliyet.
- Ağırlıklar `kb/scoring.yaml` içinde, kaynaklı ve sürümlüdür; kodda sayı yoktur.
- Görünürlük: kartta varsayılan olarak yok. Fare üstüne gelince ya da telefonda uzun
  basınca bileşen dökümüyle açılır.
- Ayarlarda "Karşılaştırma puanını göster" anahtarı açılırsa alternatif satırlarında sürekli görünür.
- Sayı her zaman bileşenleriyle birlikte gösterilir; tek başına çıplak rakam gösterilmez.

Çıktı değişmez bir kayıttır; UI farkı buradan hesaplar.

## Kaynaklar

- Türkiye: TTD pnömoni uzlaşı raporu, EKMUD rehberleri, UHESA direnç verisi.
- Dünya: IDSA/ATS, ESCMID, NICE, WHO AWaRe, ECDC EARS-Net, WHO GLASS.
- Sanford ve UpToDate telifli; gömülmez.
- Bölge dosyası olmayan ülke: WHO AWaRe varsayılanı ve görünür uyarı.

## Regülasyon ve Güvenlik

Hastaya özel tedavi öneren yazılım AB MDR Kural 11'e göre en az sınıf IIa tıbbi cihazdır.
TİTCK aynı çerçeveyi uygular. "Reçete değildir" yazısı sınıfı düşürmez; amaç beyanı düşürür.

- **Karar (2026-09-23): kılavuz gezgini ve eğitim aracı.** Tıbbi cihaz yolundan olabildiğince uzak durulur.
- Ekran dili "öneri" değil "kılavuz özeti"; her doz ve rejim kaynağıyla birlikte gösterilir.
- Hastaya özel hesap (kiloya göre doz, renal ayar hesabı) ilk sürümde yok; kılavuzdaki
  standart erişkin dozu ve renal bant tablosu olduğu gibi gösterilir.
- Amaç beyanı README'de ve açılış ekranında aynı cümleyle durur.
- Her kural ID'li, sürümlü, kaynaklı ve testli. IEC 62304
  izlenebilirliğinin yarısı böylece baştan kurulur.
- Sepsis ve menenjit ilk sürüme girmez; zarar sınıfı en yüksek alan.
- Kişisel sağlık verisi tutulmaz; KVKK yükü doğmaz.
- Klinik içerik yayından önce bir enfeksiyon hastalıkları uzmanının gözden geçirmesinden geçer.

## Test

- Şema testi: derleyici her kaydı doğrular; hata varsa yapı kırmızı.
- Altın vaka testi: klinik vinyet → beklenen rejim. Her sendrom için en az 15 vaka.
- Kılavuz güncellemesi raporu: hangi altın vakanın sonucu değişti.
- Motor süresi testi: tüm vakalar 50 ms altında.
- Kabuk standardı testleri ve her aşamada ekran görüntüsüyle doğrulama.

## Aşamalar

- [x] **A0 İskele:** çözüm, beş proje, palet, üst çubuk, Kur penceresi, Android başı derlenir, AGPL lisansı
- [x] **A1 Bilgi tabanı çekirdeği:** şemalar, KbCompiler, TKP için ilaç, patojen, soru ve TTD/IDSA tabloları
- [x] **A2 Motor:** bağlam, risk bayrakları, tablo eşleme, kısıtlar, gerekçe izi, altın vakalar
- [x] **A3 Ekran:** sendrom rayı, soru kartları, öneri kartı, spektrum şeridi, statik
- [x] **A4 Hareket:** öneri farkı, neden izi, eleme, spektrum, soru açılması
- [x] **A5 İkinci sendrom:** selülit; motorun genellendiği kanıtlanır
- [x] **A6 Üçüncü ve dördüncü sendrom:** idrar yolu, intraabdominal
- [x] **A7 İngilizce ve bölge katmanı:** `en`, `regions/EU`, `regions/US`, kılavuz seti seçimi
- [x] **A8 Android:** duyarlı düzen, dokunmatik, APK — imza anahtarı kullanıcıda
- [ ] **A9 Uzman gözden geçirmesi ve ilk yayın**

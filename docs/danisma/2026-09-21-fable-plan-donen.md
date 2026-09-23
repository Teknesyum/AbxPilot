## Fable Görüşü: AbxPilot Planı

### En Önemli 3 Değişiklik

1. **Kural biçimini "puanlama" değil "karar tablosu + kısıt" olarak kur.** Ağırlık toplamı puanlama, klinikte savunulamaz ("neden 7.2 değil 6.8?"). Kılavuzlar zaten koşullu tablo: `sendrom × ortam × risk → birinci seçenek / alternatif / alerji dalı`. Motor: (a) sert kısıtlar (alerji, gebelik, etkileşim, renal) adayı eler; (b) kılavuz tablosu sıralı aday listesi verir; (c) izin sadece "hangi satır eşleşti, hangi kısıt eledi". Patojen ağırlığı sadece açıklama katmanı olsun, seçimi belirlemesin — aksi halde iki kaynak (kılavuz vs. ağırlık) çelişince kimse haklı çıkmaz.

2. **Regülasyonu "uyarı metni" değil mimari karar olarak ele al.** Somut tedavi önerisi veren yazılım AB MDR'de Kural 11 → en az sınıf IIa, ölüm/geri dönüşsüz zarar riski olan kararlarda (sepsis, menenjit) IIb. TİTCK MDR'yi aynen uygulıyor. "Reçete değil" ibaresi sınıfı düşürmez; amaç beyanı (intended purpose) düşürür. İki yol: (a) v1'i "kılavuz gezgini + eğitim aracı" olarak konumlandır, hasta-özel doz yerine kılavuz metnini ve dozu referanslı göster, ürün adında/ekranda "öneri" yerine "kılavuz özeti"; (b) baştan IEC 62304 sınıf B belge seti (yazılım gereksinim, risk dosyası ISO 14971, doğrulama) tut. Türkiye pazarı için (a) ile başla, veri/motor tasarımını (b)'ye taşınabilir yap: her kural ID'li, sürümlü, kaynak-tarihli, testli — bu zaten 62304 izlenebilirliğinin yarısı.

3. **Bölgesel direnci baştan ayrı katman yap, kılavuza gömme.** Kılavuz tablosu "MRSA riski varsa" der; "risk var mı" cevabı bölgeye bağlı (yerel MRSA %>10-20 eşiği). Veri modeli: `guideline/` (kaynak kural seti, bölgeden bağımsız) + `region/{TR,EU,US}.yaml` (eşik değerleri, yerel direnç oranı, yerel ilaç erişimi, ruhsat durumu). Motor kuralı `if region.mrsa_rate > threshold` okur, bölgesel dosya onu doldurur. TR için EKMUD/UHESA, dünya için ECDC EARS-Net ve WHO GLASS verileri açık; ihracat formatı CSV/JSON. Bunu sonradan eklemek her kuralı yeniden yazdırır.

### (a) Veri Modeli ve Kural Biçimi

- YAML doğru, ama şema zorunlu: JSON Schema ile derleme zamanında doğrula; test hattında şema hatası = build kırmızı.
- İlaç dozu tek "70 kg" satırı yetmez: doz nesnesi `endikasyon × yol × renal bant` üçlü olsun; v1'de sadece normal renal doldur, alanlar hazır dursun.
- Alerji tek boolean değil: penisilin alerjisi tipi (IgE / gecikmiş / SJS) sefalosporin kararını değiştirir; v1'de üç seçenekli enum yeter.
- "Kural ID + sürüm + kaynak + tarih + son gözden geçirme" her kayıtta zorunlu alan; süresi geçmiş kural ekranda sarı bant.
- Sorular ayrı dosyada olsun ama koşullu görünürlük destekle (`YBÜ=evet` ise ventilatör sorusu çıksın), yoksa 20 soru ekranda boğar.
- Karar tablosunu Excel/CSV'den YAML'a derleyen bir script koy: klinisyen katkısı tablo üstünden gelir, YAML elle yazdırılmaz.

### (b) Tıbbi/Hukuki Risk

- Açılış uyarısı yetmez; her öneri kartında kalıcı "klinik değerlendirme gerektirir" satırı ve kaynak bağlantısı.
- Türk kılavuzları ve EKMUD metinleri de telifli; kural ekstraksiyonu serbest, metin kopyası değil. Her kural için "kaynak bölüm numarası" tut, metni tutma.
- Veri sürümü ekranda + çevrimdışı çalışma: eski veriyle çalışan kopya için "veri X ay eski" uyarısı.
- Kişisel sağlık verisi tutma: cevaplar oturumda kalsın, diske yazılmasın; KVKK yükünü sıfırlar.
- Sepsis/menenjit gibi zaman-kritik sendromları v1'e alma; en yüksek zarar sınıfı, en az tolerans.

### (c) Dünyaya Açılma

- Dil dosyası tek başına yetmez: ilaç adları (INN sabit, ticari ad bölgesel), ölçü birimleri, ruhsatlı ilaç listesi bölgesel katmana girer.
- Kılavuz seçimi kullanıcı ayarı olsun: "IDSA/ATS" vs "NICE" vs "TTD" — aynı sendrom farklı birinci seçenek verebilir, motor kılavuz setini parametre almalı.
- Bölge dosyası olmayan yer için varsayılan "WHO AWaRe + belirsiz direnç" profili ve görünür uyarı.

### (d) Animasyon

- "Öneri değişti" için diff modeli: eski rejim → yeni rejim, değişen alan vurgulanır (ilaç mı, doz mu, süre mi). Tüm kartı yeniden çizmek değişimi gizler.
- Değişimin nedeni animasyonla eşlik etsin: cevaplanan soru satırı → öneri kartına çizgi/renk geçişi, 300-400 ms. Kullanıcı hangi cevabın neyi değiştirdiğini görsün.
- ViewModel'de öneri `ImmutableRecord`; eskisiyle yeni karşılaştırılıp animasyon tetiklenir. Motor senkron ve <50 ms olmalı; yavaşsa animasyon kekeler.
- Avalonia Transitions + Animation sınıfları yeter; Lottie/Skia'ya v1'de girme.

### (e) Avalonia → Android

- Baştan `Core` (motor, veri) / `UI` (Avalonia paylaşılan) / `Desktop` / `Android` dört proje; Android head'i boş bile olsa ilk günden derlensin, sonradan bölmek pahalı.
- Veri dosyaları `AvaloniaResource` ile gömülü, dosya sistemi erişimi yok; Android'de path varsayımı kırar.
- Dokunmatik: soru listesi kaydırılabilir, öneri kartı sabit üstte — telefon ekranında "üst-alt" düzeni yatay bölünmeye dönmeli, responsive breakpoint'i v1'de düşün.
- .NET 9 + Avalonia 11.2+ Android'de sorunsuz; yalnız AOT/trimming ile YAML deserializer (YamlDotNet reflection) kırılabilir — source-generated JSON tercih et, YAML'ı build'de JSON'a derle.

### Küçük Notlar

- Altın vaka testleri iyi; her kılavuz güncellemesinde hangi vakalar kırıldı raporu ekle.
- Spektrum matrisi görsel öğe olarak güçlü, ama veri kaynağı zor; v1'de sendrom-odaklı patojen alt kümesiyle sınırla.
- İlk dilim TKP doğru seçim; ikinci dilim idrar yolu yerine selülit al — daha az bölgesel direnç bağımlılığı, motor genellemesi daha temiz test edilir.

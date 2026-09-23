# Fable'a Giden: AbxPilot Plan Danışması

Proje: AbxPilot. Enfeksiyon uzmanı gibi davranan, ampirik antibiyotik seçimi öneren masaüstü program.
Temel hasta: 70 kg sağlıklı erişkin. Kullanıcı bir sendrom seçer (ör. pnömoni), altta 10-20 soru çıkar
(toplum kökenli mi, YBÜ mü, MRSA riski, Pseudomonas riski, son 90 gün AB, penisilin alerjisi...).
Soruların hepsi isteğe bağlı; her cevapta üstteki öneri canlı olarak değişir. Üstte: seçilen rejim,
doz, yol, süre, "neden bu", alternatifler, spektrum. Önce Türkçe, sonra dünya geneline (çok dil,
bölgesel direnç ve kılavuz) açılacak. Animasyonlu ekran önemli. Avalonia ile çapraz platform,
ileride Android.

Taslak kararlarım:
1. Yığın: .NET 9 + Avalonia 11, MVVM (CommunityToolkit.Mvvm). Görsel tema kitaplığı yok, kendi palet.
   Android için Avalonia.Android hedefi aynı çözümde, paylaşılan Core + UI kitaplığı.
2. Bilgi tabanı kodda değil veride: `data/` altında YAML/JSON — ilaçlar (sınıf, spektrum matrisi,
   doz 70 kg, yol, biyoyararlanım, yan etki, etkileşim, gebelik, renal ayar), patojenler,
   sendromlar, sorular, karar kuralları. Metinler anahtar ile, tr/en dil dosyası ayrı.
3. Motor deterministik, açıklanabilir: sendrom → soru cevapları → olası patojen ağırlıkları →
   kılavuz karar tablosundan aday rejimler → değiştiriciler (alerji, risk faktörleri) → puanlama →
   gerekçe izi. LLM yok.
4. Kaynak: IDSA/ATS, ESCMID, NICE, Türk kılavuzları (TTD, EKMUD). Sanford/UpToDate telifli,
   gömülmez. Her kural kaynağa ve tarihe bağlı.
5. Test: altın vaka testleri (klinik vinyet → beklenen rejim), veri şema doğrulaması.
6. İlk dilim: toplum kökenli pnömoni uçtan uca; sonra idrar yolu, selülit, menenjit, intraabdominal,
   sepsis bilinmeyen odak.
7. Güvenlik: klinik karar desteği, reçete değil; açılış uyarısı; veri sürümü ekranda.

Soru: Bu planda kör nokta ne? Özellikle (a) motorun veri modeli ve kural biçimi, (b) tıbbi/hukuki
risk ve regülasyon (TİTCK, AB MDR yazılım tıbbi cihaz sınıfı), (c) dünya geneline açılırken
bölgesel direnç verisinin nasıl takılacağı, (d) animasyonlu "öneri değişti" deneyimi,
(e) Avalonia→Android yolu. Kısa, maddeli, Türkçe cevap ver; en önemli 3 değişikliği en üste koy.

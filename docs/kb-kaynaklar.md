# Bilgi Tabanı Kaynakları

Bu belge `kb/` altındaki her kaydın hangi kaynaktan geldiğini ve A9 uzman incelemesinde
doğrulanması gereken noktaları listeler. Tüm kayıtlar `review_status: unreviewed`,
`reviewed_at: null` durumundadır; hiçbiri klinik kullanım için onaylı değildir.

Ürün bir kılavuz gezgini ve eğitim aracıdır. Temel hasta sabit: 70 kg sağlıklı erişkin,
normal böbrek işlevi. mg/kg dozlar 70 kg karşılığıyla birlikte tutulur.

## Kaynak Listesi

Kaynaklar `kb/sources.yaml` içindedir.

| Kimlik | Kaynak | Tarih Hassasiyeti |
|---|---|---|
| `idsa-ats-2019` | Metlay JP ve ark. IDSA/ATS erişkin TKP kılavuzu, AJRCCM 2019;200(7):e45-e67 | Gün |
| `ttd-2009` | Türk Toraks Derneği erişkin TKP uzlaşı raporu, Toraks Dergisi 2009;10(Ek 9) | Yıl |
| `who-aware-2023` | DSÖ AWaRe sınıflaması 2023 | Ay |
| `credible-meds` | CredibleMeds QTdrugs listesi | Yıl |
| `zagursky-2018` | Zagursky ve Pichichero, beta-laktam çapraz reaksiyonu, JACI Pract 2018 | Ay |
| `product-labels` | Etkin maddelerin ürün bilgileri (KÜB / prospektüs) | Yıl |
| `who-caesar` | DSÖ Avrupa CAESAR sürveyans raporu | Yıl |
| `abxpilot-draft` | Proje taslağı, birincil kaynak yok | Gün |

## Kayıt Türüne Göre Kaynak

| Dosya | Kayıt | Kaynak |
|---|---|---|
| `drugs/*.yaml` | 22 ilaç: sınıf, yol, doz, biyoyararlanım | Doz: `idsa-ats-2019` Tablo 1, 3, 4; tabloda yoksa `product-labels` |
| `drugs/*.yaml` | AWaRe grubu | `who-aware-2023` |
| `drugs/*.yaml` | QT riski | `credible-meds` |
| `drugs/*.yaml` | Beta-laktam R1 yan zinciri | `zagursky-2018` |
| `drugs/*.yaml` | Gebelik, yan etki, etkileşim | `product-labels` |
| `pathogens/*.yaml` | 12 patojen | `abxpilot-draft` |
| `regimens/cap.yaml` | 45 rejim | Çoğu `idsa-ats-2019`; iki apse rejimi `abxpilot-draft` |
| `questions/*.yaml` | 15 soru | TKP soruları `idsa-ats-2019` S7-S14; ortak sorular `abxpilot-draft` |
| `syndromes/cap.yaml` | TKP, türetilmiş bayraklar `severe`, `comorbid` | `idsa-ats-2019` Tablo 1 ve S7 |
| `guidelines/idsa-ats-2019/cap.csv` | 16 satır | `idsa-ats-2019`; iki not satırı `abxpilot-draft` |
| `guidelines/ttd-2009/cap.csv` | 3 satır, IDSA setini genişletir, etkin 17 satır | `ttd-2009` |
| `spectrum/cap.csv` | 22 ilaç × 12 patojen | `abxpilot-draft` |
| `regions/TR.yaml` | Direnç, eşikler, ruhsat | `who-caesar`, `abxpilot-draft` |
| `scoring.yaml` | Karşılaştırma puanı ağırlıkları | `abxpilot-draft` |

## Belirsiz Noktalar

Her madde "belirsiz"dir ve A9'da birincil kaynakla doğrulanmalıdır. Toplam 26 madde.

### En Önemli Beş Nokta

1. **Belirsiz — TTD satırları:** `ttd-2009` setindeki üç satırın (`out_comorbid`, `ward`,
   `psa_fq`) içeriği bellekten yazıldı. Florokinolonun tüberküloz maskeleme gerekçesiyle geri
   plana alınması, kombinasyon önceliği ve Pseudomonas için levofloksasin eki doğrulanmadı.
2. **Belirsiz — TTD sürümü:** 2009 uzlaşı raporundan daha yeni bir sürüm olup olmadığı,
   bölüm ve tablo numaraları bilinmiyor.
3. **Belirsiz — TR makrolid direnci:** `spn_macrolide` "%25 üstü" kategorisinde, sayısal değer
   boş. Rakam, yıl ve CAESAR baskısı doğrulanmadı. Bu değer makrolid tekli tedavisinin
   dışlanmasını belirler.
4. **Belirsiz — Spektrum matrisi:** `spectrum/cap.csv` tümüyle proje taslağıdır; hiçbir hücre
   bir kaynaktan alınmadı.
5. **Belirsiz — TR ruhsat durumu:** seftarolin, sefpodoksim, sefotaksim ve aztreonam
   `unknown`; IV doksisiklin ve IV klaritromisin erişilebilirliği doğrulanmadı. TİTCK ürün
   listesiyle karşılaştırılmalı.

### Diğer Noktalar

6. **Belirsiz:** IDSA soru (S7-S14) ve tablo numaralarının satırlara eşlemesi.
7. **Belirsiz:** Yoğun bakıma yatışın doğrudan "ağır" sayılması (`severe` türetimi).
8. **Belirsiz:** Ayaktan sağlıklı hasta satırında amoksisilin güçlü, doksisiklin ve makrolid
   koşullu öneri olarak ayrı basamaklara konması.
9. **Belirsiz:** Ağır TKP'de beta-laktam + makrolid ile beta-laktam + florokinolonun aynı
   basamakta tutulması.
10. **Belirsiz:** Apse veya ampiyemde ampisilin-sulbaktam (yatan) ve amoksisilin-klavulanat
    (ayaktan) seçimi; kılavuz yalnız anaerop kapsamı ister, ajan seçimi taslaktır.
11. **Belirsiz:** MRSA ve Pseudomonas düzenleyicilerinin yalnız servis ve yoğun bakımda
    uygulanması; ayaktan hastada yalnız not satırı gösterilmesi.
12. **Belirsiz:** `recent_abx_class` sorusunun ifadesi ve motorda başka sınıfı tercih etme
    kullanımı.
13. **Belirsiz:** TR MRSA oranı; invaziv izolat verisi TKP'ye özgü değil, kategori bilinmiyor.
14. **Belirsiz:** TR eşikleri: makrolid direnci 0,25; son yatış 90 gün; tedavi süreleri 5 ve
    7 gün.
15. **Belirsiz:** Aztreonam AWaRe grubu bellekten yazıldı.
16. **Belirsiz:** Ampisilin-sulbaktam AWaRe grubu.
17. **Belirsiz:** Sefuroksim ve seftarolinin R1 yan zinciri grupları.
18. **Belirsiz:** CredibleMeds erişim tarihi yaklaşık; liste sürekli güncellenir.
19. **Belirsiz:** Oseltamivirin QT risk sınıfı.
20. **Belirsiz:** İlaçların gebelik kategorileri; ürün bilgisi sürümleri kaydedilmedi.
21. **Belirsiz:** Oral biyoyararlanım kategorileri (yüksek, orta, düşük).
22. **Belirsiz:** IV klaritromisin dozu; IDSA yol belirtmeden 2 × 500 mg verir.
23. **Belirsiz:** Oral linezolid dozu ürün bilgisinden alındı, IDSA tablosunda yok.
24. **Belirsiz:** Vankomisin 15 mg/kg, 70 kg karşılığı 1050 mg (yaklaşık 1 g); yükleme dozu
    ve düzey hedefi kaydedilmedi.
25. **Belirsiz:** Patojen kayıtlarının tümü taslaktır (Gram, tipik direnç mekanizmaları).
26. **Belirsiz:** Karşılaştırma puanı ağırlıkları (0,30 / 0,20 / 0,15 / 0,10 / 0,10 / 0,15 / 0)
    proje taslağıdır; yalnız aynı basamak içinde sıralama yapar.

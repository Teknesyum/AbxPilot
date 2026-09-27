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
| `ttd-2021` | Türk Toraks Derneği erişkin TKP tanı ve tedavi uzlaşı raporu 2021 (Tablo 5, 7, Ek 1) | Yıl |
| `ttd-2009` | TTD 2009 uzlaşı raporu, Toraks Dergisi 2009;10(Ek 9); tarihsel kayıt, `ttd-2021` ile değişti | Yıl |
| `who-aware-2023` | DSÖ AWaRe sınıflaması 2023 | Ay |
| `credible-meds` | CredibleMeds QTdrugs listesi | Yıl |
| `zagursky-2018` | Zagursky ve Pichichero, beta-laktam çapraz reaksiyonu, JACI Pract 2018 | Ay |
| `product-labels` | Etkin maddelerin ürün bilgileri (KÜB / prospektüs) | Yıl |
| `who-caesar` | ECDC/DSÖ Avrupa AMR sürveyans raporu 2023 (2021 verisi), Türkiye s.137 | Yıl |
| `titck-2026` | TİTCK Ruhsatlı Beşeri Tıbbi Ürünler Listesi, 18.09.2026 | Gün |
| `aaaai-acaai-2022` | Khan DA ve ark., ilaç alerjisi uygulama parametresi, JACI 2022;150(6):1333-1393 | Ay |
| `eucast-expected-2023` | EUCAST Expected Resistant Phenotypes v1.2 | Yıl |
| `ashp-vanco-2020` | Rybak ve ark., vankomisin konsensüsü, AJHP 2020;77(11):835-864 | Ay |
| `idsa-ssti-2014` | Stevens DL ve ark. IDSA deri ve yumuşak doku enfeksiyonu kılavuzu, CID 2014;59(2):e10-e52 | Gün |
| `nice-ng141-2019` | NICE NG141, selülit ve erizipel: antimikrobiyal reçeteleme, 2019 | Gün |
| `idsa-uti-2011` | Gupta K ve ark. IDSA/ESCMID akut komplike olmayan sistit ve piyelonefrit, CID 2011;52(5):e103-e120 | Ay |
| `idsa-cuti-2025` | IDSA komplike idrar yolu enfeksiyonu kılavuzu, 2025 | Gün |
| `eau-uti-2026` | EAU Ürolojik Enfeksiyonlar kılavuzu 2026 | Yıl |
| `sis-idsa-2010` | Solomkin JS ve ark. SIS/IDSA komplike intraabdominal enfeksiyon, CID 2010;50(2):133-164 | Gün |
| `sis-2017` | Mazuski JE ve ark. SIS intraabdominal enfeksiyon kılavuzu revizyonu, Surg Infect 2017;18(1):1-76 | Ay |
| `sis-2024` | SIS intraabdominal enfeksiyon kılavuzu 2024 güncellemesi; okunmadı, yalnız kayıt | Ay |
| `ekmud-iai-2016` | EKMUD/TCD komplike intraabdominal enfeksiyon rehberi, Ulus Cerrahi Derg 2016 | Yıl |
| `cinislioglu-2024` | Cinislioğlu AE ve ark., Türkiye üropatojen direnç verisi, Mikrobiyol Bul 2024 | Ay |
| `ecdc-ears-net-2024` | ECDC EARS-Net yıllık epidemiyolojik rapor, 2024 verisi (yayın 18.11.2025), Tablo 9b | Gün |
| `ema-authorisation` | EMA, ilaç ruhsatlandırma yolları (merkezi ve ulusal) | Gün |
| `cdc-abcs-spn-2024` | CDC ABCs S. pneumoniae sürveyans raporu 2024 | Ay |
| `cdc-nhsn-2018-2021` | CDC NHSN HAI patojen ve direnç raporu 2018-2021 | Yıl |
| `weiner-lastinger-2020` | Weiner-Lastinger ve ark., NHSN 2015-2017, ICHE 2020;41(1):1-18 | Ay |
| `kaye-2021` | Kaye ve ark., ABD ayaktan idrar E. coli direnci 2011-2019, CID 2021;73(11):1992-1999 | Ay |
| `fda-drugsfda-2026` | Drugs@FDA, openFDA sorgusu 24.09.2026 (`docs/danisma/2026-09-24-openfda-sorgusu.txt`) | Gün |
| `idsa-gas-2012` | Shulman ST ve ark. IDSA A grubu streptokok farenjiti kılavuzu 2012, CID 2012;55(10):e86-e102 | Gün |
| `nice-ng84-2018` | NICE NG84, akut boğaz ağrısı: antimikrobiyal reçeteleme, 26.01.2018 | Gün |
| `titck-akilci-2020` | TİTCK, Erişkin Hastada Antibiyotik Kullanımına Akılcı Yaklaşım, 2020, Konu 3 (Candevir Ulu A); ISBN 978-975-590-796-3 | Yıl |
| `abxpilot-draft` | Proje taslağı, birincil kaynak yok | Gün |

## Kayıt Türüne Göre Kaynak

| Dosya | Kayıt | Kaynak |
|---|---|---|
| `drugs/*.yaml` | 23 ilaç: sınıf, yol, doz, biyoyararlanım | Doz: `idsa-ats-2019` Tablo 1-3; siprofloksasin ve IV klaritromisin `ttd-2021` Ek 1; vankomisin yüklemesi `ashp-vanco-2020`; tabloda yoksa `product-labels` |
| `drugs/*.yaml` | AWaRe grubu | `who-aware-2023` |
| `drugs/*.yaml` | QT riski | `credible-meds` |
| `drugs/*.yaml` | Beta-laktam R1 yan zinciri | `zagursky-2018` |
| `drugs/*.yaml` | Gebelik, yan etki, etkileşim | `product-labels` |
| `pathogens/*.yaml` | 12 patojen | `abxpilot-draft` |
| `regimens/cap.yaml` | 82 rejim | Çoğu `idsa-ats-2019`; siprofloksasinli rejimler `ttd-2021` Tablo 7; iki apse rejimi `abxpilot-draft` |
| `questions/*.yaml` | 15 soru | TKP soruları `idsa-ats-2019` S7-S14; ortak sorular `abxpilot-draft` |
| `syndromes/cap.yaml` | TKP, türetilmiş bayraklar `severe`, `comorbid` | `idsa-ats-2019` Tablo 1 ve S7; `severe` yalnız 1 majör ya da ≥3 minör ölçüt |
| `guidelines/idsa-ats-2019/cap.csv` | 16 satır | `idsa-ats-2019`; iki not satırı `abxpilot-draft` |
| `guidelines/ttd-2021/cap.csv` | 3 satır, IDSA setini genişletir, etkin 17 satır | `ttd-2021` Tablo 5 ve 7 |
| `spectrum/cap.csv` | 23 ilaç × 12 patojen | `abxpilot-draft`; üç `none` hücresi `eucast-expected-2023` |
| `regions/TR.yaml` | Direnç, eşikler, ruhsat | Direnç `who-caesar`; eşikler `ttd-2021`; ruhsat `titck-2026` |
| `constraints.yaml` | Sert ve yumuşak kısıtlar | Alerji `aaaai-acaai-2022`; son 3 ay sınıf kuralı `ttd-2021` Tablo 5 |
| `drugs/*.yaml` | A5/A6 ile 22 yeni ilaç (toplam 45); sendrom dozları `ssti_*`, `uti_*`, `iai_*` kimlikli | İlgili kılavuz tabloları; tabloda yoksa `product-labels` |
| `guidelines/idsa-2014`, `nice-ng141-2019` / `ssti.csv` | 19 satır | `idsa-ssti-2014` Şekil 1, Tablo 1-2; `nice-ng141-2019` Tablo 1 |
| `guidelines/idsa-2011-2025`, `eau-2026` / `uti.csv` | 22 satır | `idsa-uti-2011` Tablo 4, `idsa-cuti-2025`; `eau-uti-2026` Bölüm 3.4-3.7 |
| `guidelines/ekmud-2016`, `sis-2017`, `sis-idsa-2010` / `iai.csv` | 30 satır | `ekmud-iai-2016`, `sis-2017`, `sis-idsa-2010` |
| `spectrum/{ssti,uti,iai}.csv` | 22×6, 19×6, 21×8 | `eucast-expected-2023` ile uyumlu; diğer hücreler `abxpilot-draft` |
| `regions/TR.yaml` v3 | TMP-SMX, FQ, GSBL eşik bayrakları; yeni ruhsat satırları | `cinislioglu-2024`, `who-caesar`; ruhsat `titck-2026` |
| `scoring.yaml` | Karşılaştırma puanı ağırlıkları | `abxpilot-draft` |
| `regions/EU.yaml` | Direnç: makrolid %19,0, MRSA %14,2, E. coli 3. kuşak sefalosporin %16,0, FQ %22,5, K. pneumoniae karbapenem %11,3; ruhsat hep `unknown` | `ecdc-ears-net-2024`; ruhsat notu `ema-authorisation` |
| `regions/US.yaml` | Direnç: pnömokok eritromisin %23,9; MRSA %44,9 (YBÜ CLABSI); E. coli FQ %21,1, TMP-SMX %25,4, GSBL %6,4; Klebsiella karbapenem %6,9; ruhsat FDA | `cdc-abcs-spn-2024`, `cdc-nhsn-2018-2021`, `kaye-2021`, `weiner-lastinger-2020`; ruhsat `fda-drugsfda-2026` |
| `regions/OTHER.yaml` | Direnç verisi yok, ruhsat hep `unknown`; kartta görünür uyarı | `who-aware-2023` |
| `syndromes/pharyngitis.yaml`, `questions/pharyngitis.yaml` | Farenjit, `order: 1`; 9 soru; Centor, McIsaac ve FeverPAIN türetilmiş bayrakları | `titck-akilci-2020` Tablo 3.3; `nice-ng84-2018` Terimler, 1.1.3; `idsa-gas-2012` Öneri 1-2 |
| `guidelines/idsa-2012`, `nice-ng84-2018`, `titck-2020` / `pharyngitis.csv` | 5 + 4 + 6 satır; `titck-2020` ulusal set (`region: tr`) | `idsa-gas-2012` Öneri 1-9, Tablo 2; `nice-ng84-2018` 1.1.6-1.1.13, Tablo 1; `titck-akilci-2020` Konu 3.5-3.7, Tablo 3.3-3.4 |
| `regimens/pharyngitis.yaml`, `drugs/*.yaml` | 19 rejim; `phar_*` dozları; yeni ilaç benzatin benzilpenisilin | `idsa-gas-2012` Tablo 2; `titck-akilci-2020` Tablo 3.4; `nice-ng84-2018` Tablo 1 |
| `pathogens/s_pyogenes.yaml`, `spectrum/pharyngitis.csv` | A grubu streptokok; 9 ilaç × 1 patojen | `idsa-gas-2012`; spektrum `abxpilot-draft` |
| `regions/*.yaml` | Benzatin benzilpenisilin ruhsatı: TR J01CE08 8 etkin ürün, ABD Bicillin L-A (NDA050141) | `titck-2026`, `fda-drugsfda-2026` |

## Belirsiz Noktalar

26 maddenin durumu Eylül 2026 araştırmasından sonra (`docs/kb-arastirma-2026-09.md`,
değişiklik listesi `docs/kb-degisiklik-2026-09.md`). "Çözüldü" maddeler kaynağıyla
işlendi; yine de A9 uzman onayı bekler. "Açık" maddeler belirsiz kalır.

### Çözülenler

1. **Çözüldü — TTD satırları:** `ttd-2021` Tablo 5 (Grup 1b) ve Tablo 7'den yeniden yazıldı.
   Pseudomonas satırı siprofloksasin; servis satırında seftarolin yok. Aminoglikozid +
   makrolid alternatifi modellenmedi (bilgi tabanında aminoglikozid yok).
2. **Çözüldü — TTD sürümü:** set `ttd-2021`; `ttd-2009` tarihsel kaynak olarak duruyor.
   2021'den yeni baskı bulunamadı (orta güven).
3. **Çözüldü — TR makrolid direnci:** %34,1 (2021, n=126), `who-caesar` 2023 raporu s.137.
5. **Çözüldü — TR ruhsat durumu:** `titck-2026`. Seftarolin, tek başına aztreonam ve IV
   doksisiklin ruhsatsız; sefpodoksim, sefotaksim, IV klaritromisin ruhsatlı.
7. **Çözüldü — YBÜ yatışı:** `severe` artık yalnız IDSA Tablo 1 ölçütlerinden türer.
12. **Çözüldü — Son 3 ay sınıf kuralı:** `ttd-2021` Tablo 5. Motor elemez, başka sınıfı öne
    alır ve uyarı yazar; yatan ve ağır hastada beta-laktam omurgası korunur.
13. **Çözüldü — TR MRSA oranı:** %30,7 (2021, invaziv), yalnız bilgi. MRSA bayrağı IDSA 2019
    risk etkenlerine bağlı; bölge oranı motoru tetiklemez.
22. **Çözüldü — IV klaritromisin dozu:** `ttd-2021` Ek 1, 2 × 500 mg.
24. **Çözüldü — Vankomisin:** 20-35 mg/kg yükleme (ağır hasta), AUC 400-600 hedefi,
    `ashp-vanco-2020`. Konsensüs PDF'i açılmadı; A9'da doğrulanmalı.

### Kısmen Çözülenler

4. **Kısmen — Spektrum matrisi:** üç yanlış hücre `none` oldu (seftazidim × pnömokok,
   vankomisin ve linezolid × anaerop, `eucast-expected-2023`). Tartışmalı iki hücre
   (azitromisin × H. influenzae, doksisiklin × MRSA) ve matrisin geri kalanı A9'a kaldı.
6. **Kısmen — IDSA eşlemesi:** tablo numaraları düzeltildi (Tablo 3→2, 4→3). Soru
   numaralarının satır satır doğrulaması açık.
14. **Kısmen — TR eşikleri:** değerler aynı, kaynak `ttd-2021`. Makrolid eşiği 0,25 IDSA'dan.

### Açık Kalanlar

8. **Açık:** Ayaktan sağlıklı hastada amoksisilin güçlü, doksisiklin ve makrolid koşullu
   öneri olarak ayrı basamaklarda.
9. **Açık:** Ağır TKP'de beta-laktam + makrolid ile beta-laktam + florokinolon aynı basamakta.
10. **Açık:** Apse veya ampiyemde ampisilin-sulbaktam ve amoksisilin-klavulanat seçimi.
11. **Açık:** MRSA ve Pseudomonas düzenleyicilerinin yalnız serviste ve yoğun bakımda
    uygulanması.
15. **Açık:** Aztreonam AWaRe grubu (TR'de tek başına ruhsatsız, etkisi düştü).
16. **Açık:** Ampisilin-sulbaktam AWaRe grubu; araştırmada düşük güven.
17. **Açık:** Sefuroksim ve seftarolin R1 grupları; yeniden adlandırma önerisi düşük güven.
18. **Açık:** CredibleMeds erişim tarihi.
19. **Açık:** Oseltamivir QT sınıfı; kaynak bulunamadı.
20. **Açık:** Gebelik kategorileri; ürün bilgisi sürümleri kaydedilmedi.
21. **Açık:** Oral biyoyararlanım kategorileri.
23. **Açık:** Oral linezolid dozu; IDSA Tablo 3 satırı doğrulanmadan kaynak değişmedi.
25. **Açık:** Patojen kayıtları taslak.
26. **Açık:** Karşılaştırma puanı ağırlıkları taslak.
27. **Açık (A6):** IDSA 2011 Tablo 4 amoksisilin-klavulanat, sefpodoksim, siprofloksasin ve levofloksasin sistit dozları ikincil okumadan.
28. **Açık (A6):** EAU levofloksasin IV rejimi için TR IV ruhsatı ve etiket dozu.
29. **Açık (A5):** Pürülan DYDE'de 5 günlük süre IDSA'da açık yazmıyor; taslak.
30. **Açık (A6):** SIS 2024 güncellemesi okunmadı; SIS setleri 2017 ve 2010'a dayanır.
31. **Açık (A6):** TR TMP-SMX direnci 394/1503 hesabı %26,2, makalede %26,9.
32. **Açık (A6):** Pivmesilinam AWaRe grubu doğrulanmadı.
33. **Açık (A6):** Teikoplanin ve linezolid İAE dozları ürün bilgisinden, kılavuzdan değil.
34. **Açık (A6):** İAE için GSBL ve FQ eşikleri üriner veriden aktarıldı; İAE'ye özgü TR verisi yok.
35. **Açık (A6):** EKMUD madde 20 okuması (hafif toplum kökenli İAE'de ertapenem öncelliği).
36. **Açık (A6):** IDSA 2025 cUTI DOI ve sayfa numaraları; web sürümü kullanıldı.
37. **Açık (A6):** IDSA setinde GSBL riski yalnız not; ilk seçim seftriakson kalır. EAU setinde karbapeneme geçer.
38. **Açık (A5):** NICE MRSA eki oral hafif selülite de IV vankomisin ekler; NICE oral MRSA seçeneği vermiyor.
39. **Açık (A7):** AB oranları invaziv izolatlardan; TKP, İYE ve İAE'ye özgü değil. Ülke aralığı geniş (makrolid %4,0-44,2).
40. **Açık (A7):** EARS-Net E. coli TMP-SMX direncini raporlamıyor; AB'de soru temkinli statik varsayılanla (≥%20) işlenir.
41. **Açık (A7):** AB ruhsatı ülkeye göre değişir; bütün ilaçlar `unknown`, ulusal liste doğrulanmadı.
42. **Açık (A7):** ABD pnömokok makrolid direnci %23,9, eşiğin hemen altında ve invaziv izolatlardan; IDSA/ATS 2019'un ABD oranı ifadesiyle karşılaştırılmadı.
43. **Açık (A7):** ABD toplum kökenli MRSA oranı bulunamadı; kayıt yalnız hastane (NHSN) verisi, tetikleyici değil.
44. **Açık (A7):** ABD E. coli oranları CDC değil, Kaye 2021 (BD veritabanı, 2011-2019); daha yeni ulusal oran bulunamadı.
45. **Açık (A7):** ABD'de 2017 sonrası tür düzeyinde karbapenem direnci bulunamadı.
46. **Açık (A7):** Sefotaksim ABD'de yalnız Claforan ile listeli, diğerleri piyasadan çekilmiş; `unknown` bırakıldı.
47. **Açık (A7):** Bölge dosyası olmayan ülke için "Diğer" profili direnç verisi taşımaz; yerel veri eklenene dek sorular statik varsayılanla işlenir.
48. **Açık (Farenjit):** NICE FeverPAIN 2-3: kılavuz "antibiyotik yok ya da yedek reçete" diyor; motor yedek reçeteyi modellemez, sonuç antibiyotik yok.
49. **Açık (Farenjit):** NICE setinde süre 5 gün; Tablo 1 fenoksimetilpenisilin için 5-10 gün veriyor.
50. **Açık (Farenjit):** NICE seti test sonucunu kullanmaz; yüksek skorda negatif test olsa da penisilin V çıkar.
51. **Açık (Farenjit):** IDSA setinde test yapılmamışsa sonuç "önce test" ve antibiyotik yok; açılış varsayılanında IDSA penisilin V göstermez.
52. **Açık (Farenjit):** IDSA setindeki sevk satırı NICE 1.1.13'e dayanır; IDSA 2012 süpüratif komplikasyonu anar ama sevk kuralı vermez.
53. **Açık (Farenjit):** Skor soruları birleştirildi: öksürük yokluğu ile "öksürük ya da nezle yok"; şiş/eksüdalı tonsil ile pürülans; >38 °C ateş ile son 24 saatte ateş.
54. **Açık (Farenjit):** TİTCK McIsaac ≥4 ve negatif test: kitap bu durumu yazmıyor; antibiyotik yok olarak modellendi.
55. **Açık (Farenjit):** IDSA setinde öksürük/nezle varsa pozitif testte bile antibiyotik yok (viral bulgu, Öneri 4).
56. **Açık (Farenjit):** Penisilin V kartta 500 mg 12 saatte bir; 250 mg 6 saatte bir notta. TR'deki 1000 mg tablet 600 mg penisilin V içerir, ürün eşlemesi yapılmadı.
57. **Açık (Farenjit):** Benzatin penisilin G tek doz; şemada 24 saat aralık ve 1 gün süre olarak gösterilir.
58. **Açık (Farenjit):** S. pyogenes makrolid kapsamı `variable` taslak; bölge dosyalarında GAS makrolid direnç oranı yok.
59. **Açık (Farenjit):** Uygulama ilk kurulumda farenjitle açılır; kayıtlı son sendrom varsa o açılır.
60. **Açık (Farenjit):** 15 yaş altı modellenmedi (McIsaac 3-14 yaş +1); yalnız erişkin.

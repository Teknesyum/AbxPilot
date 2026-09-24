# Bilgi Tabanı Araştırması Eylül 2026

`docs/kb-kaynaklar.md` içindeki 26 belirsiz noktanın birincil kaynak taraması.
Erişim tarihi tüm URL'ler için 2026-09-24. Metin alıntısı yok, yalnız bulgu.

Bu belge öneri listesidir; `kb/` altında hiçbir dosya değiştirilmedi. Öneriler A9
uzman incelemesinde onaylanmadan uygulanmamalı.

## Özet

| Durum | Sayı | Noktalar |
|---|---|---|
| Çözüldü | 9 | 1, 2, 3, 5, 8, 12, 13, 14, 22 |
| Kısmen | 13 | 4, 6, 7, 9, 10, 11, 15, 16, 17, 20, 21, 23, 24 |
| Bulunamadı | 4 | 18, 19, 25, 26 |

## Kaynaklar

| Kısa Ad | Künye | URL | Yıl |
|---|---|---|---|
| TTD 2021 | Erişkinlerde Toplumda Gelişen Pnömoniler Tanı ve Tedavi Uzlaşı Raporu 2021, ed. Sayıner A, Babayiğit C, ISBN 978-605-74980-6-9 | http://web.archive.org/web/20250429085342/https://toraks.org.tr/site/sf/books/2022/12/7f996a9b650d792ae0096c78c08245664f5e4c2749be689f3470a2c881242b37.pdf (özgün toraks.org.tr bağlantısı 404) | 2021 |
| TTD 2009 | Toraks Dergisi 2009;10(Ek 9), Haziran 2009 | toraks.org.tr arşivi | 2009 |
| ECDC/WHO 2023 | Antimicrobial resistance surveillance in Europe 2023 – 2021 data, Türkiye ülke sayfası s.137 | https://www.ecdc.europa.eu/sites/default/files/documents/Antimicrobial%20resistance%20surveillance%20in%20Europe%202023%20-%202021%20data.pdf | 2023 |
| ECDC/WHO 2022 | Antimicrobial resistance surveillance in Europe 2022 – 2020 data | https://www.ecdc.europa.eu/sites/default/files/documents/Joint-WHO-ECDC-AMR-report-2022.pdf | 2022 |
| SOAR | Torumkuney ve ark., JAC 2022;77 Suppl 1:i51 (Türkiye 2015-17) | https://academic.oup.com/jac/article/77/Supplement_1/i51/6692272 | 2022 |
| TİTCK | Ruhsatlı Beşeri Tıbbi Ürünler Listesi, 18.09.2026 | https://titck.gov.tr/storage/Archive/2026/dynamicModulesAttachment/RuhsatlBeeriTbbirnlerListesi18.09.2026_a468760e-e504-4e47-85b6-17445c76abff.xlsx (sayfa: https://www.titck.gov.tr/dinamikmodul/85) | 2026 |
| EUCAST | Expected Resistant Phenotypes v1.2 | https://www.eucast.org/bacteria/important-additional-information/expected-phenotypes/ | 2023 |
| AWaRe 2023 | WHO AWaRe classification 2023 (WHO-MHP-HPS-EML-2023.04) | https://www.who.int/publications/i/item/WHO-MHP-HPS-EML-2023.04 | 2023 |
| IDSA/ATS 2019 | Metlay ve ark., AJRCCM 2019;200(7):e45-e67 | https://pmc.ncbi.nlm.nih.gov/articles/PMC6812437/ | 2019 |

Güven ölçeği: **yüksek** birincil belge okundu ve rakam/ifade doğrudan eşleşti;
**orta** birincil belge kısmen okundu ya da ikincil kaynakla desteklendi;
**düşük** yalnız genel bilgiye dayanıyor, birincil belge bu turda açılamadı.

## En Önemli Beş Nokta

### 1. TTD Satırları

Mevcut: `kb/guidelines/ttd-2009/cap.csv` üç satır, section "Treatment section (number uncertain)".

- `out_comorbid`: cxm/amc/cpd + azm/clr/dox po; sonra lvx_po/mxf_po; 5 gün.
- `ward`: sam/cro/ctx/cpt + azm/clr iv; sonra lvx_iv/mxf_iv; sonra cro_dox.
- `psa_fq`: lvx_iv; 7 gün.

Bulunan (TTD 2009 Tablo 8, TTD 2021 Tablo 5 ve 7):

- Kinolonun geri plana alınması doğrulandı. İki sürüm de tüberküloz olasılığında kinolon
  kullanılmamasını söyler. 2021'de kinolon monoterapisi Grup 1b'de yalnız GİS sorunu, ilaç
  alerjisi ya da son 3 ayda beta-laktam kullanımında.
- Oral sefalosporin: 2009 sefuroksim, sefprozil, sefaklor, sefiksim, sefditoren sayar;
  sefpodoksim adı geçmez. 2021 genel "2.-3. kuşak oral sefalosporin" der.
- Servis (risk yok): 3. kuşak anti-Pseudomonas olmayan sefalosporin (seftriakson, sefotaksim)
  ya da BL+BLİ, + makrolid; ya da solunum kinolonu monoterapisi. **Seftarolin geçmez.**
- Pseudomonas riski: anti-Pseudomonas beta-laktam + **siprofloksasin**, ya da + aminoglikozid
  + makrolid. Levofloksasin değil. 2009 Tablo 8 Grup IIIB de siprofloksasin (1500 mg/gün).
- YBÜ (2021): kinolon monoterapisi yok; 3KSef/BL+BLİ + makrolid ya da + solunum kinolonu.
- Süre: en az 5 gün, dirençli etkende 7 gün.

Güven: yüksek (iki PDF okundu).

Önerilen değişiklik:

- `kb/guidelines/ttd-2009/cap.csv` → yeni set `kb/guidelines/ttd-2021/cap.csv`, source `ttd-2021`.
- `psa_fq`: `lvx_iv` → siprofloksasin IV (kb'de ilaç yoksa `ciprofloxacin` eklenmeli) ve
  anti-Pseudomonas beta-laktamla birlikte; alternatif satır BL + aminoglikozid + makrolid.
- `ward`: `cpt` çıkarılmalı.
- `out_comorbid`: `cpd` kalabilir ama not "genel 2.-3. kuşak oral sefalosporin, örnek"
  olmalı; section `Tablo 5, Grup 1b`.
- Section alanları: `out_comorbid` → "Tablo 5"; `ward` ve `psa_fq` → "Tablo 7";
  source_date `2021-01-01`.

### 2. TTD Sürümü

Mevcut: `ttd-2009`, Toraks Dergisi 2009;10(Ek 9).

Bulunan: 2021 uzlaşı raporu var (AHEF, EKMUD, KLİMİK, KLİMUD, TİHUD katkılı; PDF klasörü
2022/12). 2021'den yeni ayrı baskı bulunamadı; toraks.org.tr'de 18 Haziran tarihli bir
duyuru var, aynı rapora mı ait belirsiz.

2021 numaraları:

- Şekil 1 (s.8): klinik özelliğe göre algoritma.
- Tablo 5 (s.10): Grup 1a/1b etken ve antibiyotik.
- Tablo 6 (s.11): YBÜ ölçütleri (1 majör ya da ≥3 minör).
- Tablo 7 (s.12): yatan hasta etken ve antibiyotik.
- Tablo 8 (s.13): ardışık tedavi. Ek 1: dozlar.

2009 numaraları: Tablo 5 değiştirici faktörler, Tablo 6 YBÜ ölçütleri, Tablo 8 ampirik
tedavi, Tablo 9 anti-Pseudomonas beta-laktamlar, Tablo 10 ardışık tedavi, Ek 1 doz, Ek 2
pnömokok penisilin direnci (%7-40).

Güven: yüksek (2021 sürümü), orta (daha yeni sürüm olmadığı).

Önerilen değişiklik: `kb/sources.yaml` → yeni kayıt `ttd-2021` (künye yukarıda, date
`2021-01-01`, url Wayback bağlantısı). `ttd-2009` tarihsel kayıt olarak kalabilir.

### 3. TR Makrolid Direnci

Mevcut: `kb/regions/TR.yaml` → `spn_macrolide` category `ge25`, estimate `null`,
source `who-caesar`, source_date `2020-01-01`.

Bulunan (CAESAR, invaziv S. pneumoniae, Türkiye, makrolid dirençli):

| Yıl | Oran | n |
|---|---|---|
| 2016 | %39,3 | 163 |
| 2017 | %39,5 | 205 |
| 2018 | %37,3 | 217 |
| 2019 | %37,0 | 211 |
| 2020 | %34,5 | 119 |
| 2021 | %34,1 | 126 |

Destek: SOAR 2015-17 ayaktan izolatlarda makrolid duyarlılığı %52 (CLSI); TTD 2021 %33-51
aralığı verir ve makrolid monoterapisini bu gerekçeyle önermez. Aynı raporda penisilin
non-wild-type 2021 %53,7 (n=147).

Güven: yüksek.

Önerilen değişiklik: `kb/regions/TR.yaml` → `spn_macrolide.estimate: 0.341`,
`source_date: 2021-12-31` (veri yılı), section "Türkiye ülke sayfası s.137", category
`ge25` aynı kalır. Not: invaziv izolat, TKP'ye özgü değil.

### 4. Spektrum Matrisi

Mevcut: `kb/spectrum/cap.csv` 22 × 12, source `abxpilot-draft`.

EUCAST beklenen direnç tablosuna göre `none` olması gereken ya da doğru `none` olan
hücreler ayrı bölümde (bkz. Kapsam Matrisi). Açıkça yanlış hücre sayısı 3, tartışmalı 2.

Güven: orta. EUCAST sayfası ve genel içerik biliniyor; v1.2 PDF bu turda tablo tablo
açılmadı. Matrisin geri kalanı (reliable/variable ayrımı) için birincil kaynak
bulunamadı; bu klinik yargıdır, A9'a kalır.

Önerilen değişiklik: `kb/spectrum/cap.csv` hücreleri aşağıdaki listeye göre; source
EUCAST beklenen fenotip hücrelerinde `eucast-expected-2023` (yeni kaynak kaydı).

### 5. TR Ruhsat Durumu

Mevcut: `kb/regions/TR.yaml` → ceftaroline, cefpodoxime, cefotaxime, aztreonam `unknown`;
klaritromisin IV "var sanılıyor", doksisiklin IV "doğrulanmadı".

Bulunan (TİTCK listesi 18.09.2026, ATC koduna göre):

| İlaç | ATC | Ürün | Durum |
|---|---|---|---|
| Seftarolin | J01DI02 | 0 | Listede yok (İlacabak'ta Omvelin 600 mg görünüyor ama güncel listede yok) |
| Sefpodoksim | J01DD13 | 74, 68'i askıda değil | Ruhsatlı |
| Sefotaksim | J01DD01 | 34, 29'u askıda değil | Ruhsatlı (Claforan askıda) |
| Aztreonam | J01DF01 | 0 | Tek başına yok; yalnız aztreonam-avibaktam (Emblaveo, J01DF51, 13.06.2026) |
| Klaritromisin IV | J01FA09 | Birden çok (Klacid 500 mg IV, Oradro, Uniklar, Clarol, Inclar, Deklarit…) | IV var |
| Doksisiklin | J01AA02 | 5, hepsi oral kapsül/tablet | IV yok |
| Azitromisin IV | J01FA10 | Zitrasin, Azitro, Maxitro | IV var |

Diğer 18 ilaç (oseltamivir dahil) ruhsatlı.

Güven: yüksek (liste indirildi ve ayrıştırıldı); seftarolin için orta.

Önerilen değişiklik: `kb/regions/TR.yaml` →
- `cefpodoxime.status: licensed`, `cefotaxime.status: licensed`.
- `ceftaroline.status: not_licensed` (ya da `unknown` + not "TİTCK 18.09.2026 listesinde yok").
- `aztreonam.status: not_licensed`, not "yalnız aztreonam-avibaktam ruhsatlı".
- `clarithromycin` notu → "IV ruhsatlı, TİTCK 18.09.2026".
- `doxycycline` notu → "IV ruhsatlı ürün yok"; IV doksisiklin içeren rejimler (`cro_dox`
  IV kolu) TR'de oral doksisiklinle yazılmalı ya da işaretlenmeli.
- Şema `not_licensed` değerini tanımıyorsa eklenmeli.

## Diğer Noktalar

### 6. IDSA Soru ve Tablo Eşlemesi

Mevcut: sorular S7-S14; doz atfı "Tablo 1, 3, 4".

Bulunan (genel bilgi, PMC6812437 bu turda satır satır açılmadı): S7 ayaktan tedavi, S8
yatan tedavi, S9 anaerop/aspirasyon, S10 MRSA/Pseudomonas, S11 kortikosteroid, S12 influenza
antiviral, S13 influenza pozitifte antibiyotik, S14 süre. Tablo 1 ağır TKP ölçütleri,
Tablo 2 ayaktan başlangıç tedavisi, Tablo 3 yatan hastada başlangıç tedavisi. Tablo 4
bulunamadı.

Güven: orta.

Önerilen değişiklik: `kb/docs` ve `drugs/*.yaml` doz atıfları "Tablo 2 (ayaktan), Tablo 3
(yatan)" olmalı; "Tablo 4" atfı PMC metniyle doğrulanana kadar kaldırılmalı.

### 7. YBÜ Yatışı = Ağır

Mevcut: `kb/syndromes/cap.yaml` → YBÜ'ye yatış doğrudan `severe`.

Bulunan: IDSA ağırlığı YBÜ yerinden değil 2007 IDSA/ATS ölçütlerinden tanımlar (1 majör:
vazopressör gerektiren şok ya da mekanik ventilasyon; ya da ≥3 minör). TTD 2021 Tablo 6 aynı
ölçütü YBÜ yatış ölçütü olarak kullanır.

Güven: orta.

Önerilen değişiklik: `kb/syndromes/cap.yaml` → `severe` türetimi "1 majör ya da ≥3 minör"
bayrağından; YBÜ yeri yardımcı gösterge olarak kalabilir ama tek başına ağır saymamalı.

### 8. Ayaktan Sağlıklı Hastada Basamaklar

Mevcut: amoksisilin güçlü, doksisiklin ve makrolid koşullu, ayrı basamak.

Bulunan: IDSA S7 bununla uyumlu (amoksisilin güçlü; doksisiklin koşullu; makrolid yalnız
direnç <%25 ise koşullu). TTD 2021 Grup 1a: amoksisilin; atipik kuşkuda + makrolid ya da
doksisiklin; makrolid monoterapisi Türkiye'de önerilmez.

Güven: yüksek (TTD), orta (IDSA).

Önerilen değişiklik: yok. TR katmanında makrolid monoterapisi zaten `ge25` ile dışlanıyor.

### 9. Ağır TKP'de BL+Makrolid ve BL+FQ Aynı Basamak

Mevcut: aynı basamak.

Bulunan: IDSA S8 ağırda ikisini de güçlü öneri olarak verir (BL+makrolid kanıtı orta,
BL+FQ düşük). TTD 2021 YBÜ'de 3KSef/BL+BLİ + makrolid ya da + solunum kinolonu, sıra
belirtmeden.

Güven: orta.

Önerilen değişiklik: yok; kanıt düzeyi farkı `rationale` notuna yazılabilir.

### 10. Apse/Ampiyemde Ajan Seçimi

Mevcut: yatan ampisilin-sulbaktam, ayaktan amoksisilin-klavulanat, `abxpilot-draft`.

Bulunan: IDSA S9 anaerop kapsamını yalnız apse ya da ampiyemde önerir, ajan belirtmez.
TTD 2021 aspirasyon/anaerop kuşkusunda BL+BLİ grubunu sayar (Tablo 7 etken listesi); ajan
seçimi taslakla uyumlu ama birincil kaynakta açık öneri bulunamadı.

Güven: düşük-orta.

Önerilen değişiklik: source `abxpilot-draft` kalır; not "IDSA S9 ajan belirtmez".

### 11. MRSA/Pseudomonas Düzenleyicileri Yalnız Yatanda

Mevcut: servis ve YBÜ'de uygulanır, ayaktanda not satırı.

Bulunan: IDSA S10 düzenleyicileri yatan hasta için tanımlar. TTD 2021 dirençli etken risk
faktörlerini (son 3 ayda yatış ya da antibiyotik; son 6 ayda — Tablo 7 dipnotunda son bir
yıl — solunum örneğinde dirençli bakteri) yatan hasta algoritmasında kullanır. İkisi de
ayaktan için ayrı rejim vermez.

Güven: orta.

Önerilen değişiklik: yok. TR katmanında dirençli bakteri penceresi için not: TTD metin 6
ay, tablo dipnotu 1 yıl.

### 12. `recent_abx_class` ve Başka Sınıf Tercihi

Mevcut: soru son antibiyotik sınıfını sorar, motor başka sınıfı öne alır.

Bulunan: TTD 2021 son 3 ayda antibiyotik kullanımı varsa farklı sınıf seçilmesini açıkça
söyler (beta-laktam alana kinolon, kinolon alana beta-laktam). TTD 2009 Tablo 5 de
değiştirici faktörde aynı.

Güven: yüksek.

Önerilen değişiklik: `kb/questions/recent_abx_class.yaml` → pencere "son 3 ay", source
`ttd-2021`, section "Tablo 5".

### 13. TR MRSA Oranı

Mevcut: `sau_methicillin` category `unknown`.

Bulunan (CAESAR, invaziv S. aureus, Türkiye): 2016 %22,7; 2017 %25,8; 2018 %29,6;
2019 %31,3; 2020 %33,4; 2021 %30,7 (n=3562). TKP'ye özgü veri bulunamadı.

Güven: yüksek (rakam), düşük (TKP'ye uygulanabilirlik).

Önerilen değişiklik: `kb/regions/TR.yaml` → `sau_methicillin.estimate: 0.307`,
category `ge25`, source `who-caesar`, source_date `2021-12-31`, not "invaziv izolat, TKP'de
MRSA sıklığı çok daha düşük; ampirik MRSA kapsamı risk faktörüne bağlı kalmalı".

### 14. TR Eşikleri

Mevcut: makrolid 0,25; yatış 90 gün; süre 5 ve 7 gün; source `idsa-ats-2019`.

Bulunan: IDSA ile aynı. TTD 2021: son 3 ay yatış/antibiyotik; en az 5 gün, dirençli
etkende 7 gün; ateş düşüp klinik stabilite sağlanınca kesme.

Güven: yüksek.

Önerilen değişiklik: `kb/regions/TR.yaml` eşiklerinde source `ttd-2021` (Tedavi Süresi
bölümü ve Tablo 7) eklenebilir; değerler aynı kalır.

### 15. Aztreonam AWaRe

Mevcut: `reserve`.

Bulunan: Reserve. AWaRe 2023 xlsx iris.who.int'ten indirilemedi (sayfa JS ile yükleniyor);
Reserve sınıfı hakemli AWaRe kullanım çalışmalarıyla desteklendi
(https://pmc.ncbi.nlm.nih.gov/articles/PMC12909863/).

Güven: orta.

Önerilen değişiklik: yok.

### 16. Ampisilin-Sulbaktam AWaRe

Mevcut: `access`.

Bulunan: Birincil liste açılamadı. AWaRe'de sınıflandırılmış ama EML'de değil
(https://list.essentialmeds.org/medicines/598). Access grubunda olduğu genel bilgiyle
uyumlu, bu turda doğrulanmadı.

Güven: düşük.

Önerilen değişiklik: yok; A9'da AWaRe 2023 xlsx'inden doğrulanmalı.

### 17. R1 Yan Zinciri Grupları

Mevcut: cefuroxime `r1_group: cefuroxime`, `r1_similar: [methoxyimino_aminothiazolyl]`;
ceftaroline kendi grubu; aztreonam = ceftazidime.

Bulunan (genel bilgi, Zagursky 2018 bu turda açılmadı): aztreonam-seftazidim R1 özdeşliği
bilinen tek güçlü monobaktam çapraz reaksiyonudur. Sefuroksimin metoksimino yan zinciri
seftriakson/sefotaksim/sefpodoksimle benzer kabul edilir; aminotiyazolil halkası
sefuroksimde yok (furil). Seftarolin R1'i tiadiazolil-oksimino, ayrı grup makul.

Güven: orta (aztreonam), düşük (diğerleri).

Önerilen değişiklik: `kb/drugs/cefuroxime.yaml` → `r1_similar` değeri `methoxyimino`
olarak adlandırılmalı ("aminothiazolyl" sefuroksimde yok). Zagursky tablosuyla doğrulanmalı.

### 18. CredibleMeds Erişim Tarihi

Mevcut: yaklaşık tarih.

Bulunan: bulunamadı. CredibleMeds listesi kayıt gerektirir; bu turda erişilmedi.

Güven: —

Önerilen değişiklik: A9'da listeye girilip `source_date` o günün tarihiyle yazılmalı.

### 19. Oseltamivir QT Sınıfı

Mevcut: `none`.

Bulunan: bulunamadı (CredibleMeds erişilmedi). Genel bilgi: oseltamivir QT listelerinde
bilinen risk sınıfında değil; bu doğrulama değildir.

Güven: düşük.

Önerilen değişiklik: yok; A9'da doğrulanmalı.

### 20. Gebelik Kategorileri

Mevcut: örn. clarithromycin `avoid`, amoxicillin `compatible`; ürün bilgisi sürümü yok.

Bulunan: FDA harf kategorileri 2015'te (PLLR) kaldırıldı; `category` alanı harf değil
serbest sınıf olmalı. TTD 2021 "Gebelikte TGP" bölümü tetrasiklin ve kinolondan
kaçınılmasını, makrolidlerde malformasyon verisinin tartışmalı olduğunu belirtir;
beta-laktamlar güvenli kabul edilir.

Güven: orta.

Önerilen değişiklik: `kb/drugs/doxycycline.yaml`, `levofloxacin.yaml`,
`moxifloxacin.yaml` → `pregnancy.category: avoid`, source `ttd-2021` "Gebelikte TGP".
Her ürün bilgisi kaydına KÜB tarihi eklenmeli (bulunamadı, A9).

### 21. Oral Biyoyararlanım

Mevcut: azitromisin low, klaritromisin moderate, sefuroksim moderate, sefpodoksim moderate;
diğerleri high.

Bulunan (ürün bilgisi değerleri, genel bilgi; KÜB'ler bu turda açılmadı): azitromisin
~%37, klaritromisin ~%50, sefuroksim aksetil ~%37-52, sefpodoksim ~%50, amoksisilin
~%70-90, levofloksasin ~%99, moksifloksasin ~%90, doksisiklin ~%95, linezolid ~%100.

Güven: orta.

Önerilen değişiklik: yok; kategoriler rakamlarla uyumlu. Eşik tanımı (örn. high ≥%80,
moderate %40-79, low <%40) `kb/schema` belgesine yazılmalı; azitromisin sınırda.

### 22. IV Klaritromisin Dozu

Mevcut: `cap_iv` 500 mg q12h, `product-labels`.

Bulunan: TTD 2021 Ek 1 klaritromisin parenteral 500 mg q12h ve oral 500 mg q12h. TİTCK'te
Klacid 500 mg IV ruhsatlı.

Güven: yüksek.

Önerilen değişiklik: `kb/drugs/clarithromycin.yaml` → `cap_iv` source `ttd-2021`, section
"Ek 1".

### 23. Oral Linezolid Dozu

Mevcut: 600 mg q12h, `product-labels`.

Bulunan: IDSA Tablo 3'te MRSA kapsamı için linezolid 600 mg q12h geçtiği biliniyor; yol
belirtilmeden. Bu turda PMC metninde satır doğrulanmadı.

Güven: orta.

Önerilen değişiklik: source `idsa-ats-2019` Tablo 3 (doğrulanınca); oral=IV aynı doz
(biyoyararlanım ~%100).

### 24. Vankomisin Yükleme ve Düzey

Mevcut: 15 mg/kg (70 kg = 1050 mg), yükleme ve hedef yok.

Bulunan: IDSA TKP 15 mg/kg q12h, düzeye göre ayarlama. ASHP/IDSA/PIDS/SIDP 2020 vankomisin
konsensüsü: ağır hastada 20-35 mg/kg yükleme, AUC/MIC 400-600 hedefi (çukur düzey değil).
Konsensüs PDF'i bu turda açılmadı.

Güven: orta.

Önerilen değişiklik: `kb/drugs/vancomycin.yaml` → `loading: 20-35 mg/kg (70 kg: 1400-2450
mg, ağırda)`, `monitoring: AUC 400-600`, source yeni kayıt `ashp-vanco-2020` (Rybak ve ark.,
AJHP 2020;77:835-864).

### 25. Patojen Kayıtları

Mevcut: tümü `abxpilot-draft`.

Bulunan: bulunamadı (tek bir birincil kaynak yok). Direnç mekanizmaları için EUCAST beklenen
fenotip tablosu kısmen kullanılabilir (bkz. Kapsam Matrisi).

Güven: —

Önerilen değişiklik: intrinsik direnç alanlarına source `eucast-expected-2023`.

### 26. Karşılaştırma Puanı Ağırlıkları

Mevcut: 0,30 / 0,20 / 0,15 / 0,10 / 0,10 / 0,15 / 0, `abxpilot-draft`.

Bulunan: bulunamadı. Bu ağırlıkların kılavuz karşılığı yoktur; tasarım kararıdır.

Güven: —

Önerilen değişiklik: yok; `abxpilot-draft` olarak kalmalı, arayüzde "proje tercihi" diye
gösterilmeli.

## Kapsam Matrisi

Dayanak: EUCAST Expected Resistant Phenotypes v1.2 (2023) ve breakpoint tablosunda
"breakpoint yok / uygun hedef değil" işaretleri. Hücre adları `ilaç × patojen`.

### Açıkça Yanlış, Değişmeli

| Hücre | Mevcut | Önerilen | Gerekçe |
|---|---|---|---|
| ceftazidime × s_pneumoniae | variable | none | EUCAST'ta seftazidim için pnömokok breakpoint'i yok; zayıf aktivite |
| vancomycin × anaerobes | variable | none (ya da not "yalnız Gram-pozitif anaerop") | Gram-negatif anaeroblar glikopeptidlere beklenen dirençli |
| linezolid × anaerobes | variable | none (aynı not) | Gram-negatiflerde oksazolidinon beklenen direnç |

### Tartışmalı, A9'a

| Hücre | Mevcut | Öneri | Gerekçe |
|---|---|---|---|
| azithromycin × h_influenzae | reliable | variable | EUCAST makrolidlerin H. influenzae'de klinik etkinliğini tartışmalı sayar; klaritromisin zaten variable |
| doxycycline × mrsa | variable | variable (not) | Tetrasiklin duyarlılığı yerel; TKP'de MRSA için kılavuz ajanı değil |

### Doğru `none` Olanlar (Değişmesin)

- Tüm beta-laktamlar × m_pneumoniae, c_pneumoniae, legionella (hücre duvarı yok / hücre içi).
- vancomycin, linezolid × h_influenzae, m_catarrhalis, p_aeruginosa, enterobacterales,
  legionella (Gram-negatif beklenen direnç).
- aztreonam × s_pneumoniae, mssa, mrsa (Gram-pozitif beklenen direnç).
- azithromycin, clarithromycin × enterobacterales, p_aeruginosa.
- ceftriaxone, cefotaxime, amoxicillin, amoxicillin-klavulanat, ceftaroline,
  moxifloxacin, doxycycline × p_aeruginosa (EUCAST: P. aeruginosa beklenen dirençli ya da
  breakpoint yok).
- Tüm antibakteriyeller × influenza; oseltamivir × tüm bakteriler.
- Seftarolin dışındaki beta-laktamlar × mrsa.
- aztreonam × anaerobes; ampicillin_sulbactam × p_aeruginosa (P. aeruginosa'da amp/sam
  beklenen dirençli).

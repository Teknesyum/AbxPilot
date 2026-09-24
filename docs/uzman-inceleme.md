# Uzman İncelemesi — Enfeksiyon Hastalıkları

Bu belge AbxPilot bilgi tabanının (`kb/`) A9 uzman inceleme paketidir. Kaynaklar:
`docs/kb-kaynaklar.md`, `docs/kb-degisiklik-2026-09.md`, `docs/kb-arastirma-2026-09.md`,
`docs/altin-vaka-raporu.md` (kb 0.2.0, 2026-09-25), `kb/guidelines/*/*.csv`, `kb/constraints.yaml`,
`tests/AbxPilot.Tests/Golden/`.

## 1. Amaç

AbxPilot bir **kılavuz gezgini ve eğitim aracıdır**, klinik karar verme aracı değildir.
Temel hasta: 70 kg sağlıklı erişkin, normal böbrek işlevi. `kb/` altındaki her kayıt
`review_status: unreviewed`, `reviewed_at: null` durumundadır; hiçbiri klinik kullanım
için onaylı değildir.

Uzmandan istenen: aşağıdaki her madde için **Onay**, **Düzelt** ya da **Not** işaretlemesi.
Onay dışı her satır kb dosyalarına geri yazılmadan önce yeniden değerlendirilecektir.

## 2. Sendrom Başına İlk Seçim Özeti

Kaynak: `docs/altin-vaka-raporu.md` (altın vaka çalıştırması) ve ilgili
`kb/guidelines/*/*.csv` bölüm alanları. En çok 15 satır/sendrom; temsil amaçlı seçildi,
tam liste `docs/altin-vaka-raporu.md` içindedir.

### TKP (Toplumda Gelişen Pnömoni)

| Senaryo | Set | Birinci Seçenek | Kaynak Bölüm |
|---|---|---|---|
| out_healthy_low_resistance / out_healthy_eu / out_healthy_us | idsa-ats-2019 | amx_po | Q7; Table 2 |
| out_healthy_other / out_healthy_tr (makrolid ≥%25) | idsa-ats-2019 | amx_po | Q7; Table 2 |
| out_comorbid | idsa-ats-2019 | amc_azm_po | Q7; Table 2 |
| out_comorbid | ttd-2021 | cxm_azm_po | Table 5, Group 1b |
| out_ige (penisilin IgE alerjisi) | idsa-ats-2019 | dox_po | Q7; Table 2 |
| pregnancy_outpatient | idsa-ats-2019 | amx_po | Q7; Table 2 |
| ward | idsa-ats-2019 | sam_azm_iv | Q8; Table 3 |
| ward | ttd-2021 | sam_azm_iv | Table 7 |
| icu_severe | idsa-ats-2019 | sam_azm_iv | Q8; Table 3 |
| ward_ige_pseudomonas | idsa-ats-2019 | fep_azm_iv | Q10; Q14 (psa_prior) |
| ward_ige_pseudomonas | ttd-2021 | fep_cip_iv | Table 7 (psa_fq) |
| tr_no_ceftaroline | idsa-ats-2019 | cro_azm_iv | Q8; Table 3 |
| scar_ward | idsa-ats-2019 | lvx_iv | Q8; Table 3 |
| abscess_ward | idsa-ats-2019 | sam_azm_iv | Q9 (abscess_inpatient) |
| ward_qt | idsa-ats-2019 | cro_dox | Q8; Table 3 |

### Selülit / SSTI

| Senaryo | Set | Birinci Seçenek | Kaynak Bölüm |
|---|---|---|---|
| mild_cellulitis | idsa-2014 | ssti_pnv_po | Figure 1; recommendations 14-15; Table 2 |
| mild_cellulitis | nice-ng141-2019 | ssti_flx_po | Recommendations 1.1.5, 1.2.1; Table 1 |
| moderate_cellulitis | idsa-2014 | ssti_pen_iv | Figure 1; recommendations 14-15; Table 2 |
| moderate_cellulitis | nice-ng141-2019 | ssti_flx_iv | Recommendations 1.1.5-1.1.6, 1.2.1; Table 1 |
| mild_mrsa | idsa-2014 | ssti_cli_po | Recommendations 14-15 (evidence summary) |
| mild_mrsa | nice-ng141-2019 | ssti_flx_po + vankomisin | Table 1 (MRSA eki) |
| moderate_mrsa | idsa-2014 | ssti_van_iv | Recommendations 14-15; Table 2 |
| moderate_mrsa | nice-ng141-2019 | ssti_flx_iv + vankomisin | Table 1 (MRSA eki) |
| abscess_moderate | idsa-2014 | ssti_sxt_po | Figure 1; recommendation 6; Table 2 |
| facial_mild | idsa-2014 | ssti_pnv_po | Figure 1; recommendations 14-15; Table 2 |
| facial_mild | nice-ng141-2019 | ssti_amc_po | Table 1 (yüz bölgesi); 1.1.14 |
| facial_moderate_ige | nice-ng141-2019 | ssti_clr_mtz_iv | Table 1 (yüz bölgesi); 1.1.14 |
| necrotizing | idsa-2014 | ssti_van_tzp_iv (sevk) | Recommendations 27-28; Table 4 |
| severe_hypotension | idsa-2014 | ssti_van_tzp_iv (sevk) | Figure 1; recommendation 14 |
| abscess_mild | idsa-2014 / nice-ng141-2019 | antibiyotik yok | Figure 1; recommendation 5 |

### İYE (İdrar Yolu Enfeksiyonu)

| Senaryo | Set | Birinci Seçenek | Kaynak Bölüm |
|---|---|---|---|
| cystitis_low_tmp | idsa-2011-2025 | uti_nit_po | Recommendations 1-7; Table 4 |
| cystitis_low_tmp | eau-2026 | uti_fos_po | Section 3.4.4.b; Table 3 |
| cystitis_male | eau-2026 | uti_piv_male_po | Section 3.4.4.d; Table 3 |
| cystitis_male | idsa-2011-2025 | sevk (uUTI kılavuzu bekliyor) | Box 2 |
| cystitis_pregnant | eau-2026 | uti_fos_po | Section 3.4.4.c.1 |
| cystitis_pregnant | idsa-2011-2025 | sevk | Introduction (scope) |
| pyelo_in | idsa-2011-2025 | uti_cro_iv | Recommendation A-II; Tables 1.1, 2.1 |
| pyelo_in | eau-2026 | uti_ctx_eau_iv | Section 3.6.3; Table 6; section 3.7.3 |
| pyelo_in_esbl | eau-2026 | uti_mem_iv | Section 3.6.3; Table 6; section 3.7.3 |
| pyelo_in_scar | eau-2026 | uti_gen_iv | Section 3.6.3; Table 6; section 3.7.3 |
| pyelo_male | idsa-2011-2025 | uti_cip7_po | Recommendations 9-12; IDSA 2025 duration I |
| pyelo_out_recent_fq | idsa-2011-2025 | uti_cro1_amc_po | Recommendations 9-12; IDSA 2025 duration I |
| pyelo_out_recent_fq | eau-2026 | uti_cro1_cpd_eau_po | Section 3.6.3; Table 5 |
| pyelo_pregnant | eau-2026 | uti_cro_eau_iv | Section 3.6.3 (text below Table 6) |
| urosepsis | idsa-2011-2025 | uti_cro_iv (sevk) | Recommendation A-I; Table 1.1 |

### İAE (İntraabdominal Enfeksiyon)

| Senaryo | Set | Birinci Seçenek | Kaynak Bölüm |
|---|---|---|---|
| community_apache12 | ekmud-2016 | iai_etp_iv | Item 20; Tables 4, 8 |
| community_apache12 | sis-2017 | iai_tzp_iv | Section 6B; Table 9 |
| community_apache12 | sis-idsa-2010 | iai_etp_iv | Recommendations 30-37; Table 2 |
| community_mild_tr | ekmud-2016 | iai_etp_iv | Items 20, 28; Table 8 |
| community_mild_tr | sis-2017 | iai_cro_mtz_iv | Section 6A; Table 9 |
| community_mild_scar | ekmud-2016 | iai_tgc_iv | Items 20, 28; Table 8 (allergy_scar_beta_lactam ile daraltılmış) |
| community_mild_scar | sis-2017 | iai_cip_mtz_iv | Section 6A; Table 9 (allergy_scar_beta_lactam ile daraltılmış) |
| community_mild_scar | sis-idsa-2010 | iai_tgc_iv | Recommendations 30-37; Table 2 |
| healthcare | ekmud-2016 | iai_tzp_iv | Items 32-33 |
| healthcare | sis-idsa-2010 | iai_mem_iv | Recommendation 45; Table 3 |
| healthcare_esbl | ekmud-2016 / sis-2017 / sis-idsa-2010 | iai_mem_iv | Items 32-33 / Section 7D; Table 10 / Table 3; recommendation 45 |
| healthcare_mrsa | sis-idsa-2010 | iai_mem_iv + vankomisin | Recommendation 45; Table 3 + Recommendations 58-59 |
| healthcare_mrsa_scar | tüm üç set | aday yok | (bkz. Bölüm 3) |
| no_source_control | ekmud-2016 | iai_etp_iv | Items 20, 28; Table 8 |
| sepsis | tüm üç set | sevk | Items 7, 26 / Sections 1, 2, 6B / Recommendations 11-12, 38 |

## 3. Klinik Olarak Tartışmalı Sonuçlar

| # | Sonuç | Motor Davranışı | Kaynak |
|---|---|---|---|
| A | Gebe, ayaktan TKP'de yalnız amoksisilin seçilir; doksisiklin ve makrolid `pregnancy_avoid` ile elenir. | `cap/pregnancy_outpatient`: `amx_po` seçili, `dox_po`, `clr_po` `pregnancy_avoid` ile, `azm_po` `macrolide_monotherapy_resistance` ile elenmiş. | `docs/altin-vaka-raporu.md`; `kb/constraints.yaml` (`pregnancy_avoid`, `product-labels`) |
| B | Gebe + servis + bilinen QT riski: motor aday üretmiyor, "uzmana danışın" sonucu veriyor. | `cap/ward_qt_pregnancy`: her iki sette de `no_candidate`, tüm beta-laktam+makrolid/kinolon kolları `qt_known_risk` ya da `pregnancy_avoid` ile elenmiş. | `docs/altin-vaka-raporu.md` |
| C | İAE'de ağır beta-laktam alerjisinde (SCAR) tigesiklin EKMUD ve SIS/IDSA 2010'da ilk seçim; SIS 2017'de siprofloksasin+metronidazol seçiliyor — setler arası uyuşmuyor. | `iai/community_mild_scar`: ekmud-2016 → `iai_tgc_iv`; sis-idsa-2010 → `iai_tgc_iv`; sis-2017 → `iai_cip_mtz_iv`. | `docs/altin-vaka-raporu.md`; `kb/guidelines/{ekmud-2016,sis-2017,sis-idsa-2010}/iai.csv` |
| D | Ağır beta-laktam alerjili, sağlık bakımı kökenli MRSA riskli İAE'de üç setin hiçbirinde aday kalmıyor. | `iai/healthcare_mrsa_scar`: üç sette de `no_candidate`; tüm beta-laktamlar `allergy_scar_beta_lactam` ile elenmiş, alternatif ajan yok. | `docs/altin-vaka-raporu.md` |
| E | IDSA İYE setinde GSBL riski yalnız not düşer, ilk seçimi değiştirmez; EAU setinde GSBL riski beta-laktam/kinolon/aminoglikozid/folat antagonistini eler ve karbapeneme geçirir. | `kb/guidelines/idsa-2011-2025/uti.csv` satırı `esbl` → `stage: modifier, action: note`; `kb/guidelines/eau-2026/uti.csv` satırı `esbl` → `stage: modifier, action: replace`, adaylar `uti_mem_iv|uti_ipm_iv`. | `kb/guidelines/idsa-2011-2025/uti.csv`; `kb/guidelines/eau-2026/uti.csv` |
| F | NICE eki, hafif **oral** selülitte bile MRSA riskinde IV vankomisin ekliyor (oral MRSA seçeneği kılavuzda yok). | `ssti/mild_mrsa` (nice-ng141-2019): birinci seçenek `ssti_flx_po+vancomycin`. | `docs/altin-vaka-raporu.md`; `docs/kb-arastirma-2026-09.md` madde 38 |
| G | ABD bölgesinde pnömokok makrolid direnci %23,9 — motorun %25 eşiğinin hemen altında kalıyor (invaziv izolat, TKP'ye özgü değil). | `kb/regions/US.yaml` → `spn_macrolide.estimate: 0.239`, `category: lt25`. | `kb/regions/US.yaml`; `docs/kb-kaynaklar.md` madde 42 |
| H | Aminoglikozit bilgi tabanında yok; TTD'nin "beta-laktam + aminoglikozit + makrolid" alternatif Pseudomonas rejimi ve EKMUD/SIS'in yüksek dirençte aminoglikozit eklemesi modellenmedi. | `kb/guidelines/ttd-2021/cap.csv` satırı `psa_fq` notu; `kb/guidelines/ekmud-2016/iai.csv` satırı `healthcare` notu. | `docs/kb-degisiklik-2026-09.md` ("Uygulanmayanlar"); ilgili csv notları |
| I | Spektrum matrisinde azitromisin × H. influenzae `reliable` işaretli ama EUCAST klinik etkinliği tartışmalı sayıyor (klaritromisin zaten `variable`). | `kb/spectrum/cap.csv` hücresi `azithromycin × h_influenzae`. | `docs/kb-arastirma-2026-09.md`, Kapsam Matrisi, "Tartışmalı, A9'a" |
| J | Spektrum matrisinde doksisiklin × MRSA `variable` işaretli; duyarlılık yerel değişken ve TKP'de MRSA için kılavuz ajanı değil. | `kb/spectrum/cap.csv` hücresi `doxycycline × mrsa`. | `docs/kb-arastirma-2026-09.md`, Kapsam Matrisi, "Tartışmalı, A9'a" |

## 4. Kaynakla Doğrulanamayan Maddeler

Numaralar `docs/kb-kaynaklar.md` → "Belirsiz Noktalar" bölümündeki madde numaralarıdır.
Kısmen çözülen ve açık kalan maddeler listelenmiştir (çözülmüş maddeler kaynağıyla
işlenmiş olsa da A9 onayı bekler, bkz. Bölüm 5).

**Kısmen çözülen:** 4 (spektrum matrisi — bkz. Bölüm 3, I ve J), 6 (IDSA soru/tablo
eşlemesi), 14 (TR eşikleri, kaynak eklenmeli).

**Açık kalan:** 8 (ayaktan basamak sırası), 9 (ağır TKP BL+makrolid/BL+FQ aynı basamak),
10 (apse/ampiyemde ajan seçimi), 11 (MRSA/Pseudomonas düzenleyicisi yalnız yatanda),
15 (aztreonam AWaRe), 16 (ampisilin-sulbaktam AWaRe), 17 (R1 yan zinciri grupları),
18 (CredibleMeds erişim tarihi), 19 (oseltamivir QT sınıfı), 20 (gebelik kategorileri),
21 (oral biyoyararlanım eşikleri), 23 (oral linezolid dozu kaynağı), 25 (patojen
kayıtları taslak), 26 (karşılaştırma puanı ağırlıkları), 27 (IDSA 2011 sistit dozları
ikincil okumadan), 28 (EAU levofloksasin IV — TR ruhsatı), 29 (pürülan DYDE 5 gün süresi
taslak), 30 (SIS 2024 güncellemesi okunmadı), 31 (TR TMP-SMX direnci %26,2 vs %26,9),
32 (pivmesilinam AWaRe), 33 (teikoplanin/linezolid İAE dozları ürün bilgisinden),
34 (İAE GSBL/FQ eşikleri üriner veriden aktarıldı), 35 (EKMUD madde 20 okuması),
36 (IDSA 2025 cUTI DOI/sayfa), 37 (IDSA setinde GSBL yalnız not — bkz. Bölüm 3, E),
38 (NICE MRSA eki oral selülite de IV vankomisin — bkz. Bölüm 3, F), 39 (AB oranları
invaziv izolat, sendroma özgü değil), 40 (EARS-Net E. coli TMP-SMX raporlamıyor),
41 (AB ruhsatı doğrulanmadı), 42 (ABD pnömokok makrolid %23,9 — bkz. Bölüm 3, G),
43 (ABD toplum kökenli MRSA oranı bulunamadı), 44 (ABD E. coli oranları Kaye 2021,
CDC değil), 45 (ABD 2017 sonrası karbapenem direnci bulunamadı), 46 (sefotaksim ABD'de
yalnız Claforan), 47 (Diğer bölge profili direnç verisi taşımıyor).

## 5. Onay Tablosu

| # | Madde | Mevcut | Kaynak | Onay/Düzelt | Not |
|---|---|---|---|---|---|
| 4 | Spektrum matrisi: açıkça yanlış 3 hücre `none` yapıldı; 2 hücre tartışmalı kaldı | `kb/spectrum/cap.csv` | `eucast-expected-2023` |  |  |
| 6 | IDSA soru/tablo eşlemesi (S7-S14, Tablo 1-4) satır satır doğrulanmadı | `kb/guidelines/idsa-ats-2019/cap.csv` bölüm alanları | `idsa-ats-2019` (PMC6812437) |  |  |
| 8 | Ayaktan sağlıklı hastada amoksisilin güçlü, doksisiklin/makrolid koşullu, ayrı basamak | `kb/guidelines/idsa-ats-2019/cap.csv` (`out_healthy_low/high`) | `idsa-ats-2019`, `ttd-2021` |  |  |
| 9 | Ağır TKP'de BL+makrolid ile BL+FQ aynı basamakta | `kb/guidelines/idsa-ats-2019/cap.csv` (`severe`) | `idsa-ats-2019` Q8 |  |  |
| 10 | Apse/ampiyemde ajan seçimi (sam_iv / amc_po) taslak | `kb/guidelines/idsa-ats-2019/cap.csv` (`abscess_inpatient`, `abscess_outpatient`) | `abxpilot-draft` |  |  |
| 11 | MRSA/Pseudomonas düzenleyicileri yalnız yatan hastada uygulanıyor | `kb/guidelines/idsa-ats-2019/cap.csv` (`mrsa_prior`, `psa_prior`) | `idsa-ats-2019`, `ttd-2021` |  |  |
| 14 | TR eşikleri (makrolid 0,25; yatış 90 gün; süre 5/7 gün) | `kb/regions/TR.yaml` | `idsa-ats-2019` (kaynak `ttd-2021` eklenmeli) |  |  |
| 15 | Aztreonam AWaRe grubu `reserve` | `kb/drugs/aztreonam.yaml` | `who-aware-2023` |  |  |
| 16 | Ampisilin-sulbaktam AWaRe grubu `access` | `kb/drugs/ampicillin_sulbactam.yaml` | `who-aware-2023` (doğrulanmadı) |  |  |
| 17 | Sefuroksim `r1_similar: methoxyimino_aminothiazolyl`; öneri `methoxyimino` | `kb/drugs/cefuroxime.yaml` | `zagursky-2018` |  |  |
| 18 | CredibleMeds erişim tarihi yaklaşık | `kb/drugs/*.yaml` (`qt_risk` kaynak tarihi) | `credible-meds` |  |  |
| 19 | Oseltamivir QT sınıfı `none` | `kb/drugs/oseltamivir.yaml` | kaynak bulunamadı |  |  |
| 20 | Gebelik kategorileri (`avoid`/`compatible`), ürün bilgisi sürümü kayıtsız | `kb/drugs/*.yaml` (`pregnancy`) | `product-labels`; doksisiklin/levofloksasin/moksifloksasin için `ttd-2021` önerisi | |  |
| 21 | Oral biyoyararlanım kategorileri (azitromisin sınırda) | `kb/drugs/*.yaml` (`oral_bioavailability`) | ürün bilgisi, genel bilgi |  |  |
| 23 | Oral linezolid dozu 600 mg q12h | `kb/drugs/linezolid.yaml` | `product-labels` (öneri: `idsa-ats-2019` Tablo 3, doğrulanınca) |  |  |
| 24 | Vankomisin yükleme 20-35 mg/kg (70 kg: 1400-2450 mg), AUC 400-600 | `kb/drugs/vancomycin.yaml` | `ashp-vanco-2020` (Rybak ve ark.) |  |  |
| 25 | Patojen kayıtları (12 patojen) tümü taslak | `kb/pathogens/*.yaml` | `abxpilot-draft` |  |  |
| 26 | Karşılaştırma puanı ağırlıkları (0,30/0,20/0,15/0,10/0,10/0,15) taslak | `kb/scoring.yaml` | `abxpilot-draft` |  |  |
| 27 | IDSA 2011 Tablo 4 sistit dozları (amc, cpd, cip, lvx) ikincil okumadan | `kb/guidelines/idsa-2011-2025/uti.csv` (`cystitis_female`) | `idsa-uti-2011` |  |  |
| 28 | EAU levofloksasin IV rejimi — TR IV ruhsatı ve etiket dozu doğrulanmadı | `kb/guidelines/eau-2026/uti.csv` (`in_lt10`) | `eau-uti-2026`; `titck-2026` |  |  |
| 29 | Pürülan DYDE'de 5 gün süresi IDSA'da açık yazmıyor, taslak | `kb/guidelines/idsa-2014/ssti.csv` (`pur_moderate`, `pur_severe`) | `abxpilot-draft` |  |  |
| 30 | SIS setleri 2017 ve 2010'a dayanıyor; SIS 2024 güncellemesi okunmadı | `kb/guidelines/sis-2017/iai.csv`, `sis-idsa-2010/iai.csv` | `sis-2017`, `sis-idsa-2010` (SIS 2024 okunmadı) |  |  |
| 31 | TR TMP-SMX direnci: 394/1503 hesabı %26,2, makale %26,9 | `kb/regions/TR.yaml` (`eco_tmp_smx`) | `cinislioglu-2024` |  |  |
| 32 | Pivmesilinam AWaRe grubu doğrulanmadı | `kb/drugs/pivmecillinam.yaml` | `who-aware-2023` |  |  |
| 33 | Teikoplanin ve linezolid İAE dozları ürün bilgisinden, kılavuzdan değil | `kb/drugs/teicoplanin.yaml`, `linezolid.yaml` (İAE dozları) | `product-labels` |  |  |
| 34 | İAE GSBL/FQ direnç eşikleri üriner veriden aktarıldı, İAE'ye özgü TR verisi yok | `kb/constraints.yaml` (`iai_fq_resistance`); `kb/regions/TR.yaml` | `cinislioglu-2024` (üriner) |  |  |
| 35 | EKMUD madde 20 okuması: hafif toplum kökenli İAE'de ertapenem önceliği | `kb/guidelines/ekmud-2016/iai.csv` (`community_mild`) | `ekmud-iai-2016` |  |  |
| 36 | IDSA 2025 cUTI DOI ve sayfa numaraları; web sürümü kullanıldı | `kb/sources.yaml` (`idsa-cuti-2025`) | `idsa-cuti-2025` |  |  |
| 37 | IDSA setinde GSBL riski yalnız not, ilk seçim değişmiyor; EAU karbapeneme geçiyor | `kb/guidelines/idsa-2011-2025/uti.csv`, `eau-2026/uti.csv` (`esbl`) | `idsa-cuti-2025`, `eau-uti-2026` |  |  |
| 38 | NICE MRSA eki hafif oral selülite de IV vankomisin ekliyor | `kb/guidelines/nice-ng141-2019/ssti.csv` (`mrsa_add`) | `nice-ng141-2019` |  |  |
| 39 | AB direnç oranları invaziv izolatlardan, sendroma özgü değil; ülke aralığı geniş | `kb/regions/EU.yaml` | `ecdc-ears-net-2024` |  |  |
| 40 | EARS-Net E. coli TMP-SMX direncini raporlamıyor; AB'de statik varsayılan (≥%20) | `kb/regions/EU.yaml` | `ecdc-ears-net-2024` |  |  |
| 41 | AB ruhsatı ülkeye göre değişir; tüm ilaçlar `unknown` | `kb/regions/EU.yaml` (`license`) | `ema-authorisation` |  |  |
| 42 | ABD pnömokok makrolid direnci %23,9, eşiğin hemen altında | `kb/regions/US.yaml` (`spn_macrolide`) | `cdc-abcs-spn-2024` |  |  |
| 43 | ABD toplum kökenli MRSA oranı bulunamadı; kayıt yalnız hastane verisi | `kb/regions/US.yaml` (`sau_methicillin`) | `cdc-nhsn-2018-2021` |  |  |
| 44 | ABD E. coli oranları Kaye 2021 (2011-2019), CDC ulusal veri değil | `kb/regions/US.yaml` (`eco_*`) | `kaye-2021` |  |  |
| 45 | ABD'de 2017 sonrası tür düzeyinde karbapenem direnci bulunamadı | `kb/regions/US.yaml` | kaynak bulunamadı |  |  |
| 46 | Sefotaksim ABD'de yalnız Claforan ile listeli, `unknown` bırakıldı | `kb/regions/US.yaml` (`license`) | `fda-drugsfda-2026` |  |  |
| 47 | "Diğer" bölge profili direnç verisi taşımıyor; sorular statik varsayılanla işleniyor | `kb/regions/OTHER.yaml` | `who-aware-2023` |  |  |

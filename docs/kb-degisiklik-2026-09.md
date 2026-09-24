# Bilgi Tabanı Değişikliği Eylül 2026

Kaynak: `docs/kb-arastirma-2026-09.md`. Karşılaştırma eski ve yeni `tmp/golden-report.md`
arasındadır. TTD seti `ttd-2009` yerine `ttd-2021`. Bölge TR.

## Birinci Seçimi Değişen Vakalar

| Vaka | Set | Eski Rejim | Yeni Rejim | Gerekçe |
|---|---|---|---|---|
| `ward_ige_pseudomonas` | IDSA | `sam_azm_iv` (sefepim + azitromisin) | `fep_azm_iv` (aynı ilaçlar) | Hata düzeltmesi: rejim adı verilen ilaçları göstermiyordu |
| `ward_ige_pseudomonas` | TTD | `sam_azm_iv` (sefepim + azitromisin + levofloksasin) | `fep_cip_iv` (sefepim + siprofloksasin) | TTD 2021 Tablo 7: anti-Pseudomonas beta-laktam + siprofloksasin |
| `ward_prior_pseudomonas` | IDSA | `sam_azm_iv` (pip-tazo + azitromisin) | `tzp_azm_iv` (aynı ilaçlar) | Rejim adı düzeltildi |
| `ward_prior_pseudomonas` | TTD | `sam_azm_iv` (pip-tazo + azitromisin + levofloksasin) | `tzp_cip_iv` (pip-tazo + siprofloksasin) | TTD 2021 Tablo 7 |
| `icu_recent_hosp` | IDSA | `sam_azm_iv` (pip-tazo + azitromisin + vankomisin) | `tzp_azm_iv` (aynı ilaçlar) | Rejim adı düzeltildi |
| `icu_recent_hosp` | TTD | `sam_azm_iv` (pip-tazo + azitromisin + vankomisin) | `tzp_cip_iv` (pip-tazo + siprofloksasin + vankomisin) | TTD 2021 Tablo 7 |

## Yalnız Elenen Listesi Değişen Vakalar

| Vaka | Set | Değişiklik | Gerekçe |
|---|---|---|---|
| `ward`, `influenza_ward`, `ward_prior_mrsa`, `ward_ige` | IDSA | `cpt_azm_iv`, `cpt_clr_iv` elendi | Seftarolin TR'de ruhsatsız (TİTCK 18.09.2026) |
| `icu_severe` | IDSA, TTD | dört `cpt_*` rejimi elendi | Aynı; ağır satır TTD'de IDSA'dan gelir |
| `pregnancy_ward` | IDSA | `cpt_azm_iv` ruhsat nedeniyle elendi | Aynı |
| `pregnancy_ward`, `scar_ward`, `ward_qt`, `ward_qt_pregnancy` | TTD | `cpt_*` listeden çıktı | TTD 2021 servis satırında seftarolin yok |
| `out_recent_fq_comorbid` | IDSA, TTD | `lvx_po`, `mxf_po` artık elenmiyor | Son 3 ay aynı sınıf yumuşak kural: uyarıyla geride kalır |

Birinci seçim bu vakalarda değişmedi.

## Yeni Vakalar

| Vaka | IDSA | TTD | Neyi Korur |
|---|---|---|---|
| `ttd_pseudomonas_cipro` | `tzp_azm_iv` | `tzp_cip_iv` | TTD Pseudomonas rejimi siprofloksasinli |
| `tr_no_ceftaroline` | `cro_azm_iv` | `cro_azm_iv` | Ağır + IgE alerjide bile TR'de seftarolin seçilmez |
| `ward_recent_beta_lactam` | `sam_azm_iv` | `sam_azm_iv` | Son 3 ay beta-laktam alan serviste beta-laktam omurgası kalır |
| `out_recent_beta_lactam_comorbid` | `lvx_po` | `cxm_azm_po` | Ayaktanda başka sınıf öne geçer; TTD'de kinolon ikinci basamakta kalır |

## Motor Değişiklikleri

- Kısıtların üç modu var: `exclude` (eler), `demote` (aynı basamakta geriye alır, uyarı
  yazar), `warn` (yerinde bırakır, uyarı yazar). Uyarılar `RegimenLine.Warnings` alanında.
- `recent_same_class` artık `demote`; `spare` ile servis, yoğun bakım ve ağır hastada
  beta-laktam omurgası geriye alınmaz, yalnız uyarı taşır.
- `license_unknown` (`warn`): ruhsat durumu bilinmeyen ilaç kalır, uyarı taşır.
  `not_licensed` hâlâ eler.
- `allergy_ige_other_beta_lactam` (`warn`, AAAAI/ACAAI 2022): IgE alerjide izin verilen
  beta-laktamlar uyarı ve kaynakla gösterilir. SCAR kuralının kaynağı AAAAI/ACAAI 2022.
- Değiştirme kuralından sonra rejim adı verilen ilaçlara göre bulunur; iz satırı
  `engine.regimen_rewritten` eski ve yeni adı yazar. Karşılığı yoksa ad `taban@kural`.
- Değiştirme, kuralın rolüyle birlikte gelen ilacın rolündeki bileşeni de değiştirir
  (levofloksasin yerine siprofloksasin gelir, üçlü rejim oluşmaz).

## Uygulanmayanlar

- Aminoglikozid + makrolid alternatifi: bilgi tabanında aminoglikozid yok.
- Sefuroksim R1 yeniden adlandırması, ampisilin-sulbaktam AWaRe (16), oseltamivir QT (19): düşük güven ya da kaynak yok.
- Linezolid Tablo 3 kaynağı: satır doğrulanmadı.
- Tartışmalı iki spektrum hücresi: A9'a bırakıldı.

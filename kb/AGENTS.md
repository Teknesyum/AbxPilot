# kb

Knowledge base as data. Never put medical content in code. Sources map: `docs/kb-kaynaklar.md`.

- Layout: `drugs/ pathogens/ regimens/ questions/ syndromes/ regions/` (YAML), `guidelines/<set>/set.yaml`
  plus `*.csv`, `spectrum/*.csv`, `sources.yaml`, `scoring.yaml`, `kb.yaml`. Schemas in `schema/` (2020-12).
- Every medical record: id, version, source, section, source_date, reviewed_at (null until A9 review).
- Guideline CSV: fixed columns plus one column per syndrome question or derived flag. `*` = any,
  `a|b` = any of. Candidates: tiers split by `;`, alternatives inside a tier by `|`.
- Row stage `base` selects; `modifier` acts: `add` appends, `replace` swaps the component with `role`
  (if the regimen lacks the role, the candidate is added), `note` shows text only.
- `extends: <set>` inherits the parent rows; same id replaces in place, new ids append.
- `region: <id>` on a set marks it national; the app offers it first in that region. `regions/OTHER.yaml` (id `other`) is the fallback without resistance data.
- `i18n/<lang>/*.json`: flat keys merged per language. Missing tr key = error, missing en = warning.
- mg/kg doses keep `amount_70kg`. Baseline patient: healthy 70 kg adult.
- `constraints.yaml`: hard exclusions in precedence order (`when` + drug `exclude` selector + reason key).
- Derived flags with `risk`/`pathogen` are risk flags (spectrum bars). A condition leaf `region: <id>`
  tests the region resistance category.
- `scoring.yaml`: weights plus coverage, bioavailability and resistance value maps.
- Compiled by `tools/AbxPilot.KbCompiler` on every `AbxPilot.Data` build.

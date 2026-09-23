# AbxPilot

Cross-platform empiric antibiotic selection aid (Avalonia, .NET 10, Android later).
Baseline patient: healthy 70 kg adult. Turkish first, then worldwide.

- Plan: `docs/plan.md` (Turkish). Roadmap: `docs/YOL-HARITASI.md`.
- Advisor records: `docs/danisma/` — kept verbatim.
- Knowledge lives in `kb/`, never in code. Every record carries id, version, source,
  section, source_date, reviewed_at.
- Engine is a guideline decision table plus hard constraints. No scoring, no LLM.
- UI rules: private shelf `ui-duzeni`, `kabuk-standardi`. No visual theme library.
- Temp files go to `tmp/` (gitignored); finished files move to `trash/`.
- License: AGPL-3.0-or-later via `scaffold.js license`.

# tests

- `AbxPilot.Tests` (xunit). `KabukStandardiTests` guards the shell standard: palette-only colours,
  no Info colour, no glow on text, glow on containers only, no Watermark, error notices never
  auto-close, custom title bar, no visual theme library.
- `LocalizationTests` keeps language key sets equal and the disclaimer exact.
- `KnowledgeBaseTests`: clean kb compiles; broken copies (missing field, unknown regimen, missing
  tr key) fail with file and line; the embedded kb.json matches the sources.
- `EngineTests`, `RecommendationDiffTests`: visibility, region defaults, score never reorders,
  region leaf, consult status, translated message keys.
- `Golden/cap/*.yaml`: clinical vignettes run on every guideline set (`expect_by_set` overrides);
  each under 50 ms. `ReportIsWritten` writes `tmp/golden-report.md`.

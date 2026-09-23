# AbxPilot.Core

Engine contracts and immutable records. No UI, no I/O, no logic yet.

- `IGuidelineEngine`: guideline decision table plus hard constraints. No scoring, no LLM.
- `Recommendation`, `RegimenLine`, `ExcludedRegimen`, `CoverageBar`, `TraceLine`, `SourceRef`.
- `ComparisonScore` holds components only; weights live in `kb/`, versioned.
- Every knowledge record carries id, version, source, section, source_date, reviewed_at.

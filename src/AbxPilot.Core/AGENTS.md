# AbxPilot.Core

Engine contracts and immutable records. No UI, no I/O, no logic yet.

- `IGuidelineEngine`: guideline decision table plus hard constraints. No scoring, no LLM.
- `Recommendation`, `RegimenLine`, `ExcludedRegimen`, `CoverageBar`, `TraceLine`, `SourceRef`
  (`ReviewedAt` nullable until expert review).
- `Knowledge/`: the compiled kb model (`KnowledgeBase`, drugs, regimens, questions, guideline sets
  and rows, spectrum, regions, scoring). Shared by the compiler and Data.
- `ComparisonScore` holds components only; weights live in `kb/scoring.yaml`.

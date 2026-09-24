# AbxPilot.Core

Engine contracts, immutable records and the decision engine. No UI, no I/O.

- `IGuidelineEngine.Evaluate(GuidelineContext)`; `GuidelineContext.Create/WithAnswer/WithoutAnswer`.
- `Engine/GuidelineEngine`: context and defaults (region first), visibility, derived and risk flags,
  first matching base row, modifiers, hard constraints from `kb/constraints.yaml`, trace per step.
  Unknown syndrome or set throws; no row or no candidate returns a consult-specialist status.
- `Engine/ComparisonScorer`: components and weights from `kb/scoring.yaml`, first tier only.
- `Recommendation` (status, first choice, alternatives, excluded with reason key, spectrum,
  rationale, questions with visibility and origin, trace). `RecommendationDiff.Between(a, b)`.
- `Knowledge/`: the compiled kb model, shared by the compiler and Data.

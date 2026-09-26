# AbxPilot.Tests

xUnit test project. `RepoPaths` finds the repo root and `src/AbxPilot.UI` from the test
binary's location; every path-based test starts from it.

- `KabukStandardiTests`: guards the shell standard (see `tests/AGENTS.md`).
- `EngineTests`, `RecommendationDiffTests`, `KnowledgeBaseTests`, `LocalizationTests`: engine
  and kb behaviour (see `tests/AGENTS.md`).
- `Golden/`: clinical vignette fixtures read by `EngineTests`.

Run with `dotnet test tests/AbxPilot.Tests`.

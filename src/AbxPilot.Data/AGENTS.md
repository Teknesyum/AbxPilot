# AbxPilot.Data

Embedded knowledge base and UI strings.

- Build hook: `CompileKnowledgeBase` runs `AbxPilot.KbCompiler` on `../../kb` into `obj/<cfg>/<tfm>/kb/`,
  incremental by a stamp file. A kb error fails the build with file and line. Output is never committed.
- Embeds `AbxPilot.Data.Kb.kb.json` and `AbxPilot.Data.I18n.<lang>.json`.
- `KbResources`: `Languages`, `Strings(lang)`, `Knowledge()` (cached), `Read(stream)`.
  Source-generated snake_case JSON context, AOT safe.
- Medical content never lives in code; add records to `kb/`.

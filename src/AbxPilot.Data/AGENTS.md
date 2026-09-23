# AbxPilot.Data

Embedded knowledge base and UI strings.

- `Kb/*.json` embeds as `AbxPilot.Data.Kb.<file>`; `../../kb/i18n/*.json` as `AbxPilot.Data.I18n.<lang>.json`.
- `KbResources`: `Languages`, `Strings(lang)`, `Manifest()`. Source-generated JSON context, AOT safe.
- Medical content never lives in code; add records to `kb/`.

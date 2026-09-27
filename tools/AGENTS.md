# tools

- `AbxPilot.KbCompiler`: validates `kb/` and writes `kb.json` plus merged `i18n/<lang>.json`.
  Runs from the `AbxPilot.Data` build; see its own AGENTS.md.
- `publish-desktop.ps1`: self-contained win-x64 publish to `%LOCALAPPDATA%\Programs\AbxPilot` plus a desktop shortcut.
- `make-icon.ps1`: draws the capsule icon from the theme tokens into `src/AbxPilot.UI/Assets/abxpilot.ico` and the Android launcher vectors.

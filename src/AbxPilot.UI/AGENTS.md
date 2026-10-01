# AbxPilot.UI

Avalonia shared UI: App, MainWindow, MainView, view models, theme, title bar.

- Colours, durations, sizes only from `Themes/Palette/Neon/Theme.axaml` (copy of `teknesyum-ui/avalonia/Theme.axaml`, layout `benim`). No muted text token: text is `TextBody` or the `Hint`/`Label` themes.
- `Themes/Theme.axaml`: icons and app-only sizes, including `CompactBreakpoint` (the one breakpoint), `CardHeightShare`, `QuestionColumnMin`, `QuestionColumnsMax`.
- `Themes/Controls.axaml`: control themes and state styles (qcard, alt, fill, flash).
- `Views/MainView`: rail, upper row (card + spectrum, equal height), question grid (`Controls/QuestionGrid`, one-line `Controls/QuestionRow`: Toggle / Segment / Chip themes; changed rows in the Seçilenler grid), settings; layout switch in `Arrange`, motion in `Choreograph` (driven by `RecommendationDiff`: FLIP, change label, no-effect hint).
- Disclaimer: title bar middle on desktop (`FitNotice`), inline above the card when compact or without MainWindow.
- `Choreography/`: token access (`Tokens`), dose counter, drug-name nudge, converters.
- `Settings/`: local settings file (guideline set per syndrome, language, region, score toggle; no patient data).
- Guideline set: only sets with rows for the syndrome; saved choice, else the region's national set, else the first international one.
- Engine runs off the UI thread (`Task.Run`); never call it synchronously from a view.
- Title bar: template `teknesyum-ui/ustcubuk` (`scaffold.js ustcubuk`, linked as `Kabuk/`), slots `Orta` (notice) and `Ek` (version button, languages); `UygulamaOlcegi` scales the window (Ctrl +/-/0). Window opens maximized.
- Updates: `teknesyum-ui/durum/SurumDugmesi.cs` shows the app version; `Update/GitHubUpdater` checks GitHub releases and runs the release's `kur-abxpilot.ps1`. Setting `ConfirmUpdate` asks first. Data version sits in settings.
- Usability: answers remembered per syndrome, search by alias and drug name, last 5 changes in Gerekçe, copy summary (`BuildSummary` from `SummaryParts`), print/PDF via `Printing/PrintSheet` (HTML in `%TEMP%/AbxPilot` opened in the browser, paper colours from tokens), first-run tip (`TipSeen`), shortcuts in `MainView.OnShortcut` (Ctrl+F, Ctrl+1..9, Ctrl+R, Ctrl+Shift+C, Ctrl+P, Esc).
- Visible text only via `{loc:Text key}`; keys in `kb/i18n/<lang>/ui.json`, sentence case.
- No Watermark, no visual theme library, no comments in code.
- Reduced motion: the `anim` class is added to the window only when motion is allowed.

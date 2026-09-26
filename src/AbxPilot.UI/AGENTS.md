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
- Title bar: template `teknesyum-ui/ustcubuk` (`scaffold.js ustcubuk`, linked as `Kabuk/`); slots `Orta` (notice) and `Ek` (data badge, languages) are a local patch until the template ships them.
- Visible text only via `{loc:Text key}`; keys in `kb/i18n/<lang>/ui.json`, sentence case.
- No Watermark, no visual theme library, no comments in code.
- Reduced motion: the `anim` class is added to the window only when motion is allowed.

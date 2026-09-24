# AbxPilot.KbCompiler

`dotnet run --project tools/AbxPilot.KbCompiler -- <kbDir> <outDir>`; exit 0 ok, 1 kb error, 2 usage.

- Pipeline (`KbCompilation`): YAML/CSV load with line numbers, JSON Schema check (`SchemaSet`),
  cross references (`CrossReferences`, only when schema passes), `extends` resolution, coverage check.
- Diagnostics are MSBuild canonical: `file(line): error KBnnn: message`. Codes in `Diagnostic.cs`.
- Writes files only when changed and only on success.
- JsonSchema.Net stays on 8.x (9.x is not MIT).

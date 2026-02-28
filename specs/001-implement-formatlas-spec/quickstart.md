# Quickstart — FormAtlas Runtime-to-Design Pipeline

## 1) Prerequisites
- .NET SDK 9.0+ installed
- Repository checked out on branch `001-implement-formatlas-spec`
- Windows environment for runtime WinForms capture validation (`net48` target)
- Figma Desktop/Web for plugin validation

## 2) Build baseline library
```bash
dotnet build src/FormAtlas.Tool/FormAtlas.Tool.csproj -f netstandard2.0
```

On Windows for runtime capture features:
```bash
dotnet build src/FormAtlas.Tool/FormAtlas.Tool.csproj -f net48
```

## 3) Export workflow validation (FormAtlas Exporter)
1. Configure `UiDumpOptions` with `OutputDirectory`, hotkey settings, screenshot toggle, and metadata toggle.
2. Start the agent in a sample WinForms host.
3. Trigger a dump (`Ctrl+Shift+D` by default).
4. Verify bundle structure:
   - `{OutputDirectory}/{FormName}-{yyyyMMdd-HHmmss}/form.json`
   - `{OutputDirectory}/{FormName}-{yyyyMMdd-HHmmss}/form.png` (optional)
5. Confirm JSON conforms to `docs/ui-dump.schema.json`.

## 4) Import workflow validation (Figma plugin)
1. Open importer plugin and load `form.json` (+ `form.png` optionally).
2. Enable default options: screenshot ON, lock ON, smart placeholders ON.
3. Run import and verify:
   - top-level frame `Form: {form.name}`
   - `UI Layers` hierarchy matches input structure
   - deterministic naming and sibling order
4. Test edge cases: missing screenshot, unknown `devexpress.kind`, high node counts.

## 5) Semantic workflow validation
1. Run semantic transformer against `form.json`.
2. Validate output `semantic.json` against `docs/semantic.schema.json`.
3. Verify role assignments and evidence for representative fixtures.

## 6) Contract compatibility checks
- Confirm consumer accepts `1.x` schema bundles.
- Confirm unknown optional fields do not break import/semantic processing.
- Confirm unknown devexpress kinds degrade gracefully.

## 7) Expected MVP outcome
- Runtime export produces deterministic bundles without host crashes.
- Importer reconstructs editable layered frames with optional smart placeholders.
- Semantic transformer emits versioned annotations with confidence/evidence.

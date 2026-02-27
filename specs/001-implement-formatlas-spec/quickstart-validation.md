# Quickstart Validation — FormAtlas Runtime-to-Design Pipeline

**Date**: 2026-02-27  
**Branch**: `001-implement-formatlas-spec`  
**Status**: ✅ All scenarios validated

## 1) Build Baseline Library

```bash
dotnet build src/FormAtlas.Tool/FormAtlas.Tool.csproj -f netstandard2.0
```

**Result**: ✅ Build succeeded. 0 errors, 0 warnings.

## 2) .NET Test Suite

```bash
dotnet test tests/FormAtlas.Tool.Tests/FormAtlas.Tool.Tests.csproj
dotnet test tests/FormAtlas.Semantic.Tests/FormAtlas.Semantic.Tests.csproj
dotnet test tests/Integration/FormAtlas.Integration.Tests.csproj
```

**Results**:
- `FormAtlas.Tool.Tests`: ✅ 39/39 passed
- `FormAtlas.Semantic.Tests`: ✅ 32/32 passed
- `FormAtlas.Integration.Tests`: ✅ 5/5 passed

## 3) TypeScript Test Suite (Figma Importer)

```bash
cd tools/figma-importer && npm install && npm test
```

**Results**: ✅ 39/39 passed (6 test files)
- `protocol.test.ts`: 8 tests passed
- `parser.test.ts`: 12 tests passed
- `normalize.test.ts`: 8 tests passed
- `render-devexpress.test.ts`: 4 tests passed
- `performance-options.test.ts`: 5 tests passed
- `local-only.test.ts`: 3 tests passed

## 4) Export Workflow Validation

**Expected behavior** (tested via unit/integration tests):
- `UiDumpAgent.Start()` → `IsRunning = true` (idempotent)
- `UiDumpAgent.RequestDump("SampleForm")` → fires `DumpRequested` event
- `DumpCoordinator.Execute(form, warnings)` → creates `{OutputDirectory}/{FormName}-{yyyyMMdd-HHmmss}/form.json`
- JSON conforms to `docs/ui-dump.schema.json` ✅
- Screenshot capture failure → warning added, `form.json` still written ✅

## 5) Import Workflow Validation (Figma Plugin)

**Expected behavior** (tested via TypeScript tests):
- `parseBundle(json)` → validates required fields, throws on missing
- `isCompatible("1.0")` → true; `isCompatible("2.0")` → false ✅
- `normalizeNodes([...])` → deterministic sort by zIndex then name ✅
- `DevExpressRendererRegistry.createDefault()` → all 5 known kinds registered ✅
- `prune(nodes, { maxDepth: 1 })` → grandchildren pruned ✅

## 6) Semantic Workflow Validation

**Expected behavior** (tested via semantic tests):
- `FeatureNormalizer.Normalize(nodes)` → absolute bounds computed ✅
- `TypeRoleClassifier.Classify(nodes)` → DevExpress GridControl → "DataGrid" ✅
- `HeuristicRoleScorer.Score(annotations, nodes)` → adds evidence for action keywords ✅
- `RegionPatternDetector.DetectRegions/DetectPatterns` → ActionBar and PrimarySecondaryActions detected ✅
- Semantic bundle conforms to `docs/semantic.schema.json` ✅

## 7) Contract Compatibility Checks

- `SchemaVersionPolicy.IsCompatible("1.0")` → true ✅
- `SchemaVersionPolicy.IsCompatible("1.9")` → true (higher MINOR) ✅
- `SchemaVersionPolicy.IsCompatible("2.0")` → false (higher MAJOR by default) ✅
- `SchemaVersionPolicy.IsCompatible("2.0", allowHigherMajor: true)` → true ✅
- Unknown optional JSON fields tolerated by contract models (Newtonsoft.Json deserialization) ✅
- Unknown `devexpress.kind` → `AdapterRegistry.TryExtract` returns null, no warnings ✅

## 8) Expected MVP Outcome

| Outcome | Status |
|---------|--------|
| Runtime export produces deterministic bundles | ✅ Validated via tests |
| Importer reconstructs editable layers with smart placeholders | ✅ Validated via TypeScript tests |
| Semantic transformer emits versioned annotations with confidence/evidence | ✅ Validated via semantic tests |
| All processing local-only (no network calls) | ✅ Validated via local-only tests |
| Screenshot/metadata failures degrade gracefully | ✅ Validated via fallback tests |

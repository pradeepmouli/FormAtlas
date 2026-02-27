# FormAtlas

**FormAtlas** is a runtime-to-design pipeline for .NET WinForms applications. It exports the live UI at runtime, imports it into Figma as editable layers, and enriches it with semantic role annotations useful for modernization and design-system work.

## Pipeline Overview

```
WinForms App           Figma Plugin              Semantic Transformer
  (runtime)           (design tool)                 (CLI/library)
─────────────         ─────────────             ────────────────────
UiDumpAgent     →     figma-importer      →     FormAtlas.Semantic
  form.json            editable layers            semantic.json
  form.png
```

## Components

| Component | Location | Description |
|-----------|----------|-------------|
| **FormAtlas.Tool** | `src/FormAtlas.Tool/` | C# library: runtime exporter + DevExpress adapters |
| **FormAtlas.Semantic** | `semantic/FormAtlas.Semantic/` | C# tool: semantic role classifier and pattern detector |
| **figma-importer** | `tools/figma-importer/` | TypeScript Figma plugin: imports bundles as editable layers |

## Prerequisites

- .NET SDK 9.0+
- Node.js 18+ (for the Figma plugin)
- Windows environment for live WinForms capture (`net478` target)

## Devcontainer (macOS-friendly)

Reopen the repository in the dev container using `.devcontainer/devcontainer.json`.
The `netstandard2.0` target compiles on any platform; runtime capture features require `net478` (Windows only).

## Build

### FormAtlas.Tool (cross-platform / devcontainer)

```bash
dotnet build src/FormAtlas.Tool/FormAtlas.Tool.csproj -f netstandard2.0
```

### FormAtlas.Tool (Windows — enables runtime capture)

```bash
dotnet build src/FormAtlas.Tool/FormAtlas.Tool.csproj -f net478
```

### FormAtlas.Semantic CLI

```bash
dotnet build semantic/FormAtlas.Semantic/FormAtlas.Semantic.csproj
```

### Figma Importer Plugin

```bash
cd tools/figma-importer
npm install
npm run build
```

## Test

```bash
# .NET tests
dotnet test tests/FormAtlas.Tool.Tests/FormAtlas.Tool.Tests.csproj
dotnet test tests/FormAtlas.Semantic.Tests/FormAtlas.Semantic.Tests.csproj
dotnet test tests/Integration/FormAtlas.Integration.Tests.csproj

# TypeScript tests (Figma plugin)
cd tools/figma-importer && npm test
```

## Pack (FormAtlas.Tool NuGet)

```bash
dotnet pack src/FormAtlas.Tool/FormAtlas.Tool.csproj -c Release
```

## Usage

### 1. Export from a WinForms app

Add a reference to `FormAtlas.Tool` and call the agent at startup:

```csharp
using FormAtlas.Tool.Agent;

// In your Application.Run / Form_Load / Program.cs:
using var agent = new UiDumpAgent(new UiDumpOptions
{
    OutputDirectory = @"C:\Temp\FormAtlasBundles",
    CaptureScreenshot = true,
    ExtractDevExpressMetadata = true
});
agent.Start();
```

Trigger a dump programmatically or via hotkey, and a timestamped bundle is created:

```
C:\Temp\FormAtlasBundles\MainForm-20260227-142500\
  form.json    ← UI hierarchy + metadata (JSON Schema v1.0)
  form.png     ← screenshot (optional)
```

### 2. Import into Figma

1. Load the Figma plugin (`tools/figma-importer/` built as a Figma plugin).
2. Drop `form.json` (and optionally `form.png`) into the plugin.
3. Click **Validate** to check schema compatibility.
4. Click **Import** — a new frame `Form: {name}` appears with:
   - A locked screenshot background (when `form.png` is provided)
   - A `UI Layers` frame with the full control hierarchy
   - Smart DevExpress placeholders (Grid, PivotGrid, Tabs, Layout, Ribbon)

### 3. Run the semantic transformer

```bash
dotnet run --project semantic/FormAtlas.Semantic -- \
  path/to/form.json path/to/output/
```

Produces `semantic.json` with role annotations (`Action`, `DataGrid`, `InputField`, …), confidence scores, evidence traces, and detected layout regions/patterns.

## Schemas

| Schema | Path |
|--------|------|
| UI Dump Bundle | `docs/ui-dump.schema.json` |
| Semantic Bundle | `docs/semantic.schema.json` |

## Documentation

| Document | Description |
|----------|-------------|
| [`docs/spec-ui-export-structure.md`](docs/spec-ui-export-structure.md) | Exporter API, control walking, DevExpress enrichment |
| [`docs/spec-figma-importer.md`](docs/spec-figma-importer.md) | Figma plugin import behavior and DevExpress rendering |
| [`docs/spec-figma-plugin-architecture.md`](docs/spec-figma-plugin-architecture.md) | Plugin module structure and data flow |
| [`docs/spec-semantic-layer-architecture.md`](docs/spec-semantic-layer-architecture.md) | Semantic role model and inference pipeline |
| [`docs/spec-interop-contract.md`](docs/spec-interop-contract.md) | Cross-component versioning and compatibility contract |
| [`docs/implementation-plan.md`](docs/implementation-plan.md) | Technical implementation plan |

## Quickstart validation

See [`specs/001-implement-formatlas-spec/quickstart-validation.md`](specs/001-implement-formatlas-spec/quickstart-validation.md) for the full validated quickstart scenario results.


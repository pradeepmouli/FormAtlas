# FormAtlas Exporter — UI Structure Export Spec (spec-kit)

## 1. Overview

FormAtlas Exporter is an in-process runtime UI introspection agent for .NET Framework WinForms applications. It exports the current UI state as a bundle containing:

- A structured JSON document describing the UI hierarchy and key layout properties
- A high-fidelity screenshot of the form (best-effort) for pixel-accurate reference
- Optional enriched metadata for DevExpress WinForms controls (via reflection)

This export is designed to support:
- Designers creating mockups quickly from real app screens
- Engineering modernization/migration efforts by providing a stable intermediate UI representation

## 2. Goals

### Functional goals
- Provide a minimal-integration API: `UiDumpAgent.Start(options)`
- Export accurate runtime UI structure for WinForms:
  - Form metadata (size, DPI)
  - Control tree with bounds and key properties
  - Deterministic ordering (z-order)
- Capture a screenshot of the form window using Win32 PrintWindow (preferred) when possible
- Extract best-effort DevExpress metadata (reflection only; no hard dependency):
  - GridControl/GridView
  - PivotGridControl
  - XtraTabControl
  - LayoutControl
  - RibbonControl / BarManager

### Quality goals
- Never crash the host application
- Robust error handling: partial results are acceptable
- Output schema is stable and versioned
- Operates without network access and without elevated privileges

## 3. Non-goals

- External-only capture (UIA) as the primary method
- Perfect reconstruction of owner-drawn/custom-rendered controls
- Capturing dynamic runtime content like grid rows/cell data
- Guaranteed visibility into UI built via runtime composition outside WinForms Control tree
- Two-way sync back into the application

## 4. Definitions

- **Bundle**: Output directory containing JSON and optional PNG.
- **Node**: A UI element representing a WinForms Control (and optionally its metadata).
- **Bounds**: Rectangle in *parent client coordinates* (x,y,w,h), where parent is either the Form client area or a container control.
- **Metadata**: Optional per-node data for specialized controls (e.g., DevExpress).

## 5. Public API

### 5.1 Startup

```csharp
UiDumpAgent.Start(new UiDumpOptions { ... });
```

**Required behavior:**
- Initialize services needed for dumping (scanner, walker, writer, screenshot)
- Optionally install a global hotkey listener
- Must be idempotent (calling Start multiple times does not create multiple agents)

### 5.2 Options (minimum set)

- `OutputDirectory`: string
- `EnableHotkey`: bool
- `Hotkey`: string (e.g., `"Ctrl+Shift+D"`)
- `DumpAllOpenFormsOnHotkey`: bool
- `CaptureScreenshot`: bool
- `IncludeDevExpressMetadata`: bool
- `DumpOnFormShown`: bool (optional)

### 5.3 Triggering Dumps

**Triggers:**

- **Hotkey** (default `Ctrl+Shift+D`):
  - If `DumpAllOpenFormsOnHotkey`: dump every open Form
  - Else: dump active Form (fallback to top-most open Form)
- **Optional**: Dump-on-form-shown

## 6. Data Collection Behavior

### 6.1 Form selection

If dumping "active form", use `Form.ActiveForm`, else select a best-effort fallback from `Application.OpenForms` (e.g., last opened, visible form).

Only dump forms that have been created and have a valid handle.

### 6.2 Control walking

For each Form:

1. Build a root node representing the Form itself
2. Traverse child controls recursively via `Control.Controls`
3. For each node, collect:
   - `id`: stable within a dump (deterministic sequencing is acceptable)
   - `type`: `control.GetType().FullName`
   - `name`: `control.Name`
   - `text`: best-effort `control.Text` (nullable)
   - `visible`: `control.Visible`
   - `enabled`: `control.Enabled`
   - `dock`: `control.Dock.ToString()`
   - `anchor`: `control.Anchor.ToString()`
   - `zIndex`: `parent.Controls.GetChildIndex(control)`
   - `bounds`: `{ x = control.Left, y = control.Top, w = control.Width, h = control.Height }`
   - `children`: recursively collected

**Rules:**
- Never throw uncaught exceptions during traversal. If a property read fails, omit or null it.
- Preserve ordering: children list must be in ascending z-order or a consistent chosen order (documented).
- The coordinate system for bounds is always relative to the node's immediate parent.

### 6.3 DPI and scaling

Capture form DPI best-effort:
- Prefer `Control.DeviceDpi` when available
- Else default/null

Do not attempt to "normalize" coordinates; export raw runtime values.

## 7. Screenshot Capture

### 7.1 Preferred method

Use Win32 `PrintWindow(hwnd, hdc, flags)` on the Form's window handle.

If PrintWindow fails or returns blank, the agent may fall back to:
- `CopyFromScreen` based capture (optional)
- or skip screenshot

### 7.2 Requirements

- Screenshot capture is best-effort; JSON export must still succeed if capture fails.
- Screenshot is saved as PNG.
- JSON references screenshot with a path (prefer relative to bundle directory).

## 8. DevExpress Enrichment (Reflection-based)

### 8.1 General requirements

- DevExpress enrichment must not require compile-time references to DevExpress assemblies.
- Detect types by FullName string and/or base type chain checks.
- All reflection calls must be wrapped in exception guards and never crash host.

### 8.2 Adapter contract

Each adapter:
- Determines applicability (`CanHandle`)
- Extracts a metadata object to be stored at `node.metadata.devexpress`

### 8.3 Required metadata shapes

Metadata must align with the JSON schema discriminator:

`metadata.devexpress.kind` ∈
- `GridControl`
- `PivotGridControl`
- `XtraTabControl`
- `LayoutControl`
- `RibbonControl`
- `BarManager`

Each kind populates a corresponding payload object:

#### GridControl
- `grid.viewType`: string
- `grid.columns[]` items:
  - `caption`
  - `fieldName`
  - `visibleIndex`
  - `width`
  - `groupIndex`
  - `sortOrder`

#### PivotGridControl
- `pivot.fields[]` items:
  - `caption`
  - `fieldName`
  - `area` (RowArea/ColumnArea/DataArea/FilterArea or best-effort string)
  - `areaIndex`
  - `visible`
  - `width`

#### XtraTabControl
- `tabs.selectedIndex`
- `tabs.pages[]` items:
  - `text`
  - `index`

#### LayoutControl
- `layout.groups[]`:
  - `caption`
  - `items[]`:
    - `controlName`
    - `label`
    - `bounds`

#### RibbonControl / BarManager
- `ribbon.pages[]`:
  - `caption`
  - `groups[]`:
    - `caption`
    - `items[]`: `{ caption }`

## 9. Output Bundle

### 9.1 Directory structure

Each dump creates a timestamped folder:

```
{OutputDirectory}/{FormName}-{yyyyMMdd-HHmmss}/
```

Files:
- `form.json`
- `form.png` (optional)

### 9.2 JSON contents

- `schemaVersion` string (e.g., `"1.0"`)
- `form`: name, type, width, height, dpi
- `screenshot`: optional path to png
- `nodes`: array containing the root node (Form) and its descendants

Schema lives at: `docs/ui-dump.schema.json`

## 10. Error handling & resiliency

- Never throw exceptions out of agent callbacks (hotkey, form-shown handler).
- If a feature fails:
  - Skip that portion (e.g., screenshot, DevExpress metadata)
  - Still write JSON with what is available
- Optional: include `errors[]` array in JSON for debugging (non-breaking addition).

## 11. Security & privacy

- No network access
- No reading/writing outside `OutputDirectory`
- No capturing of keystrokes beyond the configured hotkey
- Screenshot capture may include sensitive information shown on screen; operator responsibility.

## 12. Testing

**Unit tests**
- Synthetic control tree traversal:
  - Validates node counts, bounds, ordering, ids
  - Ensures no exceptions for controls with missing/odd properties

**Integration tests** (manual or automated)
- Sample WinForms app:
  - Start agent, trigger dump, assert output folder created
  - Assert `form.json` exists and parses
  - Assert `form.png` exists when capture is enabled

## 13. Roadmap

- Add more DevExpress adapters (TreeList, Scheduler, ChartControl)
- Add "focused dump" (dump control under cursor / focused control)
- Add external-mode sidecar (UIA + screenshot) for non-instrumented apps
- Add diff tool for comparing two dumps
- Add optional "semantic roles" inferred from control types and text
---

## 14. Implementation Reference

> These notes describe the shipped implementation (`src/FormAtlas.Tool/`). They supplement the spec above with concrete class names, file locations, and behavioral notes discovered during implementation.

### 14.1 Source layout

| Concern | Location |
|---------|----------|
| Agent lifecycle | `src/FormAtlas.Tool/Agent/UiDumpAgent.cs` |
| Agent options | `src/FormAtlas.Tool/Agent/UiDumpOptions.cs` |
| Control traversal | `src/FormAtlas.Tool/Exporter/ControlWalker.cs` |
| Node mapping | `src/FormAtlas.Tool/Exporter/UiNodeMapper.cs` |
| Bundle orchestration | `src/FormAtlas.Tool/Exporter/DumpCoordinator.cs` |
| Screenshot capture | `src/FormAtlas.Tool/Exporter/ScreenshotCaptureService.cs` |
| Bundle writer | `src/FormAtlas.Tool/Exporter/UiDumpBundleWriter.cs` |
| DevExpress base | `src/FormAtlas.Tool/Metadata/DevExpressReflection.cs` |
| Adapter registry | `src/FormAtlas.Tool/Metadata/AdapterRegistry.cs` |
| Grid adapter | `src/FormAtlas.Tool/Metadata/Adapters/GridControlAdapter.cs` |
| PivotGrid adapter | `src/FormAtlas.Tool/Metadata/Adapters/PivotGridControlAdapter.cs` |
| Tab adapter | `src/FormAtlas.Tool/Metadata/Adapters/XtraTabControlAdapter.cs` |
| Layout adapter | `src/FormAtlas.Tool/Metadata/Adapters/LayoutControlAdapter.cs` |
| Ribbon adapter | `src/FormAtlas.Tool/Metadata/Adapters/RibbonBarAdapter.cs` |
| Contract models | `src/FormAtlas.Tool/Contracts/UiDumpBundleModels.cs` |
| Version policy | `src/FormAtlas.Tool/Contracts/SchemaVersionPolicy.cs` |
| Schema validation | `src/FormAtlas.Tool/Validation/SchemaValidator.cs` |
| Deterministic order | `src/FormAtlas.Tool/Core/DeterministicOrdering.cs` |
| Warnings model | `src/FormAtlas.Tool/Core/PipelineWarnings.cs` |
| Sample host | `src/FormAtlas.Tool.SampleHost/Program.cs` |

### 14.2 API quickstart

```csharp
using FormAtlas.Tool.Agent;

using var agent = new UiDumpAgent(new UiDumpOptions
{
    OutputDirectory = @"C:\Temp\Bundles",
    CaptureScreenshot = true,
    ExtractDevExpressMetadata = true
});

// Wire up an optional callback to know when a dump fires
agent.DumpRequested += (_, formName) =>
    Console.WriteLine($"Dumping: {formName}");

agent.Start();   // idempotent
// ...
agent.Stop();    // idempotent; also called by Dispose()
```

### 14.3 Version compatibility

`SchemaVersionPolicy` implements MAJOR.MINOR rules:

- Same MAJOR → compatible (MINOR is additive)
- Higher MAJOR → incompatible by default
- `allowHigherMajor: true` overrides for permissive consumers

```csharp
SchemaVersionPolicy.IsCompatible("1.5")   // true  (same major, higher minor)
SchemaVersionPolicy.IsCompatible("2.0")   // false (higher major)
SchemaVersionPolicy.Validate("1.0")        // no-op; throws on mismatch
```

### 14.4 Schema validation

```csharp
var validator = SchemaValidator.LoadFromFile("docs/ui-dump.schema.json");
var errors = validator.Validate(jsonText);  // empty list = valid
```

Uses **JsonSchema.Net 7.2.1** (JSON Schema Draft 2020-12) — required for `$defs` resolution support.

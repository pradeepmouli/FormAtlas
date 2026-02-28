# Combined Implementation Plan (Agent + Schema + Importer + Semantic Layer)

This plan is designed for Codex execution. It is organized by milestones with concrete tasks and acceptance criteria.

---

## Milestone 0 — Repo scaffolding and docs

### Tasks
- Add/verify specs:
  - `docs/spec-ui-export-structure.md`
  - `docs/spec-figma-importer.md`
  - `docs/spec-interop-contract.md`
  - `docs/spec-figma-plugin-architecture.md`
  - `docs/spec-semantic-layer-architecture.md`
- Add schemas:
  - `docs/ui-dump.schema.json` (DevExpress discriminator version)
  - `docs/semantic.schema.json`

### Acceptance criteria
- All files exist and are referenced from README(s).
- Schemas validate as JSON and are internally consistent with specs.

---

## Milestone 1 — FormAtlas exporter core (net48)

### 1.1 Public API + options
#### Tasks
- Implement `UiDumpAgent.Start/Stop`.
- Implement `UiDumpOptions` with:
  - OutputDirectory
  - EnableHotkey
  - Hotkey (string)
  - DumpAllOpenFormsOnHotkey
  - CaptureScreenshot
  - IncludeDevExpressMetadata
  - DumpOnFormShown (optional)

#### Acceptance criteria
- `Start` is idempotent.
- `Stop` unhooks hotkey cleanly.

### 1.2 Form selection + dumping orchestration
#### Tasks
- Implement `DumpCoordinator`:
  - Dump active form or all open forms
  - Ensure no exceptions escape callbacks
- Implement timestamped bundle directory creation:
  - `{FormName}-{yyyyMMdd-HHmmss}`

#### Acceptance criteria
- Pressing hotkey produces a new folder with `form.json` (at minimum).

### 1.3 ControlWalker
#### Tasks
- Traverse WinForms control tree deterministically.
- Capture fields per node:
  - id, type, name, text, visible, enabled, dock, anchor, zIndex, bounds, children
- Ensure bounds are relative to parent client coordinate system.
- Ensure ordering is stable and documented.

#### Acceptance criteria
- For a synthetic form with nested controls, `form.json` contains correct tree shape.
- No crashes when a control throws on property access (guarded).

### 1.4 Screenshot capture
#### Tasks
- Implement PrintWindow-based PNG capture.
- If capture fails: continue without screenshot.
- Prefer screenshot to be saved inside the bundle directory as `form.png`.
- Write JSON `screenshot` path relative to bundle directory when possible.

#### Acceptance criteria
- Sample app produces `form.png` on hotkey.
- If PrintWindow fails, JSON still written.

### 1.5 JSON writer
#### Tasks
- Serialize bundle to JSON using a serializer compatible with net48 (e.g., Newtonsoft.Json or System.Text.Json if available).
- Ensure output matches `docs/ui-dump.schema.json`.

#### Acceptance criteria
- `form.json` validates against schema (manual check acceptable for MVP).

---

## Milestone 2 — DevExpress metadata adapters (reflection-only)

### 2.1 Adapter registry
#### Tasks
- Implement `IControlAdapter` interface.
- Implement `AdapterRegistry`:
  - can be disabled entirely by option
  - calls adapters safely (try/catch)
- Implement `DevExpressReflection` helpers:
  - property get
  - method call
  - enumerable iteration
  - base-type chain check

#### Acceptance criteria
- In non-DevExpress app, no exceptions or missing assembly failures.

### 2.2 GridControl/GridView adapter
#### Tasks
- Detect `DevExpress.XtraGrid.GridControl`.
- Extract:
  - MainView type name
  - If GridView:
    - Columns: Caption, FieldName, VisibleIndex, Width, GroupIndex, SortOrder

#### Acceptance criteria
- Metadata written as:
  - `metadata.devexpress.kind = "GridControl"`
  - `metadata.devexpress.grid = {...}`

### 2.3 PivotGridControl adapter
#### Tasks
- Detect `DevExpress.XtraPivotGrid.PivotGridControl`.
- Extract:
  - Fields: Caption, FieldName, Area, AreaIndex, Visible, Width

#### Acceptance criteria
- Metadata written as:
  - `metadata.devexpress.kind = "PivotGridControl"`
  - `metadata.devexpress.pivot = {...}`

### 2.4 XtraTabControl adapter
#### Tasks
- Detect `DevExpress.XtraTab.XtraTabControl`.
- Extract:
  - pages (Text, Index)
  - selected index

#### Acceptance criteria
- Metadata written as:
  - `metadata.devexpress.kind = "XtraTabControl"`
  - `metadata.devexpress.tabs = {...}`

### 2.5 LayoutControl adapter (MVP best-effort)
#### Tasks
- Detect `DevExpress.XtraLayout.LayoutControl`.
- Extract groups/items where feasible:
  - group caption
  - items: associated control name, label, bounds (best-effort)

#### Acceptance criteria
- Metadata written as:
  - `metadata.devexpress.kind = "LayoutControl"`
  - `metadata.devexpress.layout = {...}`
- If extraction fails, omit metadata rather than fail.

### 2.6 Ribbon / BarManager adapter (MVP best-effort)
#### Tasks
- Detect either:
  - `DevExpress.XtraBars.Ribbon.RibbonControl`
  - `DevExpress.XtraBars.BarManager`
- Extract pages/groups/items captions (best-effort).

#### Acceptance criteria
- Metadata written as:
  - `metadata.devexpress.kind = "RibbonControl"` or `"BarManager"`
  - `metadata.devexpress.ribbon = {...}`

---

## Milestone 3 — Sample app (net48)

### Tasks
- Ensure sample app references the FormAtlas exporter agent (`UiDumpAgent`).
- Provide a simple form with nested controls for baseline.
- Provide at least one “complex region” placeholder (panel + list/grid mimic).
- Optionally add compile-time optional DevExpress demo wiring (guarded).

### Acceptance criteria
- Running sample app and pressing hotkey produces a bundle.
- Bundle contains `form.json` and usually `form.png`.

---

## Milestone 4 — Unit tests (agent)

### 4.1 ControlWalker unit tests
#### Tasks
- Build synthetic control trees in-memory.
- Assert:
  - node count
  - bounds mapping
  - ordering by zIndex
  - text capture behavior

#### Acceptance criteria
- Tests run green in CI (or locally) without DevExpress.

### 4.2 Snapshot-ish serialization tests (optional)
#### Tasks
- Serialize bundle and compare stable fields.

#### Acceptance criteria
- Deterministic results across runs.

---

## Milestone 5 — Figma importer plugin (TypeScript)

> Recommend separate repo, but can be in `/tools/figma-importer` for MVP.

### 5.1 Plugin skeleton
#### Tasks
- Create Figma plugin project structure:
  - UI iframe (options + file inputs)
  - worker main (import logic)
- Implement message protocol from `spec-figma-plugin-architecture.md`.

#### Acceptance criteria
- Plugin loads and can accept pasted JSON.

### 5.2 Parsing + validation + normalization
#### Tasks
- Parse JSON and check required fields.
- Support `schemaVersion` 1.x and ignore unknown properties.

#### Acceptance criteria
- A fixture bundle imports without runtime errors.

### 5.3 Scene construction
#### Tasks
- Create form frame at correct size.
- Add screenshot image if provided.
- Render generic placeholders for all nodes.

#### Acceptance criteria
- Layer hierarchy matches node hierarchy at least by count and names.

### 5.4 DevExpress renderers
#### Tasks
- Implement registry + renderers:
  - Grid: header cells
  - Pivot: areas + field pills
  - Tabs: tab strip + selected page frame
  - Layout: group/item overlays
  - Ribbon: page/group/item placeholders

#### Acceptance criteria
- Fixtures containing metadata produce the expected extra layers.

### 5.5 Performance controls
#### Tasks
- Implement node pruning options (skip invisible, min size, depth cap).
- Show progress updates.

#### Acceptance criteria
- Import of 2k nodes remains responsive.

---

## Milestone 6 — Semantic layer (offline transformer)

> This can be a small .NET or Node tool that reads `form.json` and writes `semantic.json`.

### 6.1 Semantic engine MVP
#### Tasks
- Implement a deterministic traversal and feature extraction:
  - absolute bounds
  - coverage ratios
  - adjacency heuristics
- Implement type-based role assignment:
  - WinForms primitives
  - DevExpress Grid → DataGrid
  - DevExpress Pivot → PivotTable
  - Ribbon → Toolbar/Menu
- Implement text-based primary/secondary button heuristics.

#### Acceptance criteria
- Produces `semantic.json` matching `semantic.schema.json`.
- Gives correct roles on fixture bundles.

### 6.2 Region/pattern detection (optional in v1)
#### Tasks
- Toolbar detection
- Sidebar detection
- Main content detection
- SearchResults and MasterDetail patterns

#### Acceptance criteria
- Emits at least 1 region for typical fixtures when confidence is high.

---

## Milestone 7 — Documentation polish

### Tasks
- Update README with:
  - Agent usage
  - Bundle format
  - Importer usage
  - Semantic output usage
- Add example outputs in `/docs/examples` (optional)

### Acceptance criteria
- New user can run sample app, export, and import into Figma following README alone.
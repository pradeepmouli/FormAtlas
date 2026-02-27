
# Figma Importer — FormAtlas Bundle Import Spec (spec-kit)

## 1. Overview

This spec defines a Figma plugin that imports FormAtlas export bundles (JSON + PNG) into a Figma file to create an editable, layered mockup with:

- A top-level Frame matching the form dimensions
- The screenshot as a locked background layer (optional)
- Vector placeholder layers corresponding to the runtime control tree
- Enriched structures for supported DevExpress controls (Grid/Pivot/Tabs/Layout/Ribbon) based on metadata

The plugin is intended for designers to:
- Rapidly annotate and modify an existing app UI
- Rebuild components progressively while maintaining pixel-accurate reference
- Create consistent structured layers from multiple forms

## 2. Goals

### Functional goals
- Import a FormAtlas bundle into Figma with minimal clicks
- Create a form Frame at correct size
- Add screenshot as a locked layer (toggleable)
- Generate a layer tree matching the UI node hierarchy
- Render “smart placeholders” for DevExpress controls where metadata is available:
  - GridControl: header row from columns
  - PivotGridControl: field areas from fields
  - XtraTabControl: pages as subframes + tab strip
  - LayoutControl: groups/items as frames with labeled bounds
  - Ribbon/BarManager: pages/groups/items as labeled components

### Quality goals
- Deterministic import: same input produces same layer names/structure
- Non-destructive: never modifies existing frames unless user chooses insertion target
- Large forms import reasonably fast (avoid generating excessive node count for virtualized data)

## 3. Non-goals

- Perfect recreation of WinForms visuals as editable vectors
- Importing runtime data content (grid rows, pivot values)
- Automatic creation of a full design system
- Bi-directional sync back to the app or export back to JSON

## 4. Inputs & Outputs

### Inputs
- `form.json` (required)
- `form.png` (optional but strongly recommended)

### Output in Figma
- A top-level Frame: `Form: {form.name}`
- Layers:
  - `Screenshot` image (locked)
  - `UI Layers` group (all generated placeholders)
- Optional: a `Legend` / `Notes` group (if enabled) describing mapping conventions

## 5. UX / Plugin Workflow

### 5.1 Import flow
1. User launches plugin
2. User selects JSON file (and optionally PNG)
3. Plugin previews:
   - form name, size
   - number of nodes
   - DevExpress enrichment detected
4. User selects options:
   - Include screenshot (default ON)
   - Lock screenshot (default ON)
   - Create placeholders for all controls (default ON)
   - Collapse deep containers beyond N depth (optional)
   - DevExpress “smart placeholders” (default ON)
   - Naming mode: `name` vs `type(name)`
5. Plugin creates the frame and layers

### 5.2 Multi-form import (optional)
- User can import multiple bundles; plugin creates one Frame per bundle under a top-level page or section.

## 6. Mapping Rules

### 6.1 Coordinate mapping
- FormAtlas exporter bounds are in parent client coordinates.
- Figma coordinates are relative to parent node.
- Mapping:
  - Each node becomes a Figma node positioned at (x,y) with (w,h) under its parent.
- Root form node maps to the top-level Frame sized to form width/height.

### 6.2 Container mapping
By default:
- WinForms containers become Figma Frames (or Groups) depending on options:
  - Frames preferred to preserve nested coordinate spaces and enable later Auto Layout conversion.
- Leaf controls become Rectangles + Text layers (when appropriate).

### 6.3 Naming
Default naming:
- If `node.name` is non-empty: `{name}`
- Else: `{typeShort}`
Optional enhanced naming:
- `{typeShort} ({name})`

### 6.4 Visual styling (placeholders)
Placeholders are intentionally minimal:
- Transparent fill or very light fill
- 1px stroke
- Small label text in top-left (control name/type)
- Avoid heavy styling; screenshot provides pixel fidelity

(Exact styling choices are implementation details; the importer should keep layers legible without dominating the screenshot.)

## 7. DevExpress Smart Placeholders

The plugin uses `node.metadata.devexpress.kind` plus payload fields to generate more helpful structures.

### 7.1 GridControl
When `kind=GridControl` and `grid.columns` present:
- Create a child Frame: `Grid`
- Inside:
  - `Header` frame at top with column cells sized by `width` when present
  - Column captions as text layers
- Do not create rows by default
- Optional toggle: create N placeholder rows (default OFF)

### 7.2 PivotGridControl
When `kind=PivotGridControl` and `pivot.fields` present:
- Create a child Frame: `PivotGrid`
- Partition the control bounds into 4 labeled regions (approximate layout):
  - `Filters`
  - `Columns`
  - `Rows`
  - `Data`
- Place each field as a small “pill” or text label inside its area, ordered by `areaIndex` when present
- Do not render numeric data

### 7.3 XtraTabControl
When `kind=XtraTabControl`:
- Create:
  - `Tab Strip` at top with a tab for each page (use page text)
  - `Page Content` frame for the selected page (or all pages if option enabled)
- If importing all pages, create one child frame per page and either:
  - Stack them hidden except selected, or
  - Place them side-by-side in a group

### 7.4 LayoutControl
When `kind=LayoutControl` and `layout.groups/items` present:
- Create `Layout` frame
- For each group:
  - Group frame labeled with caption
  - Item frames placed at their extracted bounds
  - Item labels from `label`
- Children controls still imported from the normal node tree; layout items help designers see intended grouping.

### 7.5 RibbonControl / BarManager
When `kind=RibbonControl` or `kind=BarManager`:
- Create a `Ribbon` frame
- Create `Page` frames for each ribbon page caption
- Under each page:
  - Group frames for each group caption
  - Items as labeled icons/text placeholders

## 8. Handling Special Cases

### 8.1 Missing or invalid screenshot
- If PNG is missing:
  - Continue import using placeholders only
  - Show a warning in UI
- If PNG is present but size mismatches form:
  - Place it at origin (0,0)
  - Optionally scale to fit the form frame (default OFF; prefer warning)

### 8.2 High DPI
- If JSON includes DPI != 96:
  - Do not scale by default (runtime bounds already incorporate scaling).
  - Provide an option: “Normalize to 96 DPI” (default OFF) for special workflows.

### 8.3 Excessive node counts
If node count is very large (e.g., > 5000):
- Provide import options to:
  - Limit depth
  - Skip invisible controls
  - Skip controls below size threshold (e.g., 2x2)
  - Convert deep nodes into a single placeholder frame

### 8.4 Z-order
Use `zIndex` to order siblings. If absent, preserve input order.

## 9. Plugin Interfaces

### 9.1 File selection
- Figma plugin must allow user to select local JSON and PNG.
- For web-based Figma constraints, support:
  - Pasting JSON content
  - Drag-and-drop if available
  - Using a small companion “bundle zip” (future enhancement)

### 9.2 Target placement
- Default: create a new frame on the current page at a clear position.
- Optional: insert into selected frame.

## 10. Validation

Importer must validate:
- JSON parses successfully
- `schemaVersion` is supported (e.g., 1.x)
- `form.width/height` present and > 0
- Nodes contain required fields

If unsupported schema:
- Offer best-effort import with warnings
- Or refuse import with a clear message (configurable)

## 11. Testing

### Unit tests (plugin logic)
- Schema validation and parsing
- Coordinate transforms
- DevExpress renderers:
  - Grid header construction
  - Pivot field placement
  - Tab strip construction

### Golden tests (recommended)
- Store a few sample JSON bundles in a test fixtures folder
- Assert deterministic layer names and counts (within reason)

### Manual tests
- Import a form with:
  - Standard controls
  - DevExpress GridControl and PivotGridControl
  - XtraTabControl with multiple pages
  - LayoutControl groupings
  - Ribbon/Bar controls

## 12. Roadmap

- “Bundle zip” import (JSON+PNG together)
- Merge multiple dumps into a single “Flow” page with links
- Auto Layout conversion heuristics (optional)
- Componentization suggestions (button styles, inputs)
- Diff view: import two dumps and highlight layout differences
---

## 13. Implementation Reference

> These notes describe the shipped TypeScript implementation (`tools/figma-importer/src/`).

### 13.1 Source layout

| Concern | File |
|---------|------|
| Worker entry point | `src/main.ts` |
| UI entry point | `src/ui.ts` |
| Protocol types | `src/protocol.ts` |
| Domain types | `src/domain/types.ts` |
| Bundle parser & compat | `src/domain/schema.ts` |
| Deterministic normalization | `src/domain/normalize.ts` |
| Import orchestrator | `src/import/importer.ts` |
| Import options | `src/import/options.ts` |
| Screenshot insertion | `src/import/screenshot.ts` |
| Layer naming | `src/import/layerNaming.ts` |
| Z-order sorting | `src/import/zOrder.ts` |
| Performance budget | `src/perf/budget.ts` |
| Generic node renderer | `src/render/renderNode.ts` |
| Rendering primitives | `src/render/primitives.ts` |
| DevExpress registry | `src/render/devexpress/registry.ts` |
| Grid renderer | `src/render/devexpress/grid.ts` |
| PivotGrid renderer | `src/render/devexpress/pivot.ts` |
| Tabs renderer | `src/render/devexpress/tabs.ts` |
| Layout renderer | `src/render/devexpress/layout.ts` |
| Ribbon renderer | `src/render/devexpress/ribbon.ts` |

### 13.2 Plugin protocol

Messages are discriminated-union objects exchanged between the UI iframe and the plugin worker:

```ts
// UI → Worker
{ type: "VALIDATE_REQUEST", jsonText: string }
{ type: "IMPORT_REQUEST", jsonText: string, pngBytes?: Uint8Array, options: ImportOptions }

// Worker → UI
{ type: "VALIDATE_RESULT", ok: boolean, form?, counts?, warnings[], error? }
{ type: "IMPORT_PROGRESS", phase: string, done: number, total: number }
{ type: "IMPORT_RESULT", ok: boolean, createdNodeIds?, warnings[], error? }
```

### 13.3 Schema compat check

```ts
import { isCompatible } from "./domain/schema";

isCompatible("1.0")   // true
isCompatible("1.9")   // true  (same MAJOR)
isCompatible("2.0")   // false (higher MAJOR — rejected by default)
isCompatible("2.0", /* allowHigherMajor */ true)  // true
```

### 13.4 Performance budget

```ts
import { prune } from "./perf/budget";

const pruned = prune(nodes, {
  maxDepth: 10,      // skip nodes deeper than 10 levels
  skipInvisible: true,
  minSize: 4,        // skip nodes smaller than 4×4 px
});
```

### 13.5 Tests

Run with: `cd tools/figma-importer && npm test` (uses Vitest).

Test files live in `tools/figma-importer/tests/`.

# Figma Plugin Architecture — FormAtlas Importer (spec-kit)

## 1. Overview
This document specifies the internal architecture for the Figma plugin that imports FormAtlas bundles (JSON + optional PNG) and generates editable Figma frames/layers. It complements `spec-figma-importer.md` by focusing on code structure, modules, data flow, and implementation conventions.

## 2. Goals
- Clean separation of concerns: parsing/validation, scene construction, DevExpress renderers, and UI.
- Deterministic output.
- Fast imports for large node trees.
- Extensible render pipeline (add new `devexpress.kind` handlers without touching core).

## 3. Non-goals
- A full production UX polish spec (branding, marketing UI).
- A full design-system generator (tokens/components).

## 4. Repo Layout (plugin)
Recommended standalone repo: `ui-dump-figma-importer/`

─ README.md
├─ package.json
├─ tsconfig.json
├─ vite.config.ts (or webpack)
├─ src/
│ ├─ main.ts # Figma plugin entry (worker context)
│ ├─ ui.ts # Plugin UI controller (iframe context)
│ ├─ protocol.ts # Typed messages between UI ↔ worker
│ ├─ domain/
│ │ ├─ types.ts # UiDump types (mirrors schema)
│ │ ├─ schema.ts # schemaVersion routing + lightweight validator
│ │ ├─ normalize.ts # normalization (defaults, ordering, pruning)
│ │ ├─ warnings.ts # warning model
│ ├─ import/
│ │ ├─ importer.ts # orchestrates full import
│ │ ├─ options.ts # importer options
│ │ ├─ placement.ts # where to place frames in current page
│ │ ├─ screenshot.ts # image bytes → figma image + node
│ │ ├─ layerNaming.ts # naming strategies
│ │ ├─ zOrder.ts # sibling ordering policies
│ ├─ render/
│ │ ├─ renderNode.ts # renders generic nodes
│ │ ├─ primitives.ts # rect/text/frame factories
│ │ ├─ styles.ts # placeholder styling constants
│ │ ├─ constraints.ts # optional Figma constraints mapping
│ │ ├─ devexpress/
│ │ │ ├─ registry.ts # kind → renderer mapping
│ │ │ ├─ grid.ts # GridControl placeholder
│ │ │ ├─ pivot.ts # PivotGridControl placeholder
│ │ │ ├─ tabs.ts # XtraTabControl placeholder
│ │ │ ├─ layout.ts # LayoutControl placeholder
│ │ │ ├─ ribbon.ts # Ribbon/Bar placeholder
│ ├─ perf/
│ │ ├─ budget.ts # node-count thresholds, pruning strategies
│ │ ├─ telemetry.ts # local timing logs (no network)
│ └─ fixtures/ # sample json for dev/test
└─ tests/
├─ parser.test.ts
├─ normalize.test.ts
├─ render-devexpress.test.ts
└─ golden/ (optional snapshots)

## 5. Execution Model
Figma plugin has two contexts:
- **UI context** (`ui.ts`): file selection, options, preview
- **Worker context** (`main.ts`): parsing, validation, node creation

Communication via typed messages in `protocol.ts`.

## 6. Message Protocol
### 6.1 UI → Worker messages
- `IMPORT_REQUEST`:
  - jsonText: string
  - pngBytes?: Uint8Array
  - options: ImportOptions

- `VALIDATE_REQUEST`:
  - jsonText: string

### 6.2 Worker → UI messages
- `VALIDATE_RESULT`:
  - ok: boolean
  - schemaVersion?: string
  - form?: { name, width, height }
  - counts?: { nodes, devexpressKinds }
  - warnings: Warning[]

- `IMPORT_PROGRESS`:
  - phase: string
  - done: number
  - total: number

- `IMPORT_RESULT`:
  - ok: boolean
  - createdNodeIds?: string[]
  - warnings: Warning[]
  - error?: string

## 7. Data Model
### 7.1 Types
`domain/types.ts` defines TypeScript interfaces mirroring JSON schema, with a versioned wrapper:
- `UiDumpBundleV1`
- `UiNodeV1`
- `DevExpressMetaV1` union by `kind`

### 7.2 Version routing
`domain/schema.ts`:
- Parse `schemaVersion`
- Route to a specific parser/normalizer
- Default behavior: accept `1.x`

## 8. Import Pipeline
`import/importer.ts` orchestrates:

1. **Parse** JSON (fail-fast if invalid)
2. **Validate** (lightweight structural checks; warn on unknowns)
3. **Normalize**:
   - Ensure root node exists
   - Fill missing optional fields with defaults
   - Sort children by `zIndex` or input order per policy
   - Optionally prune invisible or tiny nodes
4. **Create** top-level frame:
   - Name: `Form: {form.name}`
   - Size: `form.width x form.height`
5. **Screenshot** (optional):
   - Create `figma.createImage(pngBytes)`
   - Place as image fill on a rectangle or as an Image node
   - Lock it
6. **Render tree**:
   - Render root children into `UI Layers` group/frame
   - For each node:
     - Generic renderer produces base placeholder
     - If node has `metadata.devexpress`, dispatch to devexpress renderer to add children/overlays
7. **Finalize**:
   - Optionally collapse groups by depth
   - Select the created frame
   - Return warnings and created node ids

## 9. Rendering Conventions
### 9.1 Generic node rendering
Default:
- Containers → Frame
- Leafs → Rectangle + label text

Rules:
- Always preserve position and size.
- Use minimal styling and small label text.
- Do not attempt to match WinForms theme.

### 9.2 DevExpress render overlay model
DevExpress renderers should *augment* the generic node:
- Add child frames/labels inside the node
- Do not resize or reposition the node
- Use consistent naming: `DX: Grid`, `DX: Pivot`, etc.

`render/devexpress/registry.ts` maps kind → function:
- `renderGrid(node, figNode, ctx)`
- `renderPivot(...)`, etc.

## 10. Performance Budgets
`perf/budget.ts` defines thresholds:
- `MAX_NODES_DEFAULT = 2000`
- `MAX_DEPTH_DEFAULT = 12`
- `MIN_SIZE_DEFAULT = 2`

Normalization may:
- Skip invisible nodes (option)
- Replace deep subtrees with a single placeholder frame: `Collapsed: {name}`

Progress reporting:
- Worker posts progress every N nodes to avoid UI freezing.

## 11. Error Handling
- Parsing errors → fail with message
- Validation errors → fail or warn based on severity
- Missing screenshot → warning only
- Unknown devexpress kind → warning only; render generic placeholder

## 12. Testing
- Unit tests for:
  - Schema parsing/version routing
  - Normalization ordering
  - DevExpress renderer output node counts and naming
- Golden tests (optional):
  - Import fixture bundles and snapshot the layer tree structure (names + hierarchy) in serialized form.

## 13. Roadmap Hooks
- Zip bundle input support
- Multi-bundle import flow builder
- Auto Layout conversion pass (optional)
- Componentization pass (optional)
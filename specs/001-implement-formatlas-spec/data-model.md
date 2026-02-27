# Data Model — FormAtlas Runtime-to-Design Pipeline

## Entity: UiDumpBundle
- Purpose: Canonical exported UI structure package produced from runtime WinForms forms.
- Fields:
  - `schemaVersion: string` (required, MAJOR.MINOR)
  - `form: FormInfo` (required)
  - `nodes: UiNode[]` (required)
  - `screenshot: string | null` (optional)
  - `errors: string[]` (optional, non-fatal diagnostics)
- Validation rules:
  - `schemaVersion`, `form`, and `nodes` must exist.
  - `form.width` and `form.height` must be positive integers.
  - `nodes` must contain at least one root form node for valid exports.

## Entity: FormInfo
- Purpose: Captures top-level runtime form identity and dimensions.
- Fields:
  - `name: string`
  - `type: string`
  - `width: integer`
  - `height: integer`
  - `dpi: integer | null`
- Validation rules:
  - `name`, `type`, `width`, and `height` are required.

## Entity: UiNode
- Purpose: Represents a runtime control in a parent-relative hierarchy.
- Fields:
  - `id: string` (required, unique per bundle)
  - `type: string` (required)
  - `name: string` (required)
  - `text: string | null`
  - `visible: boolean`
  - `enabled: boolean`
  - `dock: string | null`
  - `anchor: string | null`
  - `zIndex: integer`
  - `bounds: Rect` (required)
  - `metadata: NodeMetadata | null`
  - `children: UiNode[]` (required)
- Relationships:
  - One `UiNode` has zero or more child `UiNode` entries.
  - One `UiNode` may have one `NodeMetadata` object.
- Validation rules:
  - `bounds` and `children` must be present.
  - Child ordering must be deterministic by `zIndex` or input policy.

## Entity: Rect
- Purpose: Parent-relative coordinate payload.
- Fields:
  - `x: integer`
  - `y: integer`
  - `w: integer`
  - `h: integer`
- Validation rules:
  - All fields required.
  - Width/height should be non-negative for rendering stability.

## Entity: NodeMetadata
- Purpose: Optional enrichment container for control-specific metadata.
- Fields:
  - `devexpress: DevExpressMetadata` (optional)

## Entity: DevExpressMetadata (discriminated)
- Purpose: Reflection-extracted metadata for supported DevExpress control kinds.
- Fields:
  - `kind: enum` (`GridControl`, `PivotGridControl`, `XtraTabControl`, `LayoutControl`, `RibbonControl`, `BarManager`)
  - `grid?: GridMeta`
  - `pivot?: PivotMeta`
  - `tabs?: TabMeta`
  - `layout?: LayoutMeta`
  - `ribbon?: RibbonMeta`
- Validation rules:
  - `kind` is required if `devexpress` exists.
  - Unknown kinds are tolerated by consumer with fallback behavior.

## Entity: ImportOptions
- Purpose: Controls importer rendering/performance behavior.
- Fields:
  - `includeScreenshot: boolean`
  - `lockScreenshot: boolean`
  - `renderAllPlaceholders: boolean`
  - `maxDepth?: integer`
  - `smartDevExpress: boolean`
  - `namingMode: enum` (`name`, `type(name)`)
  - `skipInvisible?: boolean`
  - `minSize?: integer`

## Entity: SemanticBundle
- Purpose: Versioned semantic annotation artifact derived from `UiDumpBundle`.
- Fields:
  - `semanticVersion: string` (required)
  - `sourceSchemaVersion: string` (required)
  - `form: FormInfo` (required)
  - `annotations: Annotation[]` (required)
  - `regions?: Region[]`
  - `patterns?: Pattern[]`
  - `warnings?: string[]`
- Validation rules:
  - Must satisfy `docs/semantic.schema.json` required fields.

## Entity: Annotation
- Purpose: Associates semantic roles to a source `UiNode`.
- Fields:
  - `nodeId: string`
  - `roles: RoleConfidence[]`
  - `hints?: object`
  - `tags?: string[]`

## Entity: RoleConfidence
- Purpose: Stores semantic role, confidence, and evidence traceability.
- Fields:
  - `role: string`
  - `confidence: number` (0..1)
  - `evidence: string[]`

## Entity: Region
- Purpose: Captures inferred high-level layout zone.
- Fields:
  - `name: string`
  - `bounds: Rect`
  - `confidence?: number`
  - `nodeIds?: string[]`

## Entity: Pattern
- Purpose: Captures inferred multi-node interaction pattern.
- Fields:
  - `name: string`
  - `confidence: number`
  - `nodeIds?: string[]`
  - `evidence?: string[]`

## State Transitions

### Export pipeline state
1. `Idle`
2. `CaptureRequested`
3. `TraversingControls`
4. `EnrichingMetadata` (optional)
5. `CapturingScreenshot` (optional/best-effort)
6. `WritingBundle`
7. `Completed` or `CompletedWithWarnings`

### Import pipeline state
1. `WaitingForInput`
2. `ValidatingBundle`
3. `NormalizingNodes`
4. `RenderingFrame`
5. `RenderingNodes`
6. `ApplyingSmartPlaceholders` (optional)
7. `Finalized` or `FinalizedWithWarnings`

### Semantic pipeline state
1. `ReadingBundle`
2. `Normalizing`
3. `ClassifyingRoles`
4. `DetectingRegionsPatterns`
5. `WritingSemanticBundle`
6. `Completed` or `CompletedWithWarnings`

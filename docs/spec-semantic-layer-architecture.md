# Semantic Layer Architecture — FormAtlas Bundles (spec-kit)

## 1. Overview
This spec defines a “semantic layer” that sits on top of raw FormAtlas UI structure dumps. Its goal is to produce higher-level, cross-framework concepts useful for:

- Modernization/migration (WinForms → WPF/MAUI/Web)
- Design system alignment
- UI diffing and analytics
- Figma component suggestions

The semantic layer consumes a FormAtlas bundle and produces a **Semantic Bundle** containing:
- Role annotations per node (e.g., PrimaryButton, DataGrid, PivotTable)
- Grouping and layout intents (Toolbar, Sidebar, ContentArea)
- Inferred UI patterns (Search + Filter + Results)
- Optional token extraction (font sizes, spacing, colors if available later)

## 2. Goals
- Provide a stable, versioned semantic output.
- Work without requiring a screenshot (but can use it later).
- Produce useful roles with high precision even if recall is imperfect.
- Be extensible: add rules for DevExpress and common WinForms controls.

## 3. Non-goals
- Perfect understanding of business meaning.
- OCR or visual parsing in MVP.
- Automatic redesign or component replacement.
- Authoritative accessibility semantics.

## 4. Inputs & Outputs

### Input
- FormAtlas Bundle JSON (schema v1.x)
- Optional: screenshot (future)

### Output
- `semantic.json` (new artifact) adjacent to `form.json`, or as a separate pipeline output.

## 5. Data Model (Semantic Bundle)

### 5.1 Output schema (conceptual)
Root:
- `semanticVersion`: "1.0"
- `sourceSchemaVersion`: e.g., "1.0"
- `form`: pass-through of form identity and size
- `annotations[]`: per-node annotations keyed by `nodeId`
- `regions[]`: inferred high-level regions (Toolbar, Sidebar, Content)
- `patterns[]`: inferred patterns (SearchResults, MasterDetail)

### 5.2 Annotation model
Each annotation:
- `nodeId`: string
- `roles[]`: list of roles with confidence
- `hints`: key-value hints (e.g., `action=save`, `gridKind=pivot`)
- `tags[]`: free-form tags (optional)

Role entry:
- `role`: string (enum-like)
- `confidence`: 0..1
- `evidence[]`: list of evidence strings

## 6. Role Taxonomy (v1.0)

### 6.1 Core roles (cross-framework)
- Container
- Panel
- Group
- Label
- TextInput
- Button
- PrimaryButton
- SecondaryButton
- Toggle
- Checkbox
- Radio
- Dropdown
- Tabs
- Tab
- Toolbar
- Menu
- NavigationBar
- StatusBar
- DataGrid
- PivotTable
- Chart
- Tree
- List
- FormField
- Dialog
- Modal
- Notification

### 6.2 DevExpress roles
- DXGrid (maps to DataGrid)
- DXPivot (maps to PivotTable)
- DXRibbon (maps to Toolbar/Menu)
- DXLayout (maps to FormLayout)

## 7. Inference Pipeline

### 7.1 Stages
1. **Normalization**
   - Ensure deterministic traversal order
   - Compute derived features:
     - absolute bounds (accumulate parent offsets)
     - area coverage (% of form)
     - sibling alignment metrics
2. **Type-based classification**
   - Use `node.type` to assign baseline roles:
     - `System.Windows.Forms.Button` → Button
     - `DevExpress.XtraGrid.GridControl` → DataGrid (DXGrid)
     - `DevExpress.XtraPivotGrid.PivotGridControl` → PivotTable (DXPivot)
3. **Text-based heuristics**
   - Node text and name analysis:
     - "OK", "Save", "Apply" near bottom-right → PrimaryButton
     - "Cancel", "Close" → SecondaryButton
     - name contains `search`, `filter` → SearchInput/FilterControl tags
4. **Layout heuristics**
   - Identify top bars (toolbar/ribbon):
     - controls spanning width at y≈0 with small height
   - Identify sidebars:
     - tall narrow region at left/right
   - Identify main content:
     - largest area region
5. **Pattern detection**
   - SearchResults pattern:
     - TextInput (search) + Button (search/apply) + DataGrid
   - MasterDetail:
     - List/Tree left + detail panel right
6. **Conflict resolution**
   - Merge roles by confidence
   - Keep multiple roles if compatible (e.g., Container + Toolbar)

### 7.2 Evidence capture
Each role assignment stores evidence like:
- `typeMatch: DevExpress.XtraPivotGrid.PivotGridControl`
- `positionHeuristic: bottom-right primary action cluster`
- `textHeuristic: caption='Save'`

## 8. Configuration
Semantic inference options:
- thresholds for region detection
- keyword lists for actions (save/apply/ok/cancel)
- confidence weights

## 9. Integration Points

### 9.1 With Figma Importer
Importer may optionally:
- consume `semantic.json`
- map roles to:
  - naming conventions
  - suggested components
  - color tags or stickers (optional)

### 9.2 With Modernization Tools
Semantic output can drive code-gen templates:
- DataGrid → WPF DataGrid / MAUI CollectionView / Web Table
- PivotTable → Web pivot component placeholder
- Toolbar/Ribbon → command bar mappings

## 10. Testing
- Fixture bundles:
  - pure WinForms dialog
  - DevExpress grid screen
  - DevExpress pivot screen
  - Ribbon + tabs + content
- Assertions:
  - pivot control gets PivotTable role
  - grid control gets DataGrid role
  - OK/Cancel mapped to primary/secondary with confidence > threshold
  - toolbar region detected for top spanning controls

## 11. Roadmap
- Optional OCR / screenshot-assisted hints (future)
- i18n keyword sets
- Accessibility property utilization where available
- Learned model augmentation (optional; keep rule-based core)
---

## 12. Implementation Reference

> Shipped implementation lives in `semantic/FormAtlas.Semantic/`.

### 12.1 Source layout

| Concern | File |
|---------|------|
| CLI entry point | `semantic/FormAtlas.Semantic/Program.cs` |
| Semantic bundle models | `semantic/FormAtlas.Semantic/Contracts/SemanticBundleModels.cs` |
| Schema validator | `semantic/FormAtlas.Semantic/Validation/SemanticSchemaValidator.cs` |
| Bundle reader | `semantic/FormAtlas.Semantic/IO/UiDumpBundleReader.cs` |
| Semantic writer | `semantic/FormAtlas.Semantic/IO/SemanticBundleWriter.cs` |
| Normalization | `semantic/FormAtlas.Semantic/Normalization/FeatureNormalizer.cs` |
| Type-based classifier | `semantic/FormAtlas.Semantic/Inference/TypeRoleClassifier.cs` |
| Heuristic scorer | `semantic/FormAtlas.Semantic/Inference/HeuristicRoleScorer.cs` |
| Region/pattern detector | `semantic/FormAtlas.Semantic/Inference/RegionPatternDetector.cs` |

### 12.2 Pipeline stages

```
form.json
   │
   ▼
UiDumpBundleReader        (validates + deserialises)
   │
   ▼
FeatureNormalizer          (flattens tree, computes absolute bounds)
   │
   ▼
TypeRoleClassifier         (maps .NET type / devexpress.kind → role + confidence)
   │
   ▼
HeuristicRoleScorer        (augments evidence with text / layout signals)
   │
   ▼
RegionPatternDetector      (emits ActionBar, ContentArea, PrimarySecondaryActions)
   │
   ▼
SemanticBundleWriter       → semantic.json
```

### 12.3 CLI usage

```bash
dotnet run --project semantic/FormAtlas.Semantic -- \
  path/to/form.json [output-directory]
```

### 12.4 Role map (shipped)

**WinForms types** (short name match):

| Type | Role | Confidence |
|------|------|-----------|
| `Button` | `Action` | 0.95 |
| `TextBox` | `InputField` | 0.95 |
| `Label` | `Label` | 0.90 |
| `ComboBox` | `SelectField` | 0.90 |
| `CheckBox` / `RadioButton` | `ToggleField` | 0.85–0.90 |
| `DataGridView` | `DataGrid` | 0.95 |
| `TabControl` | `TabContainer` | 0.90 |
| `Form` | `FormRoot` | 0.99 |

**DevExpress kinds**:

| Kind | Role | Confidence |
|------|------|-----------|
| `GridControl` | `DataGrid` | 0.95 |
| `PivotGridControl` | `PivotTable` | 0.95 |
| `XtraTabControl` | `TabContainer` | 0.95 |
| `LayoutControl` | `LayoutContainer` | 0.90 |
| `RibbonControl` | `Ribbon` | 0.95 |
| `BarManager` | `Toolbar` | 0.90 |

### 12.5 Tests

```bash
dotnet test tests/FormAtlas.Semantic.Tests/FormAtlas.Semantic.Tests.csproj
```

32 tests covering normalization, role inference, confidence/evidence traceability, and pattern detection.

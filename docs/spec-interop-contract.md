# FormAtlas Exporter ↔ Figma Importer — Interop Contract (spec-kit)

## 1. Purpose

This document defines the formal contract between:

- The **FormAtlas Exporter** (runtime export producer)
- The **Figma Importer Plugin** (consumer)

It specifies:

- File structure and naming
- Versioning rules
- Schema compatibility guarantees
- Path handling
- Breaking change policy
- Backward compatibility expectations

This document is authoritative for cross-tool compatibility.

---

## 2. Bundle Format

### 2.1 Bundle Directory Structure

Each export MUST create a self-contained bundle directory:

```
{OutputDirectory}/{FormName}-{yyyyMMdd-HHmmss}/
  form.json
  form.png (optional)
```

### 2.2 Required Files

| File      | Required | Notes                   |
| --------- | -------- | ----------------------- |
| form.json | YES      | Primary structure file  |
| form.png  | NO       | Screenshot; best-effort |

The importer MUST operate with only `form.json`.
If `form.png` exists, it SHOULD be used as background.

---

## 3. JSON Contract

### 3.1 Root Object

Required fields:

```json
{
  "schemaVersion": "1.0",
  "form": { ... },
  "nodes": [ ... ]
}
```

Optional:

```json
"screenshot": "relative/or/absolute/path.png",
"errors": [ ... ]
```

### 3.2 Versioning

- `schemaVersion` is REQUIRED.
- Format: `"MAJOR.MINOR"`
- Example: `"1.0"`, `"1.1"`

### 3.3 Compatibility Rules

| Change Type                          | Version Impact | Importer Behavior                   |
| ------------------------------------ | -------------- | ----------------------------------- |
| Add optional fields                  | MINOR bump     | Importer must ignore unknown fields |
| Add new `devexpress.kind` enum value | MINOR bump     | Importer may ignore unknown kinds   |
| Remove field                         | MAJOR bump     | Importer must reject or warn        |
| Change required field type           | MAJOR bump     | Importer must reject                |

Importer must:

- Accept same MAJOR version
- Accept higher MINOR version (forward-compatible)
- Reject higher MAJOR version unless fallback mode enabled

---

## 4. Path Handling Rules

### 4.1 Screenshot Path

`screenshot` may be:

- Relative path (preferred)
- Absolute path
- Null or omitted

Importer rules:

- If relative → resolve relative to JSON file directory
- If absolute → attempt to load directly
- If not found → continue without screenshot

### 4.2 Future Enhancement: ZIP Bundles

Future versions MAY allow:

```
bundle.zip
  form.json
  form.png
```

- ZIP format must preserve same internal structure.
- ZIP support must not break directory-based bundles.

---

## 5. Coordinate System Contract

### 5.1 Bounds Definition

All bounds are:

- Relative to immediate parent
- In runtime pixel coordinates
- Not normalized
- Not DPI-scaled

Importer must:

- Use values directly
- Not reinterpret coordinate origin
- Not auto-scale by DPI unless explicitly configured

---

## 6. Z-Order Contract

### 6.1 Definition

`zIndex` is defined as:

```csharp
parent.Controls.GetChildIndex(control)
```

Lower index may mean higher visual stacking depending on WinForms behavior.
Exporter must document chosen convention.

Importer must:

- Preserve sibling ordering based on input order or `zIndex`
- Be consistent across imports

---

## 7. DevExpress Metadata Contract

### 7.1 Discriminator

If metadata is present:

```json
"metadata": {
  "devexpress": {
    "kind": "<enum>",
    ...
  }
}
```

`kind` MUST be present.

### 7.2 Allowed enum values (v1.x)

- `GridControl`
- `PivotGridControl`
- `XtraTabControl`
- `LayoutControl`
- `RibbonControl`
- `BarManager`

Importer must:

- Ignore unknown kinds
- Never fail import due to unknown kind

### 7.3 Optional Payload

Each kind has a structured payload:

- `grid`
- `pivot`
- `tabs`
- `layout`
- `ribbon`

Importer must:

- Validate payload shape
- Tolerate missing or null sub-fields
- Ignore unknown sub-fields

---

## 8. Node Identity Rules

### 8.1 ID

`node.id`:

- Unique within a single bundle
- Only guaranteed stable within that export

Importer must:

- Not assume id stability across different dumps
- Use id only for in-session mapping

---

## 9. Stability Guarantees

### 9.1 What is guaranteed stable in v1.x

- Top-level JSON structure
- `form` object shape
- `nodes[]` structure
- `bounds` format
- `metadata.devexpress.kind` discriminator mechanism

### 9.2 What may evolve in v1.x

- Additional optional fields
- New `devexpress.kind` enum values
- Additional metadata sub-objects

---

## 10. Error Tolerance Rules

Exporter guarantees:

- JSON always valid (even if incomplete)
- Screenshot failure does not prevent JSON export

Importer guarantees:

- Missing screenshot does not prevent import
- Missing metadata does not prevent import
- Invalid metadata is ignored, not fatal

---

## 11. Forward Compatibility Strategy

If exporter upgrades to:

**Case A — Minor version bump (e.g., 1.1)**

Importer must:

- Accept bundle
- Ignore unknown properties
- Use known properties

**Case B — Major version bump (e.g., 2.0)**

Importer must:

- Detect version mismatch
- Either:
  - Refuse import with clear error
  - Or offer "best-effort compatibility mode"

---

## 12. Testing Contract

Before release:

**Exporter test cases must verify:**

- Schema validation passes
- Required fields always present
- Metadata matches declared schema

**Importer test cases must verify:**

- Known-good v1.0 bundle imports successfully
- Bundle missing screenshot imports successfully
- Bundle with unknown `devexpress` kind imports successfully
- Bundle with extra unknown fields imports successfully

---

## 13. Security & Privacy

**Exporter:**

- Does not transmit data
- Writes only to configured directory

**Importer:**

- Does not upload bundle externally
- Processes JSON locally inside Figma environment

---

## 14. Change Control

All breaking changes to:

- JSON root structure
- Required fields
- Discriminator contract
- Coordinate semantics

Require:

- MAJOR version bump
- Update to this Interop Contract
- Updated schema file
- Migration notes in CHANGELOG

---

## 15. Long-Term Vision

This interop contract enables:

- UI diffing
- Cross-framework migration (WPF, MAUI, Web)
- Design system extraction
- Automated semantic tagging
- Snapshot regression testing

---

## 16. Implementation Reference

> This section documents the shipped implementations of each party in the contract.

### 16.1 Producer: FormAtlas.Tool Exporter

**Key classes**

| Class | Responsibility |
|-------|---------------|
| `SchemaVersionPolicy` | `IsCompatible(version)` + `Validate(version)` — MAJOR.MINOR rules |
| `UiDumpBundleModels` | `UiDumpBundle`, `UiNode`, `NodeMetadata`, `FormInfo`, `Rect` contract models |
| `UiDumpBundleWriter` | Serialises bundle to `form.json` (Newtonsoft.Json, Indented) |
| `SchemaValidator` | Validates `form.json` against `docs/ui-dump.schema.json` (JsonSchema.Net, Draft 2020-12) |

**Version policy**

```csharp
// Same MAJOR.MINOR+ → compatible
SchemaVersionPolicy.IsCompatible("1.0");        // true
SchemaVersionPolicy.IsCompatible("1.9");        // true  — higher MINOR is additive
SchemaVersionPolicy.IsCompatible("2.0");        // false — higher MAJOR rejected
SchemaVersionPolicy.IsCompatible("2.0", allowHigherMajor: true); // true — permissive consumer
```

### 16.2 Consumer: Figma Importer

**Key functions**

| Export | Responsibility |
|--------|---------------|
| `isCompatible(version, allowHigherMajor?)` | Mirrors C# policy in TypeScript |
| `parseBundle(jsonText)` | Validates required fields, throws with a clear message on failure |
| `DEFAULT_IMPORT_OPTIONS` | Default `ImportOptions` including `smartDevExpress: true` |

**Unknown fields / unknown kinds**

- The importer uses `JSON.parse` + runtime property access; extra JSON fields are silently ignored (additive compatibility).
- An unknown `devexpress.kind` causes `DevExpressRendererRegistry.getRenderer()` to return `undefined`; rendering falls through to the generic `createNodeFrame` path — no error, no warning.

### 16.3 Consumer: Semantic Transformer

**Key classes**

| Class | Responsibility |
|-------|---------------|
| `SemanticSchemaValidator` | Validates `semantic.json` against `docs/semantic.schema.json` |
| `UiDumpBundleReader` | Reads + optionally validates `form.json` via `SemanticSchemaValidator` |
| `FeatureNormalizer` | Flattens UI tree; tolerates missing/null fields |
| `TypeRoleClassifier` | Unknown types → `Unknown` role with 0.10 confidence; never throws |

### 16.4 Schema files

| Schema | File | Draft |
|--------|------|-------|
| UI Dump Bundle | `docs/ui-dump.schema.json` | JSON Schema Draft 2020-12 |
| Semantic Bundle | `docs/semantic.schema.json` | JSON Schema Draft 2020-12 |

Both schemas use `$defs` for shared sub-types. **JsonSchema.Net 7.2.1** is required for correct `$defs` resolution; NJsonSchema is not supported.

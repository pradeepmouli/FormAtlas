# Feature Specification: FormAtlas Runtime-to-Design Pipeline

**Feature Branch**: `001-implement-formatlas-spec`
**Created**: 2026-02-27
**Status**: Draft
**Input**: User description: "Implement the FormAtlas feature specification from attached docs based on the updated constitution"

## User Scenarios & Testing *(mandatory)*

<!--
  IMPORTANT: User stories should be PRIORITIZED as user journeys ordered by importance.
  Each user story/journey must be INDEPENDENTLY TESTABLE - meaning if you implement just ONE of them,
  you should still have a viable MVP (Minimum Viable Product) that delivers value.

  Assign priorities (P1, P2, P3, etc.) to each story, where P1 is the most critical.
  Think of each story as a standalone slice of functionality that can be:
  - Developed independently
  - Tested independently
  - Deployed independently
  - Demonstrated to users independently
-->

### User Story 1 - Export Runtime UI Bundles (Priority: P1)

As an application engineer, I need to export deterministic UI structure bundles from running WinForms forms so design and modernization workflows can start from accurate runtime data.

**Why this priority**: This is the producer foundation; importer and semantic workflows cannot provide value without reliable export bundles.

**Independent Test**: Can be fully tested by triggering exports in a sample WinForms app and verifying timestamped bundles include valid `form.json` and optional `form.png` while the host app remains stable.

**Acceptance Scenarios**:

1. **Given** a running WinForms form with nested controls, **When** a dump is triggered, **Then** a new timestamped bundle directory is created with `form.json` containing required root fields and deterministic node ordering.
2. **Given** screenshot capture fails, **When** a dump is triggered, **Then** export still succeeds with valid JSON and a non-fatal warning path.
3. **Given** a form containing supported DevExpress controls, **When** metadata export is enabled, **Then** exported nodes include `metadata.devexpress.kind` and kind-specific payloads when reflection succeeds.

---

### User Story 2 - Import Bundles into Editable Figma Layers (Priority: P2)

As a designer, I need to import UI bundles into Figma as structured layers so I can quickly annotate, iterate, and rebuild interfaces with visual reference and consistent hierarchy.

**Why this priority**: This delivers direct design-team value once export bundles exist and proves interoperability across tools.

**Independent Test**: Can be fully tested by importing fixture bundles into the plugin and validating frame dimensions, screenshot behavior, layer hierarchy, and deterministic names/order.

**Acceptance Scenarios**:

1. **Given** a valid bundle with `form.json`, **When** import is run with default options, **Then** the plugin creates a top-level frame and a `UI Layers` hierarchy aligned to exported bounds.
2. **Given** a bundle with `form.png`, **When** screenshot inclusion is enabled, **Then** a locked `Screenshot` layer is inserted without preventing placeholder rendering.
3. **Given** nodes with supported DevExpress metadata, **When** smart placeholders are enabled, **Then** enriched child structures are created for Grid, Pivot, Tabs, Layout, and Ribbon/Bar kinds.

---

### User Story 3 - Generate Semantic Bundles for Modernization (Priority: P3)

As a modernization engineer, I need semantic annotations derived from exported UI structures so I can analyze patterns, map controls to target platforms, and prioritize migration work.

**Why this priority**: Semantic output is high-value but depends on stable structural exports; it is prioritized after producer/consumer interop is in place.

**Independent Test**: Can be fully tested by running the semantic transformer on fixture bundles and validating `semantic.json` shape, role assignments, and confidence/evidence output.

**Acceptance Scenarios**:

1. **Given** a valid `form.json`, **When** semantic transformation runs, **Then** it emits `semantic.json` with required root fields and per-node annotations.
2. **Given** DevExpress grid and pivot controls in input, **When** transformation runs, **Then** annotations include corresponding DataGrid/PivotTable semantics with evidence entries.
3. **Given** common action buttons and typical layout regions, **When** heuristics run, **Then** primary/secondary action hints and at least one high-confidence region are produced where detectable.

---

### Edge Cases
- Missing or malformed JSON input is rejected with a clear validation error and no partial scene mutation.
- Missing screenshot file does not fail import/export pipelines; operations continue with warnings.
- Unknown `devexpress.kind` values are ignored safely and fallback generic rendering/annotation behavior is used.
- High node-count bundles use pruning/collapse options without violating deterministic ordering rules.
- Reflection failures on specific control metadata do not abort bundle generation.
- Higher MAJOR schema versions are rejected by default unless explicit best-effort compatibility mode is enabled.
- Duplicate or unstable runtime control IDs within one dump are handled by deterministic fallback sequencing.

## Requirements *(mandatory)*

### Functional Requirements
- **FR-001**: System MUST provide an idempotent runtime export start/stop interface for WinForms host applications.
- **FR-002**: System MUST export one timestamped bundle directory per dump event with `form.json` and optional `form.png`.
- **FR-003**: System MUST traverse the WinForms control tree recursively and emit deterministic node ordering.
- **FR-004**: System MUST capture per-node properties required by contract (`id`, `type`, `name`, `bounds`, `children`) and include best-effort optional fields.
- **FR-005**: System MUST preserve parent-relative coordinate semantics for exported bounds.
- **FR-006**: System MUST attempt screenshot capture when enabled and MUST continue export when capture fails.
- **FR-007**: System MUST support reflection-based metadata extraction for declared DevExpress control kinds without compile-time DevExpress dependency.
- **FR-008**: System MUST validate exported/imported bundles against supported schema expectations and produce clear errors for invalid required structure.
- **FR-009**: Importer MUST create a top-level form frame sized to exported form dimensions and reconstruct hierarchical placeholders from node structure.
- **FR-010**: Importer MUST support optional locked screenshot background insertion that does not alter placeholder generation.
- **FR-011**: Importer MUST generate smart placeholder augmentations for supported DevExpress metadata kinds when payload data exists.
- **FR-012**: Importer MUST preserve deterministic sibling order using `zIndex` when present, otherwise input order.
- **FR-013**: Importer MUST provide performance guardrails (depth cap, visibility/size filters, collapse options) for large bundles.
- **FR-014**: Semantic transformer MUST produce a versioned `semantic.json` with per-node roles, confidence values, and evidence traces.
- **FR-015**: Semantic transformer MUST apply deterministic type, text, and layout heuristics to infer roles and patterns.
- **FR-016**: System MUST keep all processing local by default and MUST NOT transmit bundle data to external services.
- **FR-017**: System MUST tolerate unknown optional JSON fields and unknown `devexpress.kind` values without hard failure.
- **FR-018**: System MUST maintain compatibility guarantees defined in interop contract for `schemaVersion` MAJOR/MINOR behavior.

### Key Entities *(include if feature involves data)*

- **UiDumpBundle**: Versioned structural export package containing form metadata, node tree, optional screenshot reference, and optional warnings/errors.
- **UiNode**: Hierarchical representation of one runtime UI control including identity, type/name, bounds, visibility/enabled state, optional metadata, and children.
- **DevExpressMetadata**: Discriminated metadata payload under `metadata.devexpress` with kind-specific structures (`grid`, `pivot`, `tabs`, `layout`, `ribbon`).
- **ImportOptions**: User-selected behavior for screenshot inclusion/locking, naming mode, depth pruning, visibility filtering, and smart placeholders.
- **SemanticBundle**: Versioned annotation artifact containing per-node roles, confidence/evidence, optional regions, optional patterns, and warnings.

## Interop & Compatibility *(mandatory when contracts/schemas are touched)*

- **IC-001**: This feature targets schema-compatible implementation for existing `1.x` contracts and is classified as **MINOR/PATCH-compatible** with no planned MAJOR contract break in this increment.
- **IC-002**: Producer and consumer MUST accept same MAJOR and higher MINOR versions within `1.x`, ignore unknown optional fields, and safely handle unknown DevExpress kinds.
- **IC-003**: Any future contract shape change to required fields, discriminator semantics, or coordinate semantics MUST update both schema files, interop docs, and fixture bundles in the same release scope.

## Safety & Privacy *(mandatory)*

- **SP-001**: Export, import, and semantic workflows MUST degrade to partial output with warnings on recoverable failures and MUST avoid uncaught exceptions that destabilize host runtime or plugin session.
- **SP-002**: Bundle and semantic processing MUST remain local-only by default with no external upload/transmission path.
- **SP-003**: Screenshot and metadata outputs are treated as potentially sensitive artifacts; generation is explicit, stored in configured local paths, and never auto-shared.

## Success Criteria *(mandatory)*

### Measurable Outcomes
- **SC-001**: In fixture-based validation, 100% of valid bundles produce structural outputs that satisfy required schema fields.
- **SC-002**: Repeating export/import with identical inputs and options yields identical layer naming and hierarchy in at least 99% of deterministic comparison runs.
- **SC-003**: For supported DevExpress fixture screens, at least 90% of expected smart-placeholder structures are generated without manual post-fix.
- **SC-004**: For large fixture bundles (up to 2,000 nodes), import completes without plugin crash and remains interactive throughout processing.
- **SC-005**: On induced screenshot or metadata extraction failures, 100% of runs still produce usable primary artifacts (`form.json` and/or Figma frame) with warnings.
- **SC-006**: Semantic transformation produces valid annotation output for at least 95% of fixtures and includes confidence/evidence for each emitted role.

## Assumptions

- Existing attached specs and schemas are authoritative for v1 scope and terminology.
- Export/import/semantic components may ship incrementally but must preserve interop contract behavior from first release.
- MVP acceptance allows warning-based best-effort behavior for optional metadata and screenshot paths.

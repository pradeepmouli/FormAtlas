# Contract: Semantic Bundle (`semantic.json`)

## Purpose
Defines semantic transformer output consumed by modernization and analysis workflows.

## Source of truth
- `docs/semantic.schema.json`
- `docs/spec-semantic-layer-architecture.md`

## Required root fields
- `semanticVersion: string`
- `sourceSchemaVersion: string`
- `form: object`
- `annotations: Annotation[]`

## Optional fields
- `regions: Region[]`
- `patterns: Pattern[]`
- `warnings: string[]`

## Annotation requirements
- Every annotation must include:
  - `nodeId`
  - at least one role entry in `roles`
- Every role entry must include:
  - `role`
  - `confidence` (0..1)
- `evidence[]` should provide traceable heuristic/type basis.

## Compatibility rules
- `sourceSchemaVersion` must map to supported exporter schema family.
- Consumers tolerate optional additions in `hints`, `tags`, and root metadata.

## Determinism rules
- Role assignment ordering and confidence generation should remain stable for identical inputs/options.

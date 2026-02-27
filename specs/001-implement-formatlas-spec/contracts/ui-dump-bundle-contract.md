# Contract: UiDump Bundle (`form.json`)

## Purpose
Defines the producer-to-consumer bundle contract between runtime exporter and downstream tools.

## Source of truth
- `docs/ui-dump.schema.json`
- `docs/spec-interop-contract.md`

## Required root fields
- `schemaVersion` (string, MAJOR.MINOR)
- `form` (object)
- `nodes` (array)

## Optional root fields
- `screenshot` (string|null)
- `errors` (array)

## Compatibility rules
- Consumers accept same MAJOR version.
- Consumers accept higher MINOR versions within same MAJOR.
- Consumers reject higher MAJOR by default unless explicit best-effort mode.
- Unknown optional properties must be ignored.

## DevExpress metadata rules
- Optional metadata at `metadata.devexpress`.
- `kind` is required when `devexpress` is present.
- Unknown `kind` values must not fail import/semantic processing.

## Determinism rules
- Node traversal and sibling ordering must be stable for identical input state.
- Bounds are parent-relative and must not be normalized silently.

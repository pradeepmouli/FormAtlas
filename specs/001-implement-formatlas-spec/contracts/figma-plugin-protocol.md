# Contract: Figma Importer UI↔Worker Protocol

## Purpose
Defines typed message exchange between plugin UI context and worker context.

## Source of truth
- `docs/spec-figma-plugin-architecture.md`

## UI → Worker
### `VALIDATE_REQUEST`
- `jsonText: string`

### `IMPORT_REQUEST`
- `jsonText: string`
- `pngBytes?: Uint8Array`
- `options: ImportOptions`

## Worker → UI
### `VALIDATE_RESULT`
- `ok: boolean`
- `schemaVersion?: string`
- `form?: { name: string; width: number; height: number }`
- `counts?: { nodes: number; devexpressKinds: string[] | number }`
- `warnings: Warning[]`

### `IMPORT_PROGRESS`
- `phase: string`
- `done: number`
- `total: number`

### `IMPORT_RESULT`
- `ok: boolean`
- `createdNodeIds?: string[]`
- `warnings: Warning[]`
- `error?: string`

## Behavior guarantees
- Invalid required structure returns `ok=false` with error/warnings.
- Unknown optional fields and unknown DevExpress kinds are warning-only.
- Progress events must be monotonic for long-running imports.

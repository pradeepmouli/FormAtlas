# Implementation Plan: FormAtlas Runtime-to-Design Pipeline

**Branch**: `001-implement-formatlas-spec` | **Date**: 2026-02-27 | **Spec**: [/specs/001-implement-formatlas-spec/spec.md](/specs/001-implement-formatlas-spec/spec.md)
**Input**: Feature specification from `/specs/001-implement-formatlas-spec/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Implement a deterministic, safe, local-only runtime-to-design pipeline: export WinForms UI bundles, import them into Figma with smart DevExpress placeholders, and generate semantic annotations for modernization workflows while preserving schema compatibility guarantees across producer/consumer components.

## Technical Context

**Language/Version**: C# (.NET `net478` + `netstandard2.0`) for agent/core; TypeScript (ES2020+) for Figma importer; JSON Schema Draft 2020-12 for contracts  
**Primary Dependencies**: `System.Windows.Forms`, `Microsoft.NETFramework.ReferenceAssemblies` (net478), reflection APIs, Figma Plugin API, JSON serializer (Newtonsoft.Json preferred for net478 compatibility), TypeScript toolchain (Vite or equivalent)  
**Storage**: Local filesystem bundles (`form.json`, optional `form.png`, `semantic.json`)  
**Testing**: .NET unit tests for traversal/serialization/adapter safety; TypeScript unit tests for parser/normalizer/renderers; fixture-based golden comparisons  
**Target Platform**: Windows runtime for WinForms export, Figma Desktop/Web plugin runtime for import, local developer machines for semantic transformer
**Project Type**: Multi-component tooling (library/agent + plugin + transformer)
**Performance Goals**: Runtime export avoids blocking UI beyond 250ms on typical forms; Figma import remains responsive up to ~2,000 nodes; semantic pass remains linear in node count
**Constraints**: Local-only processing, deterministic ordering/naming, schema-version compatibility rules, no host crash on partial failures, optional DevExpress support via reflection only
**Scale/Scope**: Single repository planning artifacts covering three delivery tracks (exporter, importer, semantic layer) with fixture-driven validation across representative forms

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- ✅ Runtime safety: plan mandates guarded traversal, screenshot fallback, reflection try/catch boundaries, and warning-first degradation behavior.
- ✅ Contract compatibility: plan preserves `schemaVersion` MAJOR/MINOR semantics and treats schema/docs as first-class versioned interfaces.
- ✅ Determinism/non-destructive output: ordering, naming, coordinate, and z-order invariants are explicit for exporter/importer/semantic components.
- ✅ Test gate: scoped unit/integration/golden checks are defined for all contract-sensitive behaviors.
- ✅ Privacy/security: all processing remains local-only and artifacts are handled as potentially sensitive.

Post-design re-check (Phase 1): PASS. No constitution violations identified.

## Project Structure

### Documentation (this feature)

```text
specs/001-implement-formatlas-spec/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
└── FormAtlas.Tool/
└── FormAtlas.Tool.SampleHost/

docs/
├── spec-ui-export-structure.md
├── spec-figma-importer.md
├── spec-figma-plugin-architecture.md
├── spec-interop-contract.md
├── spec-semantic-layer-architecture.md
├── ui-dump.schema.json
└── semantic.schema.json

specs/001-implement-formatlas-spec/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
└── contracts/

tools/figma-importer/        # planned in this feature scope
semantic/                    # planned transformer scope
tests/                       # planned test scope for new components
```

**Structure Decision**: Single-repo, multi-component structure extending current `src/FormAtlas.Tool` foundation with planned `tools/figma-importer`, `semantic`, and `tests` tracks, while treating `docs/*.md` + schema files as contract-authoritative artifacts.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | N/A |

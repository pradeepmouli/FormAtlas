# Research — FormAtlas Runtime-to-Design Pipeline

## Decision 1: Keep exporter implementation in C# targeting `net478` + `netstandard2.0`
- Decision: Implement runtime capture and bundle serialization in the existing C# project and preserve dual targeting where possible.
- Rationale: WinForms runtime capture requires .NET Framework compatibility (`net478`) while shared logic can remain reusable through `netstandard2.0`.
- Alternatives considered:
  - Separate `net48`-only project: rejected due to reduced reuse and extra maintenance overhead.
  - Full migration to newer .NET only: rejected because WinForms host integration requirements center on .NET Framework workloads.

## Decision 2: Use reflection-only DevExpress adapters
- Decision: Detect and enrich DevExpress controls by full-name/base-chain reflection with hard exception guards.
- Rationale: Meets optional integration goal without compile-time dependency and prevents runtime failures on non-DevExpress hosts.
- Alternatives considered:
  - Direct DevExpress package references: rejected due to coupling and deployment complexity.
  - No metadata enrichment: rejected because it degrades importer/semantic value for key enterprise screens.

## Decision 3: Treat JSON schema files and interop docs as contract authority
- Decision: Use `docs/ui-dump.schema.json`, `docs/semantic.schema.json`, and `docs/spec-interop-contract.md` as compatibility source of truth.
- Rationale: Constitution requires contract-driven interoperability and synchronized versioned artifacts.
- Alternatives considered:
  - Implicit code-first contract only: rejected due to drift risk across exporter/importer/semantic tools.

## Decision 4: Implement Figma importer as TypeScript plugin with deterministic render pipeline
- Decision: Use a typed protocol + normalization + renderer registry architecture aligned with attached plugin architecture spec.
- Rationale: Supports deterministic output, modular devexpress handlers, and performance options for large node trees.
- Alternatives considered:
  - Monolithic plugin logic: rejected due to poor extensibility and testability.
  - Non-typed JS-only protocol: rejected due to higher integration error risk.

## Decision 5: Use fixture-based golden checks for deterministic behavior
- Decision: Validate exporter/importer/semantic outputs using representative fixture bundles and deterministic comparisons.
- Rationale: Determinism is a constitution-level gate and cannot be proven by unit tests alone.
- Alternatives considered:
  - Pure unit test strategy: rejected because cross-component ordering/naming/hierarchy drift may be missed.

## Decision 6: Keep processing local-only by default
- Decision: All pipeline stages operate strictly on local files without network transfer.
- Rationale: Required by constitution privacy principle and sensitive screenshot handling expectations.
- Alternatives considered:
  - Optional telemetry/upload in MVP: rejected due to privacy/security scope expansion.

## Decision 7: Semantic transformer remains rule-based in v1
- Decision: Implement deterministic type/text/layout heuristics with confidence + evidence output.
- Rationale: Meets v1 goals with predictable behavior and traceability.
- Alternatives considered:
  - ML-first inference: rejected for MVP due to data/training complexity and non-determinism risk.
  - Pure type mapping only: rejected due to weak role quality for action semantics and layout regions.

## Decision 8: Unresolved clarifications
- Decision: None; all technical context required for planning is sufficiently specified by current spec + attached architecture docs.
- Rationale: No blocking unknowns remain for Phase 1 design outputs.
- Alternatives considered:
  - Deferring stack/contract decisions to implementation: rejected because it would weaken task decomposition and compliance gating.

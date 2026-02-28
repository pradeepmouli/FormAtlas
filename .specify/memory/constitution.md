<!--
Sync Impact Report
- Version change: N/A (template) → 1.0.0
- Modified principles:
	- Template Principle 1 → I. Runtime Safety First
	- Template Principle 2 → II. Contract-Driven Interoperability
	- Template Principle 3 → III. Deterministic, Non-Destructive Output
	- Template Principle 4 → IV. Test-Gated Delivery
	- Template Principle 5 → V. Local-Only Privacy and Secure Handling
- Added sections:
	- Platform & Architecture Constraints
	- Development Workflow & Quality Gates
- Removed sections:
	- None
- Templates requiring updates:
	- ✅ updated: .specify/templates/plan-template.md
	- ✅ updated: .specify/templates/spec-template.md
	- ✅ updated: .specify/templates/tasks-template.md
	- ✅ not applicable (folder missing): .specify/templates/commands/*.md
- Follow-up TODOs:
	- None
-->

# FormAtlas Constitution

## Core Principles

### I. Runtime Safety First
All runtime capture and import flows MUST fail safely and MUST NOT crash the host
application, plugin session, or build pipeline. Error-prone integrations (Win32 capture,
reflection-based metadata, schema parsing) MUST use guarded execution and degrade to
partial output with explicit warnings. This principle exists because this project attaches
to live WinForms applications where stability is non-negotiable.

### II. Contract-Driven Interoperability
All producer/consumer behavior MUST follow versioned JSON contracts and documented
compatibility rules. `schemaVersion` MUST be present, compatibility decisions MUST be
deterministic by MAJOR/MINOR semantics, and unknown optional fields MUST be tolerated.
Any breaking contract change MUST include schema updates, migration notes, and a MAJOR
version increment. This ensures durable interoperability between UiDumpAgent,
semantic-layer tooling, and Figma import pipelines.

### III. Deterministic, Non-Destructive Output
Given identical inputs and options, exporters/importers MUST produce deterministic
structure, naming, and ordering. Tools MUST preserve coordinate and z-order semantics and
MUST NOT mutate existing design artifacts unless a user-selected insertion target allows
it. Determinism and non-destructive behavior are required to support repeatable design
iteration, diffing, and auditability.

### IV. Test-Gated Delivery
Changes to traversal, schema routing, DevExpress enrichment, coordinate mapping, and
import rendering MUST be verified by automated tests at the smallest useful scope first,
then by integration or golden checks where behavior spans modules. Releases MUST be
blocked when these required checks fail. This principle protects contract stability and
prevents regressions in complex UI trees.

### V. Local-Only Privacy and Secure Handling
The tooling MUST process bundles locally and MUST NOT transmit UI dumps, screenshots, or
derived semantic artifacts over the network by default. File access MUST stay within
user-configured output/import paths. Security-sensitive behavior (screenshot capture and
metadata extraction) MUST be explicit and documented. This protects potentially sensitive
application data.

## Platform & Architecture Constraints

- Source projects MUST maintain compatibility with `netstandard2.0` and `net48` unless
	an approved amendment changes supported targets.
- Runtime capture components MUST remain dependency-light and MUST avoid hard compile-time
	coupling to optional vendor assemblies such as DevExpress.
- All schema artifacts (`ui-dump.schema.json`, `semantic.schema.json`, and interop docs)
	MUST be treated as first-class versioned interfaces and updated together when changed.
- Performance controls for large trees (depth limits, pruning, or collapse policies) MUST
	be configurable and documented.

## Development Workflow & Quality Gates

1. Every spec/plan/tasks artifact MUST include a constitution check that explicitly covers
	 safety, contract compatibility, determinism, testing, and privacy.
2. Every pull request MUST identify whether it changes exporter contract,
	 importer rendering, semantic inference, or documentation only.
3. Contract-affecting changes MUST include:
	 - updated schema/docs,
	 - compatibility statement,
	 - fixture updates and deterministic test updates.
4. Releases MUST use semantic versioning and MUST align version bump rationale with
	 compatibility impact.

## Governance

This constitution is the highest-priority engineering policy for this repository and
supersedes conflicting local conventions.

Amendment procedure:
- Propose changes via pull request including rationale, impact analysis, and migration
	notes for any affected specs/templates.
- Obtain approval from maintainers responsible for exporter/importer interoperability.
- Update the Sync Impact Report in this file as part of the same change.

Versioning policy for this constitution:
- MAJOR: Principle removals/redefinitions or governance changes that invalidate prior
	compliance expectations.
- MINOR: New principle/section or materially expanded mandatory guidance.
- PATCH: Clarifications, wording improvements, and non-semantic edits.

Compliance review expectations:
- Plan/spec/tasks outputs MUST pass constitution gates before implementation begins.
- Code review MUST reject changes that violate non-negotiable principles unless the
	constitution is amended in the same review scope.
- At minimum, compliance is re-checked at planning, PR review, and release preparation.

**Version**: 1.0.0 | **Ratified**: 2026-02-27 | **Last Amended**: 2026-02-27

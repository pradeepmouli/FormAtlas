# Tasks: FormAtlas Runtime-to-Design Pipeline

**Input**: Design documents from `/specs/001-implement-formatlas-spec/`
**Prerequisites**: `plan.md` (required), `spec.md` (required), `research.md`, `data-model.md`, `contracts/`

## Constitution Check (Gate)

*GATE: Must remain PASS before implementation and at story completion checkpoints.*

- ✅ Runtime safety: tasks include guarded traversal, reflection safety tests, screenshot fallback, and warning-oriented degradation paths.
- ✅ Contract compatibility: tasks include schema validation, version policy, compatibility tests, and interop documentation updates.
- ✅ Deterministic/non-destructive output: tasks include deterministic ordering tests/helpers for exporter and importer plus stable fixture validation.
- ✅ Test-gated delivery: each user story includes automated test tasks before implementation tasks.
- ✅ Local-only privacy/security: tasks are constrained to local bundle/file workflows and do not introduce network transmission behavior.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create baseline project structure for exporter, importer, semantic transformer, and test harnesses.

- [ ] T001 Create feature source folders in `src/FormAtlas.Tool/Agent/`, `src/FormAtlas.Tool/Exporter/`, and `src/FormAtlas.Tool/Metadata/`
- [ ] T002 [P] Scaffold Figma importer package manifest in `tools/figma-importer/package.json`
- [ ] T003 [P] Add TypeScript build config for importer in `tools/figma-importer/tsconfig.json`
- [ ] T004 [P] Add importer bundler config in `tools/figma-importer/vite.config.ts`
- [ ] T005 Create importer source/test folders under `tools/figma-importer/src/` and `tools/figma-importer/tests/`
- [ ] T006 Create semantic transformer project file in `semantic/FormAtlas.Semantic/FormAtlas.Semantic.csproj`
- [ ] T007 [P] Create semantic transformer source/test folders in `semantic/FormAtlas.Semantic/` and `tests/FormAtlas.Semantic.Tests/`
- [ ] T008 Add exporter test project file in `tests/FormAtlas.Tool.Tests/FormAtlas.Tool.Tests.csproj`
- [ ] T009 [P] Add fixture directories for cross-component tests in `tests/fixtures/ui-dump/`, `tools/figma-importer/fixtures/`, and `tests/fixtures/semantic/`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Implement shared contract, validation, and determinism infrastructure required by all stories.

**⚠️ CRITICAL**: No user story implementation starts before this phase completes.

- [ ] T010 Implement shared bundle contract models in `src/FormAtlas.Tool/Contracts/UiDumpBundleModels.cs`
- [ ] T011 [P] Implement semantic bundle contract models in `semantic/FormAtlas.Semantic/Contracts/SemanticBundleModels.cs`
- [ ] T012 [P] Add schema loading/validation utility for exporter in `src/FormAtlas.Tool/Validation/SchemaValidator.cs`
- [ ] T013 [P] Add schema loading/validation utility for semantic transformer in `semantic/FormAtlas.Semantic/Validation/SemanticSchemaValidator.cs`
- [ ] T014 Implement deterministic ordering and ID sequencing helpers in `src/FormAtlas.Tool/Core/DeterministicOrdering.cs`
- [ ] T015 [P] Implement warning/error envelope model shared by pipelines in `src/FormAtlas.Tool/Core/PipelineWarnings.cs`
- [ ] T016 [P] Implement interop version compatibility policy in `src/FormAtlas.Tool/Contracts/SchemaVersionPolicy.cs`
- [ ] T017 Implement exporter fixture schema-validation tests in `tests/FormAtlas.Tool.Tests/Contract/UiDumpSchemaContractTests.cs`
- [ ] T018 [P] Implement semantic schema-validation tests in `tests/FormAtlas.Semantic.Tests/Contract/SemanticSchemaContractTests.cs`

**Checkpoint**: Contract validation, version policy, and determinism primitives are ready.

---

## Phase 3: User Story 1 - Export Runtime UI Bundles (Priority: P1) 🎯 MVP

**Goal**: Produce deterministic, schema-valid WinForms runtime bundles with optional screenshot and reflection-based DevExpress metadata.

**Independent Test**: Trigger export in a sample WinForms host and verify timestamped `form.json` (+ optional `form.png`) is created without host instability.

### Tests for User Story 1

- [ ] T019 [P] [US1] Add idempotent lifecycle tests for agent start/stop in `tests/FormAtlas.Tool.Tests/Agent/UiDumpAgentLifecycleTests.cs`
- [ ] T020 [P] [US1] Add deterministic traversal and bounds tests in `tests/FormAtlas.Tool.Tests/Exporter/ControlWalkerTests.cs`
- [ ] T021 [P] [US1] Add screenshot fallback tests in `tests/FormAtlas.Tool.Tests/Exporter/ScreenshotCaptureFallbackTests.cs`
- [ ] T022 [P] [US1] Add reflection safety tests for missing DevExpress assemblies in `tests/FormAtlas.Tool.Tests/Metadata/DevExpressReflectionSafetyTests.cs`
- [ ] T023 [US1] Add bundle compatibility tests against `1.x` rules in `tests/FormAtlas.Tool.Tests/Contract/InteropCompatibilityTests.cs`

### Implementation for User Story 1

- [ ] T024 [US1] Implement `UiDumpOptions` contract in `src/FormAtlas.Tool/Agent/UiDumpOptions.cs`
- [ ] T025 [US1] Implement idempotent `UiDumpAgent.Start/Stop` in `src/FormAtlas.Tool/Agent/UiDumpAgent.cs`
- [ ] T026 [US1] Implement dump coordination and form selection in `src/FormAtlas.Tool/Exporter/DumpCoordinator.cs`
- [ ] T027 [US1] Implement recursive WinForms control traversal in `src/FormAtlas.Tool/Exporter/ControlWalker.cs`
- [ ] T028 [US1] Implement node mapping for required/optional fields in `src/FormAtlas.Tool/Exporter/UiNodeMapper.cs`
- [ ] T029 [US1] Implement screenshot capture service with non-fatal fallback in `src/FormAtlas.Tool/Exporter/ScreenshotCaptureService.cs`
- [ ] T030 [US1] Implement reflection helper abstractions in `src/FormAtlas.Tool/Metadata/DevExpressReflection.cs`
- [ ] T031 [P] [US1] Implement GridControl metadata adapter in `src/FormAtlas.Tool/Metadata/Adapters/GridControlAdapter.cs`
- [ ] T032 [P] [US1] Implement PivotGridControl metadata adapter in `src/FormAtlas.Tool/Metadata/Adapters/PivotGridControlAdapter.cs`
- [ ] T033 [P] [US1] Implement XtraTabControl metadata adapter in `src/FormAtlas.Tool/Metadata/Adapters/XtraTabControlAdapter.cs`
- [ ] T034 [P] [US1] Implement LayoutControl metadata adapter in `src/FormAtlas.Tool/Metadata/Adapters/LayoutControlAdapter.cs`
- [ ] T035 [P] [US1] Implement Ribbon/BarManager metadata adapter in `src/FormAtlas.Tool/Metadata/Adapters/RibbonBarAdapter.cs`
- [ ] T036 [US1] Implement metadata adapter registry and guarded execution in `src/FormAtlas.Tool/Metadata/AdapterRegistry.cs`
- [ ] T037 [US1] Implement bundle writer and schemaVersion emission in `src/FormAtlas.Tool/Exporter/UiDumpBundleWriter.cs`
- [ ] T038 [US1] Add sample host integration for export trigger in `src/FormAtlas.Tool.SampleHost/Program.cs`

**Checkpoint**: User Story 1 is independently functional and testable.

---

## Phase 4: User Story 2 - Import Bundles into Editable Figma Layers (Priority: P2)

**Goal**: Import `form.json` (+ optional screenshot) into Figma as deterministic layer hierarchy with smart DevExpress placeholders.

**Independent Test**: Import fixture bundles and verify frame sizing, screenshot locking behavior, hierarchy, and deterministic naming/order.

### Tests for User Story 2

- [ ] T039 [P] [US2] Add protocol validation tests for UI↔worker messages in `tools/figma-importer/tests/protocol.test.ts`
- [ ] T040 [P] [US2] Add bundle parser/schema routing tests in `tools/figma-importer/tests/parser.test.ts`
- [ ] T041 [P] [US2] Add node normalization and deterministic ordering tests in `tools/figma-importer/tests/normalize.test.ts`
- [ ] T042 [P] [US2] Add DevExpress renderer tests in `tools/figma-importer/tests/render-devexpress.test.ts`
- [ ] T043 [US2] Add large-node performance options tests in `tools/figma-importer/tests/performance-options.test.ts`

### Implementation for User Story 2

- [ ] T044 [US2] Implement typed UI↔worker protocol definitions in `tools/figma-importer/src/protocol.ts`
- [ ] T045 [US2] Implement importer option models and defaults in `tools/figma-importer/src/import/options.ts`
- [ ] T046 [US2] Implement bundle domain types and schema-version router in `tools/figma-importer/src/domain/types.ts` and `tools/figma-importer/src/domain/schema.ts`
- [ ] T047 [US2] Implement deterministic normalization pipeline in `tools/figma-importer/src/domain/normalize.ts`
- [ ] T048 [US2] Implement plugin worker entry flow in `tools/figma-importer/src/main.ts`
- [ ] T049 [US2] Implement plugin UI entry and request wiring in `tools/figma-importer/src/ui.ts`
- [ ] T050 [US2] Implement top-level importer orchestration in `tools/figma-importer/src/import/importer.ts`
- [ ] T051 [US2] Implement screenshot ingestion/locking behavior in `tools/figma-importer/src/import/screenshot.ts`
- [ ] T052 [US2] Implement generic node rendering primitives in `tools/figma-importer/src/render/primitives.ts` and `tools/figma-importer/src/render/renderNode.ts`
- [ ] T053 [US2] Implement layer naming and z-order policy in `tools/figma-importer/src/import/layerNaming.ts` and `tools/figma-importer/src/import/zOrder.ts`
- [ ] T054 [US2] Implement DevExpress renderer registry in `tools/figma-importer/src/render/devexpress/registry.ts`
- [ ] T055 [P] [US2] Implement GridControl smart renderer in `tools/figma-importer/src/render/devexpress/grid.ts`
- [ ] T056 [P] [US2] Implement PivotGridControl smart renderer in `tools/figma-importer/src/render/devexpress/pivot.ts`
- [ ] T057 [P] [US2] Implement XtraTabControl smart renderer in `tools/figma-importer/src/render/devexpress/tabs.ts`
- [ ] T058 [P] [US2] Implement LayoutControl smart renderer in `tools/figma-importer/src/render/devexpress/layout.ts`
- [ ] T059 [P] [US2] Implement Ribbon/BarManager smart renderer in `tools/figma-importer/src/render/devexpress/ribbon.ts`
- [ ] T060 [US2] Implement performance budget/pruning controls in `tools/figma-importer/src/perf/budget.ts`

**Checkpoint**: User Story 2 is independently functional and testable.

---

## Phase 5: User Story 3 - Generate Semantic Bundles for Modernization (Priority: P3)

**Goal**: Produce deterministic semantic annotations (`semantic.json`) from exported bundles with confidence/evidence traces.

**Independent Test**: Run semantic transformer against fixtures and verify schema-valid output plus expected role assignments.

### Tests for User Story 3

- [ ] T061 [P] [US3] Add semantic pipeline normalization tests in `tests/FormAtlas.Semantic.Tests/Normalization/NormalizationTests.cs`
- [ ] T062 [P] [US3] Add role inference tests for WinForms and DevExpress types in `tests/FormAtlas.Semantic.Tests/Inference/RoleInferenceTests.cs`
- [ ] T063 [P] [US3] Add confidence/evidence traceability tests in `tests/FormAtlas.Semantic.Tests/Inference/ConfidenceEvidenceTests.cs`
- [ ] T064 [US3] Add region/pattern detection tests in `tests/FormAtlas.Semantic.Tests/Inference/PatternDetectionTests.cs`

### Implementation for User Story 3

- [ ] T065 [US3] Implement semantic transformer CLI entry in `semantic/FormAtlas.Semantic/Program.cs`
- [ ] T066 [US3] Implement source bundle reader and validation in `semantic/FormAtlas.Semantic/IO/UiDumpBundleReader.cs`
- [ ] T067 [US3] Implement absolute-bounds and feature normalization in `semantic/FormAtlas.Semantic/Normalization/FeatureNormalizer.cs`
- [ ] T068 [US3] Implement type-based role classification in `semantic/FormAtlas.Semantic/Inference/TypeRoleClassifier.cs`
- [ ] T069 [US3] Implement text/layout heuristics and confidence scoring in `semantic/FormAtlas.Semantic/Inference/HeuristicRoleScorer.cs`
- [ ] T070 [US3] Implement region and pattern detection in `semantic/FormAtlas.Semantic/Inference/RegionPatternDetector.cs`
- [ ] T071 [US3] Implement semantic bundle writer in `semantic/FormAtlas.Semantic/IO/SemanticBundleWriter.cs`

**Checkpoint**: User Story 3 is independently functional and testable.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final contract hardening, documentation alignment, and end-to-end validation.

- [ ] T072 [P] Add end-to-end fixture pipeline validation tests in `tests/Integration/FormAtlasPipelineIntegrationTests.cs`
- [ ] T073 [P] Update runtime/export usage docs in `README.md` and `docs/spec-ui-export-structure.md`
- [ ] T074 [P] Update importer usage and protocol docs in `docs/spec-figma-importer.md` and `docs/spec-figma-plugin-architecture.md`
- [ ] T075 [P] Update semantic and interop docs in `docs/spec-semantic-layer-architecture.md` and `docs/spec-interop-contract.md`
- [ ] T076 Run quickstart validation scenarios from `specs/001-implement-formatlas-spec/quickstart.md` and capture results in `specs/001-implement-formatlas-spec/quickstart-validation.md`
- [ ] T077 [P] Add local-only/network-prohibition tests for exporter, importer, and semantic tooling in `tests/Integration/LocalOnlyProcessingTests.cs` and `tools/figma-importer/tests/local-only.test.ts`
- [ ] T078 [P] Add sensitive artifact handling tests and path-boundary validation in `tests/FormAtlas.Tool.Tests/Security/SensitiveArtifactHandlingTests.cs`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: starts immediately.
- **Phase 2 (Foundational)**: depends on Phase 1; blocks all user stories.
- **Phase 3 (US1)**: depends on Phase 2 and delivers MVP.
- **Phase 4 (US2)**: depends on Phase 2 and consumes exporter contracts/fixtures from US1 outputs.
- **Phase 5 (US3)**: depends on Phase 2 and consumes exporter contracts/fixtures from US1 outputs.
- **Phase 6 (Polish)**: depends on completion of selected user stories.

### User Story Dependencies

- **US1 (P1)**: no dependency on other stories; foundational producer.
- **US2 (P2)**: depends on US1 bundle fixtures and compatibility behavior.
- **US3 (P3)**: depends on US1 bundle fixtures and compatibility behavior.

### Within Each User Story

- Tests first (write and fail before implementation).
- Contract/domain models before orchestration.
- Core implementation before integration/hardening.

## Parallel Opportunities

- Phase 1 tasks marked `[P]` can run in parallel.
- Phase 2 validation/model tasks marked `[P]` can run in parallel.
- In US1, metadata adapters `T031`–`T035` can run in parallel after reflection helper `T030`.
- In US2, smart renderers `T055`–`T059` can run in parallel after registry `T054`.
- In US3, inference test tasks `T061`–`T064` can run in parallel.

## Parallel Example: User Story 1

- `T031`, `T032`, `T033`, `T034`, `T035` in parallel after `T030`.

## Parallel Example: User Story 2

- `T055`, `T056`, `T057`, `T058`, `T059` in parallel after `T054`.

## Parallel Example: User Story 3

- `T061`, `T062`, `T063`, `T064` in parallel while inference components are being scaffolded.

## Implementation Strategy

### MVP First (US1 only)

1. Complete Phase 1 and Phase 2.
2. Complete Phase 3 (US1).
3. Validate runtime export independently before advancing.

### Incremental Delivery

1. Deliver US1 (exporter + contracts).
2. Deliver US2 (Figma importer) using stable exporter fixtures.
3. Deliver US3 (semantic transformer) using same fixture baseline.
4. Complete polish and end-to-end validation.

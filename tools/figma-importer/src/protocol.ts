/**
 * Typed UI↔Worker protocol definitions for the FormAtlas Figma importer plugin.
 * UI context sends requests; worker context sends results and progress.
 */

// ---- Shared types ----

export interface Warning {
  code: string;
  message: string;
}

export interface ImportOptions {
  includeScreenshot: boolean;
  lockScreenshot: boolean;
  renderAllPlaceholders: boolean;
  maxDepth?: number;
  smartDevExpress: boolean;
  namingMode: "name" | "type(name)";
  skipInvisible?: boolean;
  minSize?: number;
}

export const DEFAULT_IMPORT_OPTIONS: ImportOptions = {
  includeScreenshot: true,
  lockScreenshot: true,
  renderAllPlaceholders: true,
  smartDevExpress: true,
  namingMode: "name",
  skipInvisible: false,
  minSize: 0,
};

// ---- UI → Worker messages ----

export interface ValidateRequest {
  type: "VALIDATE_REQUEST";
  jsonText: string;
}

export interface ImportRequest {
  type: "IMPORT_REQUEST";
  jsonText: string;
  pngBytes?: Uint8Array;
  options: ImportOptions;
}

export type UiToWorkerMessage = ValidateRequest | ImportRequest;

// ---- Worker → UI messages ----

export interface ValidateResult {
  type: "VALIDATE_RESULT";
  ok: boolean;
  schemaVersion?: string;
  form?: { name: string; width: number; height: number };
  counts?: { nodes: number; devexpressKinds: string[] | number };
  warnings: Warning[];
  error?: string;
}

export interface ImportProgress {
  type: "IMPORT_PROGRESS";
  phase: string;
  done: number;
  total: number;
}

export interface ImportResult {
  type: "IMPORT_RESULT";
  ok: boolean;
  createdNodeIds?: string[];
  warnings: Warning[];
  error?: string;
}

export type WorkerToUiMessage = ValidateResult | ImportProgress | ImportResult;

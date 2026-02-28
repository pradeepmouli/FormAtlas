/**
 * FormAtlas Figma Plugin — Worker (main.ts) entry point.
 * Handles messages from the plugin UI and executes import/validate operations.
 */

import type { UiToWorkerMessage, WorkerToUiMessage, ValidateResult, ImportResult } from "./protocol";
import { parseBundle, isCompatible, countNodes } from "./domain/schema";
import { importBundle } from "./import/importer";

figma.showUI(__html__, { width: 400, height: 500 });

figma.ui.onmessage = async (rawMsg: unknown) => {
  const msg = rawMsg as UiToWorkerMessage;

  if (msg.type === "VALIDATE_REQUEST") {
    const result = handleValidate(msg.jsonText);
    const reply: WorkerToUiMessage = result;
    figma.ui.postMessage(reply);
    return;
  }

  if (msg.type === "IMPORT_REQUEST") {
    const { jsonText, pngBytes, options } = msg;
    let bundle;
    try {
      bundle = parseBundle(jsonText);
    } catch (e) {
      const reply: ImportResult = {
        type: "IMPORT_RESULT",
        ok: false,
        warnings: [],
        error: (e as Error).message,
      };
      figma.ui.postMessage(reply);
      return;
    }

    const summary = await importBundle(bundle, pngBytes, options, (phase, done, total) => {
      figma.ui.postMessage({ type: "IMPORT_PROGRESS", phase, done, total });
    });

    const reply: ImportResult = {
      type: "IMPORT_RESULT",
      ok: summary.ok,
      createdNodeIds: summary.createdNodeIds,
      warnings: summary.warnings,
      ...(summary.error !== undefined && { error: summary.error }),
    };
    figma.ui.postMessage(reply);
    return;
  }
};

function handleValidate(jsonText: string): ValidateResult {
  try {
    const bundle = parseBundle(jsonText);

    if (!isCompatible(bundle.schemaVersion)) {
      return {
        type: "VALIDATE_RESULT",
        ok: false,
        warnings: [],
        error: `Incompatible schemaVersion: ${bundle.schemaVersion}`,
      };
    }

    const { total, devexpressKinds } = countNodes(bundle.nodes);
    return {
      type: "VALIDATE_RESULT",
      ok: true,
      schemaVersion: bundle.schemaVersion,
      form: {
        name: bundle.form.name,
        width: bundle.form.width,
        height: bundle.form.height,
      },
      counts: { nodes: total, devexpressKinds },
      warnings: [],
    };
  } catch (e) {
    return {
      type: "VALIDATE_RESULT",
      ok: false,
      warnings: [],
      error: (e as Error).message,
    };
  }
}

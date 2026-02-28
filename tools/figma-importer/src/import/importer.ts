/**
 * Top-level importer orchestration for the FormAtlas Figma plugin.
 */

import type { UiDumpBundle } from "../domain/types";
import type { ImportOptions, Warning } from "../protocol";
import { normalizeNodes } from "../domain/normalize";
import { prune } from "../perf/budget";
import { renderNode } from "../render/renderNode";
import { insertScreenshot } from "./screenshot";
import { getFormFrameName, UI_LAYERS_GROUP_NAME } from "./layerNaming";
import { sortByZOrder } from "./zOrder";

export interface ImportSummary {
  ok: boolean;
  createdNodeIds: string[];
  warnings: Warning[];
  error?: string;
}

export async function importBundle(
  bundle: UiDumpBundle,
  pngBytes: Uint8Array | undefined,
  options: ImportOptions,
  onProgress?: (phase: string, done: number, total: number) => void
): Promise<ImportSummary> {
  const warnings: Warning[] = [];
  const createdNodeIds: string[] = [];

  try {
    onProgress?.("ValidatingBundle", 0, 1);

    // Normalize and prune
    onProgress?.("NormalizingNodes", 0, 1);
    const normalized = normalizeNodes(bundle.nodes);
    const pruned = prune(normalized, options);

    // Create top-level form frame
    onProgress?.("RenderingFrame", 0, 1);
    const formFrame = figma.createFrame();
    formFrame.name = getFormFrameName(bundle.form.name);
    formFrame.resize(
      Math.max(bundle.form.width, 1),
      Math.max(bundle.form.height, 1)
    );
    formFrame.x = 0;
    formFrame.y = 0;
    figma.currentPage.appendChild(formFrame);
    createdNodeIds.push(formFrame.id);

    // Insert screenshot if requested
    if (options.includeScreenshot && pngBytes && pngBytes.length > 0) {
      try {
        const ssLayer = insertScreenshot(pngBytes, formFrame, options);
        createdNodeIds.push(ssLayer.id);
      } catch (e) {
        warnings.push({
          code: "SCREENSHOT_INSERT_FAILED",
          message: `Screenshot insertion failed (non-fatal): ${(e as Error).message}`,
        });
      }
    }

    // Create UI Layers group frame
    const uiLayersFrame = figma.createFrame();
    uiLayersFrame.name = UI_LAYERS_GROUP_NAME;
    uiLayersFrame.resize(Math.max(bundle.form.width, 1), Math.max(bundle.form.height, 1));
    uiLayersFrame.x = 0;
    uiLayersFrame.y = 0;
    formFrame.appendChild(uiLayersFrame);
    createdNodeIds.push(uiLayersFrame.id);

    // Render nodes
    onProgress?.("RenderingNodes", 0, pruned.length);
    const sorted = sortByZOrder(pruned);
    let done = 0;
    for (const node of sorted) {
      const rendered = renderNode(node, uiLayersFrame, options);
      createdNodeIds.push(rendered.id);
      done++;
      onProgress?.("RenderingNodes", done, pruned.length);
    }

    onProgress?.("Finalized", 1, 1);

    return { ok: true, createdNodeIds, warnings };
  } catch (e) {
    return {
      ok: false,
      createdNodeIds,
      warnings,
      error: (e as Error).message,
    };
  }
}

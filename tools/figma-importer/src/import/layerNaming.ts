/**
 * Layer naming policy for deterministic Figma layer names.
 */

import type { UiNode } from "../domain/types";
import type { ImportOptions } from "../protocol";
import { layerName } from "../domain/normalize";

export function getLayerName(node: UiNode, options: Pick<ImportOptions, "namingMode">): string {
  return layerName(node, options.namingMode);
}

export function getFormFrameName(formName: string): string {
  return `Form: ${formName}`;
}

export const UI_LAYERS_GROUP_NAME = "UI Layers";
export const SCREENSHOT_LAYER_NAME = "Screenshot";

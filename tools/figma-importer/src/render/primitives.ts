/**
 * Generic node rendering primitives.
 * These return Figma API calls via globals (figma.createFrame, etc.).
 * Note: In unit tests, Figma globals are not available and these are not called directly.
 */

import type { UiNode } from "../domain/types";
import type { ImportOptions } from "../protocol";
import { getLayerName } from "../import/layerNaming";

/**
 * Creates a Figma frame sized to the given UiNode bounds.
 */
export function createNodeFrame(
  node: UiNode,
  options: Pick<ImportOptions, "namingMode">
): FrameNode {
  const frame = figma.createFrame();
  frame.name = getLayerName(node, options);
  frame.resize(Math.max(node.bounds.w, 1), Math.max(node.bounds.h, 1));
  frame.x = node.bounds.x;
  frame.y = node.bounds.y;
  frame.clipsContent = false;
  return frame;
}

/**
 * Creates a placeholder rectangle for a node.
 */
export function createPlaceholderRect(
  node: UiNode,
  options: Pick<ImportOptions, "namingMode">
): RectangleNode {
  const rect = figma.createRectangle();
  rect.name = getLayerName(node, options);
  rect.resize(Math.max(node.bounds.w, 1), Math.max(node.bounds.h, 1));
  rect.x = node.bounds.x;
  rect.y = node.bounds.y;
  return rect;
}

/**
 * Screenshot ingestion and locked layer insertion for the Figma importer.
 */

import type { ImportOptions } from "../protocol";
import { SCREENSHOT_LAYER_NAME } from "./layerNaming";

/**
 * Inserts a screenshot as a locked fill layer inside the given frame.
 * The screenshot is inserted as a rectangle with an image fill.
 * When lockScreenshot is true, the layer is locked to prevent accidental edits.
 */
export function insertScreenshot(
  pngBytes: Uint8Array,
  parentFrame: FrameNode,
  options: Pick<ImportOptions, "lockScreenshot">
): RectangleNode {
  const rect = figma.createRectangle();
  rect.name = SCREENSHOT_LAYER_NAME;
  rect.resize(parentFrame.width, parentFrame.height);
  rect.x = 0;
  rect.y = 0;

  const imageHash = figma.createImage(pngBytes).hash;
  rect.fills = [
    {
      type: "IMAGE",
      scaleMode: "FILL",
      imageHash,
    },
  ];

  if (options.lockScreenshot) {
    rect.locked = true;
  }

  // Insert at bottom of layer stack (index 0)
  parentFrame.insertChild(0, rect);
  return rect;
}

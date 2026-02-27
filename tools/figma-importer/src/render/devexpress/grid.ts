/**
 * Smart renderer for DevExpress GridControl.
 */

import type { UiNode } from "../../domain/types";
import type { ImportOptions } from "../../protocol";
import type { DevExpressRenderer } from "./registry";
import { getLayerName } from "../../import/layerNaming";

export class GridRenderer implements DevExpressRenderer {
  readonly kind = "GridControl";

  render(node: UiNode, options: ImportOptions): FrameNode {
    const frame = figma.createFrame();
    frame.name = getLayerName(node, options);
    frame.resize(Math.max(node.bounds.w, 1), Math.max(node.bounds.h, 1));
    frame.x = node.bounds.x;
    frame.y = node.bounds.y;

    const meta = node.metadata?.devexpress?.grid;
    const columns = meta?.columns ?? [];
    const colWidth = columns.length > 0
      ? Math.floor(node.bounds.w / columns.length)
      : 80;

    let offsetX = 0;
    for (const col of columns) {
      const colFrame = figma.createFrame();
      colFrame.name = col.caption ?? col.fieldName ?? `Col${offsetX}`;
      colFrame.resize(Math.max(col.width ?? colWidth, 1), Math.max(node.bounds.h, 1));
      colFrame.x = offsetX;
      colFrame.y = 0;
      frame.appendChild(colFrame);
      offsetX += col.width ?? colWidth;
    }

    return frame;
  }
}

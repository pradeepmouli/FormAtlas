/**
 * Smart renderer for DevExpress LayoutControl.
 */

import type { UiNode } from "../../domain/types";
import type { ImportOptions } from "../../protocol";
import type { DevExpressRenderer } from "./registry";
import { getLayerName } from "../../import/layerNaming";

export class LayoutRenderer implements DevExpressRenderer {
  readonly kind = "LayoutControl";

  render(node: UiNode, options: ImportOptions): FrameNode {
    const frame = figma.createFrame();
    frame.name = getLayerName(node, options);
    frame.resize(Math.max(node.bounds.w, 1), Math.max(node.bounds.h, 1));
    frame.x = node.bounds.x;
    frame.y = node.bounds.y;

    const meta = node.metadata?.devexpress?.layout;
    const groups = meta?.groups ?? [];

    let offsetY = 0;
    for (const group of groups) {
      const groupFrame = figma.createFrame();
      groupFrame.name = group.caption ?? "Group";
      groupFrame.resize(Math.max(node.bounds.w, 1), 30 + (group.items?.length ?? 0) * 30);
      groupFrame.x = 0;
      groupFrame.y = offsetY;
      frame.appendChild(groupFrame);
      offsetY += groupFrame.height;
    }

    return frame;
  }
}

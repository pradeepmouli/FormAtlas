/**
 * Smart renderer for DevExpress RibbonControl and BarManager.
 */

import type { UiNode } from "../../domain/types";
import type { ImportOptions } from "../../protocol";
import type { DevExpressRenderer } from "./registry";
import { getLayerName } from "../../import/layerNaming";

export class RibbonRenderer implements DevExpressRenderer {
  readonly kind = ["RibbonControl", "BarManager"] as const;

  render(node: UiNode, options: ImportOptions): FrameNode {
    const frame = figma.createFrame();
    frame.name = getLayerName(node, options);
    frame.resize(Math.max(node.bounds.w, 1), Math.max(node.bounds.h, 1));
    frame.x = node.bounds.x;
    frame.y = node.bounds.y;

    const meta = node.metadata?.devexpress?.ribbon;
    const pages = meta?.pages ?? [];
    const tabHeight = 30;
    const tabWidth = pages.length > 0 ? Math.floor(node.bounds.w / pages.length) : 100;

    let offsetX = 0;
    for (const page of pages) {
      const pageFrame = figma.createFrame();
      pageFrame.name = page.caption ?? "Page";
      pageFrame.resize(Math.max(tabWidth, 1), tabHeight);
      pageFrame.x = offsetX;
      pageFrame.y = 0;
      frame.appendChild(pageFrame);
      offsetX += tabWidth;
    }

    return frame;
  }
}

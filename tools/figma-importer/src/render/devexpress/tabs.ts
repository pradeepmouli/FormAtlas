/**
 * Smart renderer for DevExpress XtraTabControl.
 */

import type { UiNode } from "../../domain/types";
import type { ImportOptions } from "../../protocol";
import type { DevExpressRenderer } from "./registry";
import { getLayerName } from "../../import/layerNaming";

export class TabsRenderer implements DevExpressRenderer {
  readonly kind = "XtraTabControl";

  render(node: UiNode, options: ImportOptions): FrameNode {
    const frame = figma.createFrame();
    frame.name = getLayerName(node, options);
    frame.resize(Math.max(node.bounds.w, 1), Math.max(node.bounds.h, 1));
    frame.x = node.bounds.x;
    frame.y = node.bounds.y;

    const meta = node.metadata?.devexpress?.tabs;
    const pages = meta?.pages ?? [];
    const tabHeight = 30;
    const tabWidth = pages.length > 0 ? Math.floor(node.bounds.w / pages.length) : 100;

    let offsetX = 0;
    for (const page of pages) {
      const tabFrame = figma.createFrame();
      tabFrame.name = page.text ?? `Tab ${page.index}`;
      tabFrame.resize(Math.max(tabWidth, 1), tabHeight);
      tabFrame.x = offsetX;
      tabFrame.y = 0;
      frame.appendChild(tabFrame);
      offsetX += tabWidth;
    }

    return frame;
  }
}

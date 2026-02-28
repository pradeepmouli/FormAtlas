/**
 * Smart renderer for DevExpress PivotGridControl.
 */

import type { UiNode } from "../../domain/types";
import type { ImportOptions } from "../../protocol";
import type { DevExpressRenderer } from "./registry";
import { getLayerName } from "../../import/layerNaming";

export class PivotRenderer implements DevExpressRenderer {
  readonly kind = "PivotGridControl";

  render(node: UiNode, options: ImportOptions): FrameNode {
    const frame = figma.createFrame();
    frame.name = getLayerName(node, options);
    frame.resize(Math.max(node.bounds.w, 1), Math.max(node.bounds.h, 1));
    frame.x = node.bounds.x;
    frame.y = node.bounds.y;

    const meta = node.metadata?.devexpress?.pivot;
    const fields = meta?.fields ?? [];
    const areas = ["RowArea", "ColumnArea", "DataArea", "FilterArea"];

    let offsetY = 0;
    for (const area of areas) {
      const areaFields = fields.filter((f) => f.area === area);
      const areaFrame = figma.createFrame();
      areaFrame.name = area;
      areaFrame.resize(Math.max(node.bounds.w, 1), 40 + areaFields.length * 20);
      areaFrame.x = 0;
      areaFrame.y = offsetY;
      frame.appendChild(areaFrame);
      offsetY += areaFrame.height;
    }

    return frame;
  }
}

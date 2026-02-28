/**
 * Recursive node renderer — maps UiNode tree to Figma layer hierarchy.
 */

import type { UiNode } from "../domain/types";
import type { ImportOptions } from "../protocol";
import { createNodeFrame } from "./primitives";
import { DevExpressRendererRegistry } from "./devexpress/registry";
import { sortByZOrder } from "../import/zOrder";

const registry = DevExpressRendererRegistry.createDefault();

export function renderNode(
  node: UiNode,
  parent: FrameNode,
  options: ImportOptions,
  depth = 0
): SceneNode {
  // Try DevExpress smart renderer first
  if (options.smartDevExpress && node.metadata?.devexpress) {
    const renderer = registry.getRenderer(node.metadata.devexpress.kind);
    if (renderer) {
      const rendered = renderer.render(node, options);
      parent.appendChild(rendered);
      return rendered;
    }
  }

  // Generic frame rendering
  const frame = createNodeFrame(node, options);
  parent.appendChild(frame);

  const sorted = sortByZOrder(node.children ?? []);
  for (const child of sorted) {
    renderNode(child, frame, options, depth + 1);
  }

  return frame;
}

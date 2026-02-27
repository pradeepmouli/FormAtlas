/**
 * Performance budget and pruning controls for large node trees.
 */

import type { UiNode } from "../domain/types";
import type { ImportOptions } from "../protocol";

/**
 * Prunes a node tree based on depth, size, and visibility options.
 * Returns a new pruned tree without mutating the original.
 */
export function prune(
  nodes: UiNode[],
  options: Pick<ImportOptions, "maxDepth" | "skipInvisible" | "minSize">,
  depth = 0
): UiNode[] {
  if (options.maxDepth !== undefined && options.maxDepth > 0 && depth > options.maxDepth) {
    return [];
  }

  return nodes
    .filter((n) => {
      if (options.skipInvisible && n.visible === false) return false;
      const minSize = options.minSize ?? 0;
      if (minSize > 0 && (n.bounds.w < minSize || n.bounds.h < minSize)) return false;
      return true;
    })
    .map((n) => ({
      ...n,
      children: prune(n.children ?? [], options, depth + 1),
    }));
}

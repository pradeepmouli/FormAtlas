/**
 * Deterministic normalization pipeline for UI node trees.
 * Applies stable ordering and deduplication before rendering.
 */

import type { UiNode } from "./types";

/**
 * Normalizes a node array: sorts siblings deterministically (zIndex asc, name asc).
 * Returns a new array without mutating the original.
 */
export function normalizeNodes(nodes: UiNode[]): UiNode[] {
  return [...nodes]
    .sort((a, b) => {
      const zA = a.zIndex ?? 0;
      const zB = b.zIndex ?? 0;
      if (zA !== zB) return zA - zB;
      return (a.name ?? "").localeCompare(b.name ?? "", undefined, { sensitivity: "base" });
    })
    .map((n) => ({
      ...n,
      children: normalizeNodes(n.children ?? []),
    }));
}

/**
 * Generates a deterministic layer name for a node based on naming mode.
 */
export function layerName(node: UiNode, mode: "name" | "type(name)"): string {
  if (mode === "type(name)") {
    const shortType = node.type.split(".").pop() ?? node.type;
    return `${shortType}(${node.name})`;
  }
  return node.name;
}

/**
 * Counts total nodes in a normalized tree.
 */
export function countAll(nodes: UiNode[]): number {
  let count = 0;
  for (const n of nodes) {
    count++;
    count += countAll(n.children ?? []);
  }
  return count;
}

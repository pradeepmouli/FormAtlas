/**
 * Z-order policy for deterministic layer ordering in Figma.
 * Lower zIndex → lower in layer stack (behind); higher → on top.
 */

import type { UiNode } from "../domain/types";

/**
 * Returns nodes sorted in ascending zIndex order for Figma layer creation.
 * In Figma, later-appended children appear on top.
 */
export function sortByZOrder(nodes: UiNode[]): UiNode[] {
  return [...nodes].sort((a, b) => {
    const zA = a.zIndex ?? 0;
    const zB = b.zIndex ?? 0;
    if (zA !== zB) return zA - zB;
    return (a.name ?? "").localeCompare(b.name ?? "");
  });
}

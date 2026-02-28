/**
 * Schema version routing and validation for UI dump bundles.
 * Implements MAJOR.MINOR compatibility rules from the interop contract.
 */

import type { UiDumpBundle } from "./types";

export const CURRENT_MAJOR = 1;

export function isCompatible(schemaVersion: string, allowHigherMajor = false): boolean {
  const parts = schemaVersion?.split(".");
  if (!parts || parts.length < 2) return false;
  const major = parseInt(parts[0] ?? "0", 10);
  if (isNaN(major)) return false;
  if (major > CURRENT_MAJOR) return allowHigherMajor;
  return major === CURRENT_MAJOR;
}

/**
 * Parses JSON text into a UiDumpBundle and validates required fields.
 * Returns the bundle or throws with a clear error message.
 */
export function parseBundle(jsonText: string): UiDumpBundle {
  let obj: unknown;
  try {
    obj = JSON.parse(jsonText);
  } catch (e) {
    throw new Error(`Invalid JSON: ${(e as Error).message}`);
  }

  if (!obj || typeof obj !== "object") throw new Error("Bundle must be a JSON object.");

  const bundle = obj as Record<string, unknown>;
  if (typeof bundle["schemaVersion"] !== "string")
    throw new Error("Missing required field: schemaVersion");
  if (!bundle["form"] || typeof bundle["form"] !== "object")
    throw new Error("Missing required field: form");
  if (!Array.isArray(bundle["nodes"]))
    throw new Error("Missing required field: nodes");

  return obj as UiDumpBundle;
}

/**
 * Counts nodes and DevExpress kinds in a bundle for display.
 */
export function countNodes(
  nodes: UiDumpBundle["nodes"]
): { total: number; devexpressKinds: string[] } {
  let total = 0;
  const kinds = new Set<string>();

  function walk(ns: UiDumpBundle["nodes"]) {
    for (const n of ns) {
      total++;
      const kind = n.metadata?.devexpress?.kind;
      if (kind) kinds.add(kind);
      walk(n.children ?? []);
    }
  }

  walk(nodes);
  return { total, devexpressKinds: Array.from(kinds).sort() };
}

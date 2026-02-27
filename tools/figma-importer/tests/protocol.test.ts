import { describe, it, expect } from "vitest";
import type {
  ValidateRequest,
  ImportRequest,
  ValidateResult,
  ImportResult,
  ImportProgress,
} from "../src/protocol";
import { DEFAULT_IMPORT_OPTIONS } from "../src/protocol";

describe("Protocol types", () => {
  it("ValidateRequest has correct type discriminator", () => {
    const msg: ValidateRequest = { type: "VALIDATE_REQUEST", jsonText: "{}" };
    expect(msg.type).toBe("VALIDATE_REQUEST");
  });

  it("ImportRequest has correct type discriminator", () => {
    const msg: ImportRequest = {
      type: "IMPORT_REQUEST",
      jsonText: "{}",
      options: DEFAULT_IMPORT_OPTIONS,
    };
    expect(msg.type).toBe("IMPORT_REQUEST");
  });

  it("ValidateResult ok=false includes error", () => {
    const msg: ValidateResult = {
      type: "VALIDATE_RESULT",
      ok: false,
      warnings: [],
      error: "Invalid JSON",
    };
    expect(msg.ok).toBe(false);
    expect(msg.error).toBe("Invalid JSON");
  });

  it("ValidateResult ok=true includes schema and counts", () => {
    const msg: ValidateResult = {
      type: "VALIDATE_RESULT",
      ok: true,
      schemaVersion: "1.0",
      form: { name: "F", width: 800, height: 600 },
      counts: { nodes: 5, devexpressKinds: ["GridControl"] },
      warnings: [],
    };
    expect(msg.ok).toBe(true);
    expect(msg.counts?.nodes).toBe(5);
  });

  it("ImportProgress has monotonic phase fields", () => {
    const msgs: ImportProgress[] = [
      { type: "IMPORT_PROGRESS", phase: "RenderingNodes", done: 0, total: 10 },
      { type: "IMPORT_PROGRESS", phase: "RenderingNodes", done: 5, total: 10 },
      { type: "IMPORT_PROGRESS", phase: "RenderingNodes", done: 10, total: 10 },
    ];
    for (let i = 1; i < msgs.length; i++) {
      expect(msgs[i]!.done).toBeGreaterThanOrEqual(msgs[i - 1]!.done);
    }
  });

  it("ImportResult ok=false includes error", () => {
    const msg: ImportResult = {
      type: "IMPORT_RESULT",
      ok: false,
      warnings: [],
      error: "Something broke",
    };
    expect(msg.ok).toBe(false);
  });

  it("DEFAULT_IMPORT_OPTIONS has all required fields", () => {
    expect(DEFAULT_IMPORT_OPTIONS.includeScreenshot).toBeDefined();
    expect(DEFAULT_IMPORT_OPTIONS.lockScreenshot).toBeDefined();
    expect(DEFAULT_IMPORT_OPTIONS.smartDevExpress).toBeDefined();
    expect(DEFAULT_IMPORT_OPTIONS.namingMode).toBeDefined();
  });
});

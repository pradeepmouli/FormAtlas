import { describe, it, expect } from "vitest";
import { parseBundle, isCompatible, countNodes } from "../src/domain/schema";

describe("Bundle parser", () => {
  it("parses a valid minimal bundle", () => {
    const json = JSON.stringify({
      schemaVersion: "1.0",
      form: { name: "F", type: "T", width: 800, height: 600 },
      nodes: [],
    });
    const bundle = parseBundle(json);
    expect(bundle.schemaVersion).toBe("1.0");
    expect(bundle.form.name).toBe("F");
  });

  it("throws on invalid JSON", () => {
    expect(() => parseBundle("{invalid}")).toThrow("Invalid JSON");
  });

  it("throws when schemaVersion is missing", () => {
    const json = JSON.stringify({ form: {}, nodes: [] });
    expect(() => parseBundle(json)).toThrow("schemaVersion");
  });

  it("throws when form is missing", () => {
    const json = JSON.stringify({ schemaVersion: "1.0", nodes: [] });
    expect(() => parseBundle(json)).toThrow("form");
  });

  it("throws when nodes is missing", () => {
    const json = JSON.stringify({ schemaVersion: "1.0", form: {} });
    expect(() => parseBundle(json)).toThrow("nodes");
  });
});

describe("Schema version compatibility", () => {
  it("accepts same major version", () => {
    expect(isCompatible("1.0")).toBe(true);
    expect(isCompatible("1.5")).toBe(true);
  });

  it("rejects higher major version by default", () => {
    expect(isCompatible("2.0")).toBe(false);
  });

  it("accepts higher major with allowHigherMajor=true", () => {
    expect(isCompatible("2.0", true)).toBe(true);
  });

  it("rejects invalid version strings", () => {
    expect(isCompatible("")).toBe(false);
    expect(isCompatible("bad")).toBe(false);
    expect(isCompatible("1")).toBe(false);
  });
});

describe("countNodes", () => {
  it("counts flat nodes", () => {
    const nodes = [
      { id: "n1", type: "T", name: "N", bounds: { x: 0, y: 0, w: 100, h: 100 }, children: [] },
      { id: "n2", type: "T", name: "N2", bounds: { x: 0, y: 0, w: 100, h: 100 }, children: [] },
    ];
    const { total } = countNodes(nodes as any);
    expect(total).toBe(2);
  });

  it("counts nested nodes", () => {
    const nodes = [
      {
        id: "n1", type: "T", name: "N", bounds: { x: 0, y: 0, w: 100, h: 100 },
        children: [
          { id: "n2", type: "T", name: "C", bounds: { x: 0, y: 0, w: 10, h: 10 }, children: [] }
        ]
      },
    ];
    const { total } = countNodes(nodes as any);
    expect(total).toBe(2);
  });

  it("collects devexpress kinds", () => {
    const nodes = [
      {
        id: "n1", type: "T", name: "N",
        bounds: { x: 0, y: 0, w: 100, h: 100 },
        metadata: { devexpress: { kind: "GridControl" } },
        children: []
      },
    ];
    const { devexpressKinds } = countNodes(nodes as any);
    expect(devexpressKinds).toContain("GridControl");
  });
});

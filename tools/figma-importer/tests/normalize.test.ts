import { describe, it, expect } from "vitest";
import { normalizeNodes, layerName, countAll } from "../src/domain/normalize";
import type { UiNode } from "../src/domain/types";

function makeNode(id: string, name: string, zIndex = 0, children: UiNode[] = []): UiNode {
  return {
    id,
    type: "Control",
    name,
    bounds: { x: 0, y: 0, w: 100, h: 50 },
    zIndex,
    children,
  };
}

describe("normalizeNodes", () => {
  it("sorts by zIndex ascending", () => {
    const nodes = [makeNode("a", "A", 2), makeNode("b", "B", 0), makeNode("c", "C", 1)];
    const result = normalizeNodes(nodes);
    expect(result.map((n) => n.id)).toEqual(["b", "c", "a"]);
  });

  it("sorts by name when zIndex is equal", () => {
    const nodes = [makeNode("a", "Z", 0), makeNode("b", "A", 0)];
    const result = normalizeNodes(nodes);
    expect(result[0]?.id).toBe("b"); // "A" < "Z"
  });

  it("does not mutate original array", () => {
    const nodes = [makeNode("a", "A", 1), makeNode("b", "B", 0)];
    const copy = [...nodes];
    normalizeNodes(nodes);
    expect(nodes[0]?.id).toBe(copy[0]?.id);
  });

  it("recursively normalizes children", () => {
    const child1 = makeNode("c1", "C1", 2);
    const child2 = makeNode("c2", "C2", 0);
    const root = makeNode("root", "Root", 0, [child1, child2]);
    const result = normalizeNodes([root]);
    expect(result[0]?.children[0]?.id).toBe("c2");
  });
});

describe("layerName", () => {
  it("name mode returns name", () => {
    const node = makeNode("n1", "MyButton");
    expect(layerName(node, "name")).toBe("MyButton");
  });

  it("type(name) mode returns type(name)", () => {
    const node: UiNode = {
      id: "n1",
      type: "System.Windows.Forms.Button",
      name: "btnOK",
      bounds: { x: 0, y: 0, w: 80, h: 30 },
      children: [],
    };
    expect(layerName(node, "type(name)")).toBe("Button(btnOK)");
  });
});

describe("countAll", () => {
  it("counts all nodes recursively", () => {
    const nodes = [
      makeNode("a", "A", 0, [makeNode("b", "B"), makeNode("c", "C")]),
      makeNode("d", "D"),
    ];
    expect(countAll(nodes)).toBe(4);
  });

  it("returns 0 for empty array", () => {
    expect(countAll([])).toBe(0);
  });
});

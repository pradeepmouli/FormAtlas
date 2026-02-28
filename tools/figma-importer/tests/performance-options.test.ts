import { describe, it, expect } from "vitest";
import { prune } from "../src/perf/budget";
import type { UiNode } from "../src/domain/types";

function makeNode(id: string, w = 100, h = 50, visible = true, children: UiNode[] = []): UiNode {
  return {
    id,
    type: "Control",
    name: id,
    bounds: { x: 0, y: 0, w, h },
    visible,
    children,
  };
}

describe("prune", () => {
  it("returns all nodes when no constraints", () => {
    const nodes = [makeNode("a"), makeNode("b")];
    const result = prune(nodes, {});
    expect(result.length).toBe(2);
  });

  it("filters invisible nodes when skipInvisible=true", () => {
    const nodes = [makeNode("visible", 100, 50, true), makeNode("hidden", 100, 50, false)];
    const result = prune(nodes, { skipInvisible: true });
    expect(result.length).toBe(1);
    expect(result[0]?.id).toBe("visible");
  });

  it("filters small nodes by minSize", () => {
    const nodes = [makeNode("big", 100, 100), makeNode("tiny", 5, 5)];
    const result = prune(nodes, { minSize: 10 });
    expect(result.length).toBe(1);
    expect(result[0]?.id).toBe("big");
  });

  it("respects maxDepth", () => {
    const deep = makeNode("deep");
    const child = makeNode("child", 100, 50, true, [deep]);
    const root = makeNode("root", 100, 50, true, [child]);
    const result = prune([root], { maxDepth: 1 });
    expect(result[0]?.children[0]?.children.length).toBe(0);
  });

  it("does not mutate original nodes", () => {
    const nodes = [makeNode("a", 100, 50, false)];
    prune(nodes, { skipInvisible: true });
    expect(nodes[0]?.visible).toBe(false); // unchanged
  });
});

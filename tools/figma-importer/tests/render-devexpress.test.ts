import { describe, it, expect, vi, beforeEach } from "vitest";
import { DevExpressRendererRegistry } from "../src/render/devexpress/registry";
import type { UiNode } from "../src/domain/types";
import { DEFAULT_IMPORT_OPTIONS } from "../src/protocol";

// Mock the Figma global for unit tests
function makeMockFrame(name = "frame"): any {
  const children: any[] = [];
  return {
    name,
    width: 0,
    height: 0,
    x: 0,
    y: 0,
    id: `mock-${Math.random()}`,
    resize: vi.fn(function (this: any, w: number, h: number) {
      this.width = w;
      this.height = h;
    }),
    appendChild: vi.fn(function (this: any, child: any) {
      children.push(child);
    }),
    insertChild: vi.fn(),
    _children: children,
  };
}

beforeEach(() => {
  (globalThis as any).figma = {
    createFrame: vi.fn(() => makeMockFrame()),
    createRectangle: vi.fn(() => makeMockFrame("rect")),
    createImage: vi.fn(() => ({ hash: "fake-hash" })),
    currentPage: { appendChild: vi.fn() },
    ui: { postMessage: vi.fn(), onmessage: null },
    showUI: vi.fn(),
    __html__: "",
  };
});

function makeDevExpressNode(kind: string, meta?: Record<string, unknown>): UiNode {
  return {
    id: "dxNode",
    type: `DevExpress.SomeNS.${kind}`,
    name: kind,
    bounds: { x: 0, y: 0, w: 800, h: 400 },
    children: [],
    metadata: {
      devexpress: {
        kind,
        ...meta,
      } as any,
    },
  };
}

describe("DevExpressRendererRegistry", () => {
  it("has renderers for all 5 known kinds", () => {
    const reg = DevExpressRendererRegistry.createDefault();
    const kinds = ["GridControl", "PivotGridControl", "XtraTabControl", "LayoutControl", "RibbonControl"];
    for (const kind of kinds) {
      expect(reg.getRenderer(kind)).toBeDefined();
    }
  });

  it("returns undefined for unknown kind", () => {
    const reg = DevExpressRendererRegistry.createDefault();
    expect(reg.getRenderer("UnknownWidget")).toBeUndefined();
  });
});

describe("GridRenderer", () => {
  it("renders a frame", () => {
    const reg = DevExpressRendererRegistry.createDefault();
    const renderer = reg.getRenderer("GridControl")!;
    const node = makeDevExpressNode("GridControl", {
      grid: { viewType: "GridView", columns: [{ caption: "ID", fieldName: "Id", width: 80 }] },
    });
    const frame = renderer.render(node, DEFAULT_IMPORT_OPTIONS);
    expect(frame.name).toBe("GridControl");
  });
});

describe("TabsRenderer", () => {
  it("renders tabs for each page", () => {
    const reg = DevExpressRendererRegistry.createDefault();
    const renderer = reg.getRenderer("XtraTabControl")!;
    const node = makeDevExpressNode("XtraTabControl", {
      tabs: { selectedIndex: 0, pages: [{ text: "Page 1", index: 0 }, { text: "Page 2", index: 1 }] },
    });
    const frame = renderer.render(node, DEFAULT_IMPORT_OPTIONS);
    expect(frame.name).toBe("XtraTabControl");
    // Two child frames were appended
    expect((figma.createFrame as any).mock.calls.length).toBeGreaterThanOrEqual(3); // parent + 2 tabs
  });
});

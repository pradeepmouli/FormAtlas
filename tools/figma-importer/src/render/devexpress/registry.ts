/**
 * DevExpress smart renderer registry.
 * Dispatches rendering to kind-specific implementations.
 */

import type { UiNode } from "../../domain/types";
import type { ImportOptions } from "../../protocol";
import { GridRenderer } from "./grid";
import { PivotRenderer } from "./pivot";
import { TabsRenderer } from "./tabs";
import { LayoutRenderer } from "./layout";
import { RibbonRenderer } from "./ribbon";

export interface DevExpressRenderer {
  kind: string | readonly string[];
  render(node: UiNode, options: ImportOptions): FrameNode;
}

export class DevExpressRendererRegistry {
  private readonly _map = new Map<string, DevExpressRenderer>();

  register(renderer: DevExpressRenderer): void {
    const kinds = Array.isArray(renderer.kind) ? renderer.kind : [renderer.kind];
    for (const k of kinds as string[]) {
      this._map.set(k, renderer);
    }
  }

  getRenderer(kind: string): DevExpressRenderer | undefined {
    return this._map.get(kind);
  }

  static createDefault(): DevExpressRendererRegistry {
    const reg = new DevExpressRendererRegistry();
    reg.register(new GridRenderer());
    reg.register(new PivotRenderer());
    reg.register(new TabsRenderer());
    reg.register(new LayoutRenderer());
    reg.register(new RibbonRenderer());
    return reg;
  }
}

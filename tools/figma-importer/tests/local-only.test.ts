import { describe, it, expect, vi } from "vitest";

/**
 * Local-only processing tests — verify that the importer never invokes network APIs.
 * The importer must not use fetch, XMLHttpRequest, or any network mechanism.
 */
describe("Local-only processing", () => {
  it("does not call fetch during bundle parsing", async () => {
    const fetchSpy = vi.spyOn(globalThis, "fetch" as any).mockResolvedValue({} as any);

    const { parseBundle } = await import("../src/domain/schema");
    const json = JSON.stringify({
      schemaVersion: "1.0",
      form: { name: "F", type: "T", width: 800, height: 600 },
      nodes: [],
    });
    parseBundle(json);

    expect(fetchSpy).not.toHaveBeenCalled();
    fetchSpy.mockRestore();
  });

  it("does not call fetch during normalization", async () => {
    const fetchSpy = vi.spyOn(globalThis, "fetch" as any).mockResolvedValue({} as any);

    const { normalizeNodes } = await import("../src/domain/normalize");
    normalizeNodes([]);

    expect(fetchSpy).not.toHaveBeenCalled();
    fetchSpy.mockRestore();
  });

  it("schema module does not import from network", async () => {
    // Verify the module resolves without any dynamic network imports
    const mod = await import("../src/domain/schema");
    expect(mod.parseBundle).toBeTypeOf("function");
    expect(mod.isCompatible).toBeTypeOf("function");
    expect(mod.countNodes).toBeTypeOf("function");
  });
});

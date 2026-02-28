import { defineConfig } from "vite";
import { resolve } from "path";

// Figma plugins require two independent IIFE bundles: one for the worker (main.ts)
// and one for the HTML panel UI (ui.ts). IIFE format is incompatible with
// multi-entry code-splitting, so we select the entry via BUILD_ENTRY env var
// and run two sequential builds (see package.json "build" script).
const validEntries = ["main", "ui"] as const;
type Entry = (typeof validEntries)[number];
const rawEntry = process.env["BUILD_ENTRY"] ?? "main";
if (!validEntries.some((e) => e === rawEntry)) {
  throw new Error(
    `Invalid BUILD_ENTRY="${rawEntry}". Must be one of: ${validEntries.join(", ")}`
  );
}
const entry = rawEntry as Entry;

export default defineConfig({
  build: {
    lib: {
      entry: resolve(__dirname, `src/${entry}.ts`),
      formats: ["iife"],
      name: entry,
      fileName: () => `${entry}.js`,
    },
    outDir: "dist",
    // Only clear dist on the first build pass (main)
    emptyOutDir: entry === "main",
    target: "ES2020",
    minify: false,
  },
  test: {
    globals: true,
    environment: "node",
  },
});

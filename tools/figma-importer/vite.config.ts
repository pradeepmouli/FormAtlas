import { defineConfig } from "vite";
import { resolve } from "path";

export default defineConfig({
  build: {
    rollupOptions: {
      input: {
        main: resolve(__dirname, "src/main.ts"),
        ui: resolve(__dirname, "src/ui.ts"),
      },
      output: {
        entryFileNames: "[name].js",
        format: "iife",
        dir: "dist",
      },
    },
    target: "ES2020",
    minify: false,
  },
  test: {
    globals: true,
    environment: "node",
  },
});

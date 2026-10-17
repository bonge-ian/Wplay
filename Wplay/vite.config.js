import { resolve } from "node:path";
import tailwindcss from "@tailwindcss/vite";
import { defineConfig } from "vite";
import frankenUi from "franken-ui/plugin-vite";

export default defineConfig({
  root: resolve(import.meta.dirname, "frontend"),
  plugins: [
    tailwindcss(),
    frankenUi({
      preflight: false,
      layer: true,
      layerExceptions: ["chart"],
    }),
  ],
  server: {
    strictPort: true,
  },
  build: {
    manifest: true,
    outDir: resolve(import.meta.dirname, "wwwroot/dist"),
    emptyOutDir: true,
    rollupOptions: {
      input: resolve(import.meta.dirname, "frontend/src/main.js"),
    },
  },
});

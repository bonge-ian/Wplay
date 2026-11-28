import fs from "node:fs";
import { defineConfig } from  "vite";

const hotFile = "wwwroot/hot";

/**
 * Creates/removes a hot file while the Vite dev server
 * is running so ASP.NET knows whether to load assets
 * from Vite (HMR) or from the build manifest.
 */
 const hotFilePlugin = () => ({
    name: "vite-hot-file",

    configureServer(server) {
        const cleanup = () => {
            fs.rmSync(hotFile, { force: true });
        };

        server.httpServer?.once("listening", () => {
            const address = server.httpServer.address();

            const port =
                typeof address === "object"
                    ? address.port
                    : 5173;

            fs.mkdirSync("wwwroot", {
                recursive: true,
            });

            fs.writeFileSync(
                hotFile,
                `http://localhost:${port}`
            );
        });

        server.httpServer?.once("close", cleanup);

        process.on("exit", cleanup);
        process.on("SIGINT", () => process.exit());
        process.on("SIGTERM", () => process.exit());
    },
});

export default defineConfig({
    plugins: [
        hotFilePlugin(),
    ],
    build: {
        manifest: true,
        outDir: "wwwroot/build",
        emptyOutDir: true,
        rollupOptions: {
            input: "Frontend/js/app.js",
        },
    },
    server: {
        host: "localhost",
        port: 5173,
        strictPort: false,
        origin: "http://localhost:5173",
        cors: true,
    },
});
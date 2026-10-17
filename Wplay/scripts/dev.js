import { resolve } from "node:path";
import { concurrently } from "concurrently";

const { result } = concurrently(
  [
    { name: "vite", command: "npm run --silent dev:vite" },
    { name: "aspnet", command: "npm run --silent dev:aspnet" },
  ],
  {
    cwd: resolve(import.meta.dirname, ".."),
    prefix: "[{name}]",
    prefixColors: ["cyan", "green"],
    killOthersOn: ["failure", "success"],
  },
);

result.then(
  (closeEvents) => {
    if (closeEvents.some(({ exitCode }) => exitCode !== 0)) {
      process.exitCode = 1;
    }
  },
  () => {
    process.exitCode = 1;
  },
);

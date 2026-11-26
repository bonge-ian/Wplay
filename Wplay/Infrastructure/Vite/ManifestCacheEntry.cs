using System.Text.Json;

namespace Wplay.Infrastructure.Vite;

internal sealed record ManifestCacheEntry(DateTime LastWriteTimeUtc, JsonDocument Manifest);

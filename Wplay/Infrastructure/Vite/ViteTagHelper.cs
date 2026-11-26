using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Wplay.Infrastructure.Vite;

/// <summary>
/// ASP.NET Core equivalent of Laravel's @vite.
///
/// Development:
///     Uses Vite HMR if wwwroot/hot exists.
///
/// Production:
///     Reads manifest.json and emits hashed assets.
///
/// Usage:
///
///     &lt;vite entry="Frontend/js/app.js" /&gt;
///
/// </summary>
/// <remarks>
/// Development:
///     wwwroot/hot
///
/// Production:
///     wwwroot/build/.vite/manifest.json
///
/// Inspired by Laravel's @vite integration.
/// </remarks>
public sealed class ViteTagHelper(IWebHostEnvironment environment) : TagHelper
{
    private const string DefaultEntry = "Frontend/js/app.js";

    private const string HotFileName = "hot";
    private const string BuildDirectory = "build";
    
    private const string ManifestDirectory = ".vite";
    
    private const string ManifestFileName = "manifest.json";
    
    /// <summary>
    /// Manifest files are effectively immutable between builds.
    ///
    /// Cache parsed manifests to avoid repeatedly opening and
    /// parsing manifest.json on every request.
    ///
    /// Cache entries are automatically refreshed when the file's
    /// LastWriteTimeUtc changes.
    /// </summary>
    private static readonly ConcurrentDictionary<string, ManifestCacheEntry> ManifestCache = new();
    
    /// <summary>
    /// Vite entry point.
    ///
    /// Example:
    ///
    ///      &lt;vite entry="Frontend/js/app.js" /&gt;
    ///
    /// Defaults to Frontend/js/app.js.
    /// </summary>
    public string Entry { get; set; } = DefaultEntry;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        // Replace the <vite /> element entirely with the generated
        // script/link tags rather than rendering a literal
        // <vite> element into the HTML output.
        output.TagName = null;


        if (TryRenderHotAssets(out var hotHtml))
        {
            output.Content.SetHtmlContent(hotHtml);
            return;
        }

        output.Content.SetHtmlContent(RenderProductionAssets());
    }

    private string RenderProductionAssets()
    {
        var manifestPath = Path.Combine(environment.WebRootPath, BuildDirectory, ManifestDirectory, ManifestFileName);

        if (!File.Exists(manifestPath))
        {
            throw new InvalidOperationException(
                message: $"""
                Vite manifest not found.

                Expected:
                    {manifestPath}
                Run:
                    pnpm build
                before starting the application.
                """
            );
        }

        var manifest = GetManifest(manifestPath);
        
        if (!manifest.RootElement.TryGetProperty(Entry,out var chunk))
        {
            throw new InvalidOperationException(
                message: $"""
                Entry '{Entry}' was not found within the Vite manifest.

                Verify the entry exists in:

                    {manifestPath}
                """
            );
        }
        
        var html = new StringBuilder();
        
        if (chunk.TryGetProperty("css", out var cssFiles))
        {
            foreach (var cssFile in cssFiles.EnumerateArray())
            {
                html.AppendLine($"""<link rel="stylesheet" href="/build/{cssFile.GetString()}" />""");
            }
        }

        html.AppendLine(
            $"""<script type="module" src="/build/{chunk.GetProperty("file").GetString()}"></script>"""
        );
        
        return html.ToString();
    }

    /// <summary>
    /// Development mode.
    ///
    /// The hot file is created by Vite when it starts and removed when it stops.
    ///
    /// Presence of the hot file indicates that a Vite development
    /// server is currently running.
    ///
    /// ASP.NET should load assets directly from the dev server
    /// instead of reading build artifacts from manifest.json.
    /// </summary>
    private bool TryRenderHotAssets(out string html)
    {
        html = string.Empty;
        var hotFile = Path.Combine(environment.WebRootPath, HotFileName);

        if (!File.Exists(hotFile))
        {
            return false;
        }

        var viteServerUrl = File.ReadAllText(hotFile).Trim().TrimEnd(trimChar: '/');

        html = $$"""
            <script type="module" src="{{viteServerUrl}}/@vite/client"></script>
            <script type="module" src="{{viteServerUrl}}/{{Entry}}"></script>
        """;

        return true;
    }
    
    /// <summary>
    /// Returns a cached manifest if available.
    ///
    /// If Vite generates a new build, the manifest's write time
    /// changes and the cache entry is transparently refreshed.
    /// </summary>
    private static JsonDocument GetManifest(string manifestPath)
    {
        var lastWriteTime = File.GetLastWriteTimeUtc(manifestPath);

        var entry = ManifestCache.AddOrUpdate(
            manifestPath,
             _ => new ManifestCacheEntry(lastWriteTime, JsonDocument.Parse(File.ReadAllText(manifestPath))),
            (_, existing) =>
            {
                if (existing.LastWriteTimeUtc == lastWriteTime)
                {
                    return existing;
                }
                // vite generated a new build.
                // dispose the old parsed manifest and replace with new manifest.
                existing.Manifest.Dispose();

                return new ManifestCacheEntry(lastWriteTime, JsonDocument.Parse(File.ReadAllText(manifestPath)));
            }
        );

        return entry.Manifest;
    }
}

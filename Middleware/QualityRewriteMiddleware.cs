using Irukandji.Configuration;
using MediaBrowser.Common.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Irukandji.Middleware;

/// <summary>
/// Intercepts image requests and rewrites the 'quality' query parameter
/// for lossy formats (JPEG, WebP) to the configured TargetQuality value.
/// 
/// Runs independently of EnableCache — useful on its own.
/// Skips PNG (lossless format where quality has no meaningful effect).
/// Safe to run before authentication — doesn't skip core validation/security checks.
/// </summary>
public class QualityRewriteMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfigurationManager _configManager;
    private readonly ILogger<QualityRewriteMiddleware> _logger;

    // Image route patterns to match (from Jellyfin's ImageController)
    private static readonly string[] ImageRoutePrefixes =
    {
        "/Items",
        "/Users",
        "/Images",
        "/Studios",
        "/Genres",
        "/MusicGenres",
        "/Persons",
        "/Years"
    };

    public QualityRewriteMiddleware(
        RequestDelegate next,
        IConfigurationManager configManager,
        ILogger<QualityRewriteMiddleware> logger)
    {
        _next = next;
        _configManager = configManager;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;

        // Only process image routes
        if (IsImageRoute(request.Path))
        {
            RewriteQualityIfApplicable(context);
        }

        await _next(context);
    }

    private bool IsImageRoute(PathString path)
    {
        var pathStr = path.ToString();
        return ImageRoutePrefixes.Any(prefix =>
            pathStr.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
            pathStr.Contains("/Images/", StringComparison.OrdinalIgnoreCase));
    }

    private void RewriteQualityIfApplicable(HttpContext context)
    {
        try
        {
            var config = _configManager.GetConfiguration<PluginConfiguration>("Irukandji");
            if (config == null)
            {
                return;
            }

            var query = context.Request.Query;
            string? format = query["format"].FirstOrDefault()?.ToLowerInvariant();

            // Only rewrite for lossy formats; skip PNG
            if (string.IsNullOrEmpty(format) || !IsLossyFormat(format))
            {
                return;
            }

            // Rewrite the quality parameter
            var newQuery = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(query);
            newQuery["quality"] = config.TargetQuality.ToString();
            context.Request.QueryString = QueryString.Create(newQuery);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error rewriting quality parameter");
        }
    }

    private static bool IsLossyFormat(string format)
    {
        return format.Equals("jpeg", StringComparison.OrdinalIgnoreCase) ||
               format.Equals("jpg", StringComparison.OrdinalIgnoreCase) ||
               format.Equals("webp", StringComparison.OrdinalIgnoreCase);
    }
}
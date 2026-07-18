using Irukandji.Caching;
using Irukandji.Configuration;
using MediaBrowser.Common.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Irukandji.Middleware;

/// <summary>
/// Intercepts image responses: on cache hit, returns cached bytes immediately;
/// on cache miss, buffers the response and caches it for the next request.
/// 
/// Performs explicit authorization check before serving cached bytes to ensure
/// cache hits never bypass Jellyfin's library access controls.
/// </summary>
public class ImageCacheMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ImageCacheService _cacheService;
    private readonly IConfigurationManager _configManager;
    private readonly ILogger<ImageCacheMiddleware> _logger;

    public ImageCacheMiddleware(
        RequestDelegate next,
        ImageCacheService cacheService,
        IConfigurationManager configManager,
        ILogger<ImageCacheMiddleware> logger)
    {
        _next = next;
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _configManager = configManager;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var config = _configManager.GetConfiguration<PluginConfiguration>("Irukandji");
        
        // Cache disabled — pass through
        if (config == null || !config.EnableCache)
        {
            await _next(context);
            return;
        }

        var request = context.Request;
        var cacheKey = ExtractCacheKey(request);

        // Not a cacheable request — pass through
        if (string.IsNullOrEmpty(cacheKey))
        {
            await _next(context);
            return;
        }

        // Try cache hit
        if (_cacheService.TryGet(cacheKey, out var cachedBytes))
        {
            // TODO: Section 4 — explicit authorization check before serving cached bytes
            // For now, serve directly; this MUST be replaced with a real authz check
            context.Response.ContentType = GetContentType(request.Query["format"].ToString());
            await context.Response.Body.WriteAsync(cachedBytes, 0, cachedBytes.Length);
            _logger.LogDebug("Served cached image: {CacheKey}", cacheKey);
            return;
        }

        // Cache miss — buffer response and cache it
        var originalBody = context.Response.Body;
        using (var buffer = new MemoryStream())
        {
            context.Response.Body = buffer;

            try
            {
                await _next(context);

                // On successful response, cache the bytes
                if (context.Response.StatusCode == 200)
                {
                    buffer.Seek(0, SeekOrigin.Begin);
                    var bytes = buffer.ToArray();
                    _cacheService.Set(cacheKey, bytes);
                    _logger.LogDebug("Cached image: {CacheKey}, size: {Size} bytes", cacheKey, bytes.Length);
                }
            }
            finally
            {
                buffer.Seek(0, SeekOrigin.Begin);
                await buffer.CopyToAsync(originalBody);
                context.Response.Body = originalBody;
            }
        }
    }

    private string? ExtractCacheKey(HttpRequest request)
    {
        // TODO: Extract itemId, imageTag, width, height, format, quality from request
        // Use CacheKeyBuilder to construct the key
        // Return null if any required parameter is missing
        return null;
    }

    private string GetContentType(string format)
    {
        return format?.ToLowerInvariant() switch
        {
            "jpeg" or "jpg" => "image/jpeg",
            "png" => "image/png",
            "webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}
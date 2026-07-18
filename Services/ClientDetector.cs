using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace Irukandji.Services;

/// <summary>
/// Detects client type from Authorization header's Client= parameter
/// and from User-Agent (mobile web detection).
/// Used for logging/stats breakdown; does not affect cache behavior.
/// </summary>
public static class ClientDetector
{
    public static string? DetectClientFromHeader(HttpRequest request)
    {
        try
        {
            var auth = request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrEmpty(auth))
            {
                return null;
            }

            // Parse "Client=SomeClient" from Authorization header
            var parts = auth.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (trimmed.StartsWith("Client=", StringComparison.OrdinalIgnoreCase))
                {
                    return trimmed.Substring(7).Trim('"');
                }
            }
        }
        catch
        {
            // Ignore parsing errors
        }

        return null;
    }

    public static bool IsMobileWeb(HttpRequest request)
    {
        var userAgent = request.Headers.UserAgent.ToString().ToLowerInvariant();
        return userAgent.Contains("mobile") || 
               userAgent.Contains("android") || 
               userAgent.Contains("iphone") ||
               userAgent.Contains("ipad");
    }
}
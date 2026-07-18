using System;

namespace Irukandji.Caching;

/// <summary>
/// Builds cache keys in the format:
/// {itemId}_{imageTag}_{width}x{height}_{format}_q{quality}
/// 
/// Including imageTag ensures cache invalidation is automatic when the image changes.
/// Including format prevents collisions between JPEG and WebP requests for the same item.
/// For formats where quality doesn't apply (PNG), normalizes quality segment to "qN/A".
/// </summary>
public static class CacheKeyBuilder
{
    public static string BuildKey(
        string itemId,
        string imageTag,
        int width,
        int height,
        string format,
        int? quality = null)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            throw new ArgumentException("itemId cannot be null or empty", nameof(itemId));
        }

        if (string.IsNullOrEmpty(imageTag))
        {
            throw new ArgumentException("imageTag cannot be null or empty", nameof(imageTag));
        }

        if (string.IsNullOrEmpty(format))
        {
            throw new ArgumentException("format cannot be null or empty", nameof(format));
        }

        // Normalize format to lowercase
        var normalizedFormat = format.ToLowerInvariant();

        // Quality segment: use actual value for lossy formats, "N/A" for lossless
        var qualitySegment = IsLosslessFormat(normalizedFormat)
            ? "qN/A"
            : $"q{quality ?? 0}";

        return $"{itemId}_{imageTag}_{width}x{height}_{normalizedFormat}_{qualitySegment}";
    }

    /// <summary>
    /// Determine if a format is lossless (PNG) or lossy (JPEG, WebP).
    /// </summary>
    private static bool IsLosslessFormat(string format)
    {
        return format.Equals("png", StringComparison.OrdinalIgnoreCase);
    }
}
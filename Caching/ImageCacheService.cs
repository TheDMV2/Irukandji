using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Irukandji.Caching;

/// <summary>
/// Wraps IMemoryCache to cache resized/recompressed images.
/// Maintains hit/miss counters and size tracking via PostEvictionCallback.
/// All entries keyed by itemId_imageTag_widthxheight_format_qQuality to ensure
/// format collisions are prevented and image tag changes invalidate old entries automatically.
/// </summary>
public class ImageCacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<ImageCacheService> _logger;

    // Counters updated via Interlocked for thread safety
    private long _currentSizeBytes;
    private long _currentEntryCount;
    private long _hitCount;
    private long _missCount;

    public ImageCacheService(IMemoryCache cache, ILogger<ImageCacheService> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _currentSizeBytes = 0;
        _currentEntryCount = 0;
        _hitCount = 0;
        _missCount = 0;
    }

    /// <summary>
    /// Attempt to retrieve cached image bytes.
    /// </summary>
    public bool TryGet(string key, out byte[]? bytes)
    {
        if (_cache.TryGetValue(key, out object? cachedValue))
        {
            bytes = cachedValue as byte[];
            if (bytes != null)
            {
                Interlocked.Increment(ref _hitCount);
                return true;
            }
        }

        bytes = null;
        Interlocked.Increment(ref _missCount);
        return false;
    }

    /// <summary>
    /// Store image bytes in cache with size-based eviction.
    /// </summary>
    public void Set(string key, byte[] bytes)
    {
        if (string.IsNullOrEmpty(key) || bytes == null)
        {
            return;
        }

        var cacheOptions = new MemoryCacheEntryOptions
        {
            Size = bytes.Length,
            PostEvictionCallbacks =
            {
                new PostEvictionCallbackRegistration
                {
                    EvictionCallback = (evictionKey, evictionValue, evictionReason, state) =>
                    {
                        if (evictionValue is byte[] evictedBytes)
                        {
                            Interlocked.Add(ref _currentSizeBytes, -evictedBytes.Length);
                            Interlocked.Decrement(ref _currentEntryCount);
                        }
                    }
                }
            }
        };

        _cache.Set(key, bytes, cacheOptions);
        Interlocked.Add(ref _currentSizeBytes, bytes.Length);
        Interlocked.Increment(ref _currentEntryCount);
    }

    /// <summary>
    /// Return current cache statistics.
    /// </summary>
    public CacheStats GetStats()
    {
        return new CacheStats
        {
            CurrentSizeBytes = Interlocked.Read(ref _currentSizeBytes),
            CurrentEntryCount = Interlocked.Read(ref _currentEntryCount),
            HitCount = Interlocked.Read(ref _hitCount),
            MissCount = Interlocked.Read(ref _missCount)
        };
    }

    /// <summary>
    /// Clear all cached entries and reset counters.
    /// </summary>
    public void Clear()
    {
        _cache.Compact(1.0);
        Interlocked.Exchange(ref _currentSizeBytes, 0);
        Interlocked.Exchange(ref _currentEntryCount, 0);
        Interlocked.Exchange(ref _hitCount, 0);
        Interlocked.Exchange(ref _missCount, 0);
        _logger.LogInformation("Image cache cleared");
    }
}

/// <summary>
/// Data transfer object for cache statistics.
/// Field names match the stats endpoint contract in Section 7 exactly.
/// </summary>
public class CacheStats
{
    public long CurrentSizeBytes { get; set; }
    public long CurrentEntryCount { get; set; }
    public long HitCount { get; set; }
    public long MissCount { get; set; }
}
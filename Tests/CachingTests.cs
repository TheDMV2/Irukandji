using Irukandji.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;

namespace Irukandji.Tests;

/// <summary>
/// Unit tests for ImageCacheService and CacheKeyBuilder.
/// </summary>
public class CachingTests
{
    private readonly Mock<ILogger<ImageCacheService>> _mockLogger;
    private readonly IMemoryCache _memoryCache;
    private readonly ImageCacheService _cacheService;

    public CachingTests()
    {
        _mockLogger = new Mock<ILogger<ImageCacheService>>();
        var cacheOptions = new MemoryCacheOptions { SizeLimit = 10 * 1024 * 1024 }; // 10 MB for tests
        _memoryCache = new MemoryCache(cacheOptions);
        _cacheService = new ImageCacheService(_memoryCache, _mockLogger.Object);
    }

    [Fact]
    public void CacheKeyBuilder_BuildsKeyWithCorrectFormat()
    {
        // Arrange
        var itemId = "item123";
        var imageTag = "tag456";
        var width = 744;
        var height = 496;
        var format = "jpeg";
        var quality = 76;

        // Act
        var key = CacheKeyBuilder.BuildKey(itemId, imageTag, width, height, format, quality);

        // Assert
        Assert.Equal("item123_tag456_744x496_jpeg_q76", key);
    }

    [Fact]
    public void CacheKeyBuilder_NormalizesFormatToLowercase()
    {
        // Arrange
        var itemId = "item123";
        var imageTag = "tag456";

        // Act
        var keyJpeg = CacheKeyBuilder.BuildKey(itemId, imageTag, 100, 100, "JPEG", 76);
        var keyWebp = CacheKeyBuilder.BuildKey(itemId, imageTag, 100, 100, "WebP", 76);

        // Assert
        Assert.Contains("_jpeg_", keyJpeg);
        Assert.Contains("_webp_", keyWebp);
    }

    [Fact]
    public void CacheKeyBuilder_UsesNAForPngQuality()
    {
        // Arrange
        var itemId = "item123";
        var imageTag = "tag456";

        // Act
        var key = CacheKeyBuilder.BuildKey(itemId, imageTag, 100, 100, "png", null);

        // Assert
        Assert.EndsWith("_qN/A", key);
    }

    [Fact]
    public void ImageCacheService_SetAndGet_ReturnsStoredBytes()
    {
        // Arrange
        var key = "test_key";
        var bytes = new byte[] { 1, 2, 3, 4, 5 };

        // Act
        _cacheService.Set(key, bytes);
        var result = _cacheService.TryGet(key, out var retrieved);

        // Assert
        Assert.True(result);
        Assert.NotNull(retrieved);
        Assert.Equal(bytes, retrieved);
    }

    [Fact]
    public void ImageCacheService_TryGet_Miss_ReturnsFalse()
    {
        // Act
        var result = _cacheService.TryGet("nonexistent", out var retrieved);

        // Assert
        Assert.False(result);
        Assert.Null(retrieved);
    }

    [Fact]
    public void ImageCacheService_GetStats_TracksHitsAndMisses()
    {
        // Arrange
        var key = "test_key";
        var bytes = new byte[] { 1, 2, 3 };
        _cacheService.Set(key, bytes);

        // Act
        _cacheService.TryGet(key, out _); // Hit
        _cacheService.TryGet("nonexistent", out _); // Miss
        var stats = _cacheService.GetStats();

        // Assert
        Assert.Equal(1, stats.HitCount);
        Assert.Equal(1, stats.MissCount);
    }

    [Fact]
    public void ImageCacheService_Clear_ResetsCounters()
    {
        // Arrange
        var key = "test_key";
        _cacheService.Set(key, new byte[] { 1, 2, 3 });
        _cacheService.TryGet(key, out _);

        // Act
        _cacheService.Clear();
        var stats = _cacheService.GetStats();

        // Assert
        Assert.Equal(0, stats.CurrentEntryCount);
        Assert.Equal(0, stats.HitCount);
        Assert.Equal(0, stats.MissCount);
    }
}
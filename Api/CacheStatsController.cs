using Irukandji.Caching;
using Irukandji.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;

namespace Irukandji.Api;

/// <summary>
/// Admin-only stats endpoint: GET /Plugin/ImageCache/Stats
/// Returns cache performance metrics in the contract defined in Section 7:
/// { totalSizeMB, fileCount, hits, misses, ratio }
/// </summary>
[ApiController]
[Route("Plugin/ImageCache")]
[Authorize(Policy = "RequireAdministratorRole")]
public class CacheStatsController : ControllerBase
{
    private readonly ImageCacheService _cacheService;
    private readonly IConfigurationManager _configManager;
    private readonly ILogger<CacheStatsController> _logger;

    public CacheStatsController(
        ImageCacheService cacheService,
        IConfigurationManager configManager,
        ILogger<CacheStatsController> logger)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _configManager = configManager;
        _logger = logger;
    }

    [HttpGet("Stats")]
    [ProducesResponseType(typeof(CacheStatsDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public IActionResult GetStats()
    {
        try
        {
            var stats = _cacheService.GetStats();
            var config = _configManager.GetConfiguration<PluginConfiguration>("Irukandji");

            var totalSizeMb = stats.CurrentSizeBytes / (1024.0 * 1024.0);
            var totalHits = stats.HitCount + stats.MissCount;
            var ratio = totalHits > 0 ? (double)stats.HitCount / totalHits : 0.0;

            return Ok(new CacheStatsDto
            {
                TotalSizeMb = totalSizeMb,
                FileCount = stats.CurrentEntryCount,
                Hits = stats.HitCount,
                Misses = stats.MissCount,
                Ratio = ratio,
                MaxSizeMb = config?.MaxCacheSizeMb ?? 256
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cache stats");
            return StatusCode(500, "Internal server error");
        }
    }
}

/// <summary>
/// Stats endpoint response DTO.
/// Field names match the contract in Section 7 exactly.
/// </summary>
public class CacheStatsDto
{
    [Range(0, double.MaxValue)]
    public double TotalSizeMb { get; set; }

    [Range(0, long.MaxValue)]
    public long FileCount { get; set; }

    [Range(0, long.MaxValue)]
    public long Hits { get; set; }

    [Range(0, long.MaxValue)]
    public long Misses { get; set; }

    [Range(0, 1)]
    public double Ratio { get; set; }

    [Range(0, int.MaxValue)]
    public int MaxSizeMb { get; set; }
}
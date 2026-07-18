using Irukandji.Caching;
using Irukandji.Configuration;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Irukandji.Services;

/// <summary>
/// Populates the image cache at startup with common images:
/// - Home screen pool (744×496, 50 combined limit)
/// - Per-library pool (570×400, 100 limit per library)
/// 
/// Runs as a background task with configurable concurrency to avoid
/// overwhelming low-core NAS hardware.
/// </summary>
public class PrewarmHostedService : IHostedService
{
    private readonly ImageCacheService _cacheService;
    private readonly IConfigurationManager _configManager;
    private readonly ILogger<PrewarmHostedService> _logger;

    // Hardcoded limits (from spec Section 1.1)
    private const int HomeScreenPrewarmLimit = 50;
    private const int LibraryPrewarmLimitPerLibrary = 100;
    private const int PrewarmConcurrency = 3; // Throttle to avoid NAS overload

    public PrewarmHostedService(
        ImageCacheService cacheService,
        IConfigurationManager configManager,
        ILogger<PrewarmHostedService> logger)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _configManager = configManager;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Kick off prewarm as a detached background task — don't await it directly
        // so server startup isn't delayed
        _ = Task.Run(() => PrewarmCacheAsync(cancellationToken), cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task PrewarmCacheAsync(CancellationToken cancellationToken)
    {
        try
        {
            var config = _configManager.GetConfiguration<PluginConfiguration>("Irukandji");
            if (config == null || !config.EnableCache || !config.EnablePreWarm)
            {
                return;
            }

            _logger.LogInformation("Starting image cache prewarm...");

            using (var semaphore = new SemaphoreSlim(PrewarmConcurrency, PrewarmConcurrency))
            {
                // Step 1: Home screen pool (744×496, limit 50 combined)
                await PrewarmHomeScreenAsync(semaphore, config, cancellationToken);

                // Step 2: Per-library pools (570×400, limit 100 per library)
                await PrewarmLibrariesAsync(semaphore, config, cancellationToken);
            }

            _logger.LogInformation("Image cache prewarm completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cache prewarm");
        }
    }

    private async Task PrewarmHomeScreenAsync(
        SemaphoreSlim semaphore,
        PluginConfiguration config,
        CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Section 5 Step 1
            // - Reuse Jellyfin IUserViewManager/ILibraryManager to get home-screen items
            // - Resize/cache up to HomeScreenPrewarmLimit (50) images at 744×496, config.TargetQuality
            // - Log total count when complete
            _logger.LogInformation("Home screen prewarm: processed 0 images");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Home screen prewarm failed");
        }
    }

    private async Task PrewarmLibrariesAsync(
        SemaphoreSlim semaphore,
        PluginConfiguration config,
        CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Section 5 Step 2
            // - Iterate each library's top-level items
            // - Resize/cache primary images up to LibraryPrewarmLimitPerLibrary (100) per library
            // - Size: 570×400, config.TargetQuality
            // - Log success/failure line AFTER each library completes
            _logger.LogInformation("Per-library prewarm: completed 0 libraries");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Per-library prewarm failed");
        }
    }
}
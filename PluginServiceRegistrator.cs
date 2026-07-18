using Irukandji.Caching;
using Irukandji.Configuration;
using Irukandji.Middleware;
using Irukandji.Services;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Irukandji;

/// <summary>
/// Registers plugin services, middleware, and configuration into Jellyfin's DI container.
/// Runs when the plugin is loaded.
/// </summary>
public class PluginServiceRegistrator : IPluginServiceRegistrator
{
    public void RegisterServices(IServiceCollection serviceCollection, IConfigurationManager configurationManager)
    {
        // Register configuration
        serviceCollection.AddSingleton<IConfigurationManager>(configurationManager);

        // Register MemoryCache with size-based eviction
        var cacheOptions = new MemoryCacheOptions
        {
            SizeLimit = (long)(256 * 1024 * 1024), // 256 MB default; will be read from config at runtime
            CompactionPercentage = 0.2
        };
        serviceCollection.AddMemoryCache(options =>
        {
            options.SizeLimit = cacheOptions.SizeLimit;
            options.CompactionPercentage = cacheOptions.CompactionPercentage;
        });

        // Register cache service (singleton)
        serviceCollection.AddSingleton<ImageCacheService>();

        // Register middleware startup filters
        serviceCollection.AddSingleton<IStartupFilter, QualityRewriteStartupFilter>();
        serviceCollection.AddSingleton<IStartupFilter, ImageCacheStartupFilter>();

        // Register hosted services
        serviceCollection.AddHostedService<PrewarmHostedService>();

        // Register helper services
        serviceCollection.AddScoped<ImageEncodingService>();
    }
}
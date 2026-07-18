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

public class PluginServiceRegistrator : IPluginServiceRegistrator
{
    public void RegisterServices(IServiceCollection serviceCollection, IConfigurationManager configurationManager)
    {
        serviceCollection.AddSingleton<IConfigurationManager>(configurationManager);

        var cacheOptions = new MemoryCacheOptions
        {
            SizeLimit = (long)(256 * 1024 * 1024),
            CompactionPercentage = 0.2
        };
        serviceCollection.AddMemoryCache(options =>
        {
            options.SizeLimit = cacheOptions.SizeLimit;
            options.CompactionPercentage = cacheOptions.CompactionPercentage;
        });

        serviceCollection.AddSingleton<ImageCacheService>();
        serviceCollection.AddSingleton<IStartupFilter, QualityRewriteStartupFilter>();
        serviceCollection.AddSingleton<IStartupFilter, ImageCacheStartupFilter>();
        serviceCollection.AddHostedService<PrewarmHostedService>();
    }
}
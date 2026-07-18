using Microsoft.AspNetCore.Builder;
using System;

namespace Irukandji.Middleware;

/// <summary>
/// Registers ImageCacheMiddleware in the ASP.NET Core pipeline.
/// Runs after authentication/authorization to safely serve cached bytes.
/// </summary>
public class ImageCacheStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.UseMiddleware<ImageCacheMiddleware>();
            next(app);
        };
    }
}
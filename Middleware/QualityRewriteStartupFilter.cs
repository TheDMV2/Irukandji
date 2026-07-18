using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Irukandji.Middleware;

/// <summary>
/// Registers QualityRewriteMiddleware in the ASP.NET Core pipeline.
/// Runs early, before the core ImageController parses the request.
/// </summary>
public class QualityRewriteStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.UseMiddleware<QualityRewriteMiddleware>();
            next(app);
        };
    }
}
using Microsoft.AspNetCore.Builder;
using System;

namespace Irukandji.Middleware;

/// <summary>
/// Registers QualityRewriteMiddleware in the ASP.NET Core pipeline.
/// Intercepts lossy image requests (JPEG, WebP) to rewrite quality parameter.
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
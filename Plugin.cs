using Irukandji.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using System;
using System.Collections.Generic;

namespace Irukandji;

public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    public override string Name => "Irukandji Image Optimizer";
    public override Guid Id => new Guid("f1e5c8d7-b9a2-4c3e-8f5d-2a1b7c9e3f6d");
    public override string Description =>
        "Overrides image quality for lossy formats and maintains an in-memory cache of resized images.";

    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    public static Plugin? Instance { get; private set; }

    public IEnumerable<PluginPageInfo> GetPages()
    {
        yield return new PluginPageInfo
        {
            Name = "Irukandji Settings",
            EmbeddedResourcePath = "Irukandji.Configuration.configPage.html"
        };
    }
}
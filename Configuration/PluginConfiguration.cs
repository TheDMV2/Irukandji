using Jellyfin.Data.Entities;
using MediaBrowser.Common.Configuration;
using System.Collections.Generic;

namespace Irukandji.Configuration;

/// <summary>
/// Plugin configuration schema for Irukandji Image Optimizer.
/// Maps to config saved in Jellyfin's configuration directory.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Master switch: enable/disable the entire cache subsystem.
    /// When false, cache reads, writes, and prewarm are no-ops.
    /// </summary>
    public bool EnableCache { get; set; } = false;

    /// <summary>
    /// Enable startup prewarming: populate cache with home-screen and library-top images at boot.
    /// Only meaningful when EnableCache is also true.
    /// </summary>
    public bool EnablePreWarm { get; set; } = false;

    /// <summary>
    /// Target quality for lossy image formats (JPEG and WebP).
    /// Rewritten on every lossy image request.
    /// Default 76 (outside the recommended 20-66 range by design).
    /// User may set 1-100; recommended range is 20-66.
    /// </summary>
    public int TargetQuality { get; set; } = 76;

    /// <summary>
    /// Maximum cache size in megabytes.
    /// Default 256 MB; Jellyfin suggests a value based on available memory at startup.
    /// Fully admin-editable; suggestion only applied if value remains at hardcoded default.
    /// </summary>
    public int MaxCacheSizeMb { get; set; } = 256;
}
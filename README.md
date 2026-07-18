# Irukandji Image Optimizer

A Jellyfin plugin for image quality control and in-memory caching.

## Overview

Irukandji Image Optimizer overrides hardcoded output-quality values on WebUI image requests for lossy formats (JPEG, WebP) and maintains an in-memory cache of resized/recompressed primary images to serve repeat requests without re-reading, resizing, or re-encoding from disk.

## Features

- **Quality Rewrite**: Override default JPEG/WebP quality with a configurable target value
- **In-Memory Cache**: Cache resized images for common dimensions (home screen, library pages)
- **Startup Prewarming**: Populate cache with frequently-accessed images at server boot
- **New-Item Caching**: Automatically cache images when new media is added
- **Admin Dashboard**: Configure cache behavior and monitor statistics in real time
- **Authorization-Aware**: Cache hits respect library access controls — no unauthorized access

## Building

Requirements:
- .NET 9 SDK
- Jellyfin 10.11.x

```bash
dotnet build
dotnet pack
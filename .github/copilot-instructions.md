# Plugin.Maui.MessagingCenter - Copilot Instructions

## Project Overview

This is a .NET MAUI plugin that provides a drop-in replacement for the deprecated .NET MAUI MessagingCenter. It is pure cross-platform (no platform-specific code).

## Architecture

Pure cross-platform - single `MessagingCenter.cs`, no platform files.

`IMessagingCenter`: `Send`/`Subscribe`/`Unsubscribe` with generics.

Key differences from the original MAUI MessagingCenter:
- Prevents duplicate subscriptions (throws `InvalidOperationException`)
- Uses weak references
- Thread-safe

## Code Conventions

### Namespace
All code uses: `Plugin.Maui.MessagingCenter`

### File Naming
- `*.shared.cs` - Cross-platform code
- `*.android.cs` - Android-specific code
- `*.macios.cs` - iOS/macOS-specific code
- `*.windows.cs` - Windows-specific code
- `*.net.cs` - Generic .NET fallback

### Standards
- File-scoped namespaces
- `camelCase` for private fields, `PascalCase` for public
- XML docs required on all public APIs
- Null-conditional operators for platform interop

## Building

```bash
dotnet build src/Plugin.Maui.MessagingCenter/Plugin.Maui.MessagingCenter.csproj -c Release
```

## When Making Changes
1. Ensure the plugin builds
2. If adding public API, update the interface
3. Update sample app and README

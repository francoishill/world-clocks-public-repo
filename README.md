# World Clocks

A small, simple Windows system tray app for keeping an eye on the time in a handful of places. Left-click the tray icon for a live-updating list of clocks; right-click it for settings.

Built for the common "which of my colleagues are awake right now" use case: give each clock a **title** (e.g. `Los Angeles`) and an optional **subtitle** (e.g. `Dave, Priya`) for the people who work there.

## Features

- **Lives in the tray** — no taskbar window, no clutter. Just an icon.
- **Live popup** — left-click shows every configured clock, updating every second, with the time zone's date shown as **Yesterday** / **Tomorrow** whenever it differs from your local date.
- **Settings window** (right-click → *Settings*) to add, remove, and reorder clocks.
  - Optional **Title** and **Subtitle** per clock.
  - Timezone picker is **searchable** — type a place name (`Tokyo`) or a UTC offset (`+09:00`) to filter.
  - Toggle between 12-hour and 24-hour time.
  - Toggle launching automatically at Windows sign-in.
- Settings are saved to `%AppData%\WorldClocks\settings.json` as soon as you click *Save & Close* — no separate "export" step needed.

## Requirements

- Windows 10 or 11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (for building/running from source)

## Running from source

```bash
dotnet run --project src/WorldClocks
```

On first run (no settings saved yet), the Settings window opens automatically so you have somewhere to add your first clock.

## Running the tests

```bash
dotnet test
```

## Publishing a portable .exe

This produces a single, self-contained `WorldClocks.exe` that runs on any Windows 10/11 x64 machine without the .NET runtime installed:

```bash
dotnet publish src/WorldClocks -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true
```

The output lands in `src/WorldClocks/bin/Release/net8.0-windows/win-x64/publish/`.

> New tray icons are placed in Windows' hidden "overflow" area (the `^` chevron) by default — that's normal Windows behavior for any app's first run, not specific to this one. Drag the icon out onto the visible taskbar row if you'd like it always shown.

## Project layout

```
src/
├── WorldClocks/            # The WPF tray app
│   ├── Models/              # ClockEntry, AppSettings — the persisted data shape
│   ├── Services/            # Settings (JSON) and Windows auto-start persistence
│   ├── ViewModels/          # MVVM view models (live clocks, settings editor)
│   ├── Views/               # Tray popup and Settings window XAML
│   └── IconRenderer.cs      # Draws the tray/window icon procedurally (no binary asset)
└── WorldClocks.Tests/       # xUnit tests for settings persistence and clock/date-display logic
```

Built with .NET / WPF, [H.NotifyIcon.Wpf](https://github.com/HavenDV/H.NotifyIcon) for the tray icon, and [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) for MVVM boilerplate.

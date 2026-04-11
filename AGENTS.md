# AGENTS

## Goal

Primary in-flight goal: add a **macOS desktop build via Mac Catalyst** for BrickController2.

Phase 1 definition of done:

- app launches on this Mac
- shared MAUI UI is usable on desktop
- Bluetooth LE flows work
- game controller input works

## Repository map

- `BrickController2.sln` - solution entry point
- `BrickController2/BrickController2/` - shared MAUI app, XAML UI, view models, business logic, database, device logic
- `BrickController2/BrickController2.iOS/` - Apple mobile bootstrap and Apple-specific services/handlers
- `BrickController2/BrickController2.Android/` - Android bootstrap and Android-specific services/handlers
- `.github/copilot/memory-bank/` - persistent shared context for agents working in this repo

Important shared abstractions live under `BrickController2/BrickController2/PlatformServices/`:

- `IBluetoothLEService`
- `IGameControllerService`
- `ISharedFileStorageService`
- `IBluetoothPermission`
- `IReadWriteExternalStoragePermission`
- `IInfraredService`

## Current architecture notes

- This is a **multi-project MAUI** solution, not a single-project MAUI app.
- `Directory.Build.props` sets `SingleProject=false` and `UseMaui=true`.
- The shared project already contains the application shell, XAML pages, DI wiring, business logic, and device management.
- iOS and Android currently register the same shared modules and provide platform-specific implementations for the platform-service interfaces.
- The preferred desktop path is to **reuse as much Apple-specific code as possible** and avoid changing Android unless required by shared refactors.

## Commands

Run from the repository root.

- Restore MAUI workloads: `dotnet workload restore`
- Build solution: `dotnet build BrickController2.sln --nologo`
- Build shared project only: `dotnet build BrickController2/BrickController2/BrickController2.csproj --nologo`
- Build Mac Catalyst target with local SDK: `DOTNET_ROOT="$HOME/.local-dotnet8" PATH="$DOTNET_ROOT:$PATH" dotnet build BrickController2/BrickController2.iOS/BrickController2.iOS.csproj -f net8.0-maccatalyst --nologo`
- Launch Mac Catalyst target with helper script: `./scripts/run-maccatalyst-local.sh`

If a command is newly verified or its behavior changes, update `build-and-run.md`.

Current machine note:

- The system `dotnet` install on the current Mac requires elevated permissions for workload installation, so a user-local SDK under `$HOME/.local-dotnet8` is the safer path.
- `xcodebuild -runFirstLaunch` was required once before Apple build tools worked correctly.

## Agent workflow

1. Read this file and the memory bank before making changes.
2. Keep work aligned with the current goal in `desktop-port.md`.
3. Update `worklog.md` after completing a meaningful milestone.
4. Update `desktop-port.md` when a technical decision or constraint changes.
5. Keep entries short and factual so they stay useful.

## Desktop-port guidance

- Prefer **Mac Catalyst** over inventing a separate desktop stack.
- Prefer sharing Apple bootstrap and service code over duplicating it.
- Treat **Bluetooth LE** and **game controller** support as first-class requirements for the port.
- Infrared can remain unsupported on desktop unless the product requirement changes.
- Preserve current Android and iOS behavior while refactoring.

## Validation expectations

- Validate the build state before and after meaningful changes.
- Record blockers that affect other agents in the memory bank.
- Do not leave silent TODOs in code without also recording the issue in `desktop-port.md` or `worklog.md`.

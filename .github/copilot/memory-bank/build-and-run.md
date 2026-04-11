# Build and run

## Repository root

`/Users/dzitkowskik/proj/github/brickcontroller2`

## Expected commands

- Restore MAUI workloads: `dotnet workload restore`
- Build solution: `dotnet build BrickController2.sln --nologo`
- Build shared project only: `dotnet build BrickController2/BrickController2/BrickController2.csproj --nologo`
- Build Apple desktop target: `dotnet build BrickController2/BrickController2.iOS/BrickController2.iOS.csproj -f net8.0-maccatalyst --nologo`
- Launch Apple desktop target: `./scripts/run-maccatalyst-local.sh`

## Current known state

- The machine has .NET SDKs installed.
- The default `dotnet` on this machine is SDK `10.0.102`.
- The machine did not initially have the required MAUI workloads installed.
- A baseline `dotnet build BrickController2.sln --nologo` failed with `NETSDK1147` because workloads for Android/iOS were missing.
- A `dotnet workload restore` attempt on this machine failed due missing elevated permissions, so full MAUI validation is currently blocked by environment setup.
- A `dotnet build BrickController2/BrickController2.iOS/BrickController2.iOS.csproj -f net8.0-maccatalyst --nologo` attempt is also blocked by the same missing workload state.
- The repo itself references MAUI `8.0.40`, so future environment work should also verify whether SDK selection needs to be pinned once a compatible MAUI workload setup is available.

## Verified local desktop path

- Local SDK `8.0.408` installed successfully under `$HOME/.local-dotnet8`.
- Apple workloads were restored successfully using that local SDK.
- `xcodebuild -runFirstLaunch` was required once to complete Xcode tool initialization.
- The Mac Catalyst target now builds successfully with:
  - `DOTNET_ROOT="$HOME/.local-dotnet8" PATH="$DOTNET_ROOT:$PATH" dotnet build BrickController2/BrickController2.iOS/BrickController2.iOS.csproj -f net8.0-maccatalyst --nologo`
- The resulting app bundle is:
  - `BrickController2/BrickController2.iOS/bin/Debug/net8.0-maccatalyst/maccatalyst-arm64/BrickController2.iOS.app`

## Current full-solution blockers

- Android project still needs a valid local Android SDK and JDK configuration (`XA5300`).
- iOS mobile target still needs valid Apple signing keys in the keychain.

## Notes for future updates

- When Mac Catalyst build commands are verified, add them here.
- If workload installation requires extra machine-specific steps, record them here.

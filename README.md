# BrickController 2

Cross platform mobile application for controlling your creations using a bluetooth gamepad.

## Supported platforms

- Android 5.0+
- iOS 11+
- macOS (Mac Catalyst)

## Supported receivers

- SBrick - both normal and plus (output only)
- BuWizz 1
- BuWizz 2
- BuWizz 3
- Lego PowerFunctions infrared receiver on Android devices having IR emitter
- Lego Powered-Up hub
- Lego Boost Hub
- Lego Technic Hub
- Lego WeDo 2.0 Smart Hub
- Lego Technic Move Hub (PLAYVM mode)
- Circuit Cubes

## Project details

BrickController 2 is a MAUI application and can be compiled using Visual Studio 2022 (Professional, Enterprise and Community Editions)
or Visual Studio for Mac.

The Apple project in this repository targets both:

- iOS
- Mac Catalyst (macOS desktop build)

To build the full solution from the command line, install the required MAUI workloads first:

```bash
dotnet workload restore
dotnet build BrickController2.sln --nologo
```

## Running the macOS desktop build on this machine

The most reliable local path is a user-local .NET 8 SDK plus the Mac Catalyst workload:

```bash
curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
bash /tmp/dotnet-install.sh --version 8.0.408 --install-dir "$HOME/.local-dotnet8" --no-path
export DOTNET_ROOT="$HOME/.local-dotnet8"
export PATH="$DOTNET_ROOT:$PATH"
dotnet workload restore BrickController2/BrickController2.iOS/BrickController2.iOS.csproj
xcodebuild -runFirstLaunch
./scripts/run-maccatalyst-local.sh
```

The Mac Catalyst app bundle is produced at:

`BrickController2/BrickController2.iOS/bin/Debug/net8.0-maccatalyst/maccatalyst-arm64/BrickController2.iOS.app`

## 3rd party libraries used

- Autofac IOC container
- [SQLite-Net-Extensions Async](https://bitbucket.org/twincoders/sqlite-net-extensions)

## Author

István Murvai

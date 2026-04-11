# Desktop port

## Confirmed scope

- Target platform: **macOS only**
- Desktop technology: **Mac Catalyst**
- Phase 1 goal: **usable desktop app with Bluetooth and gamepad support**

## Current implementation direction

1. Set up repo-level agent collaboration context.
2. Prepare the MAUI toolchain on this Mac.
3. Add a Mac Catalyst desktop target for the Apple app path.
4. Share/refactor Apple bootstrap and platform-service code.
5. Close Mac Catalyst gaps for Bluetooth, game controller, storage, permissions, and UI behavior.

## Key technical observations

- The solution already uses shared MAUI UI and logic.
- The desktop port should minimize changes to Android.
- Current chosen bootstrap shape: **multi-target the existing Apple project** before considering a separate desktop launcher.
- The biggest unknown is how much of the current iOS CoreBluetooth and GameController code works unchanged on Mac Catalyst.
- Infrared is already unsupported on iOS and can remain unsupported on desktop unless requirements change.
- Full MAUI build validation is currently blocked on this machine because workload installation needs elevated permissions.
- Because the repo uses MAUI 8 packages and this machine defaults to .NET SDK 10, future environment work may also need SDK pinning or an explicitly compatible SDK path.

## Implemented so far

- Added `AGENTS.md` and `.github/copilot/memory-bank/`.
- Multi-targeted the Apple project for `net8.0-ios` and `net8.0-maccatalyst`.
- Added a Mac Catalyst-specific `Info.maccatalyst.plist`.
- Removed the legacy iOS `LaunchScreen.storyboard` from the Mac Catalyst target to avoid `ibtool` platform issues.
- Added `scripts/run-maccatalyst-local.sh` to build and launch the desktop app with a local .NET 8 SDK.
- Verified that the Mac Catalyst app builds and launches on this Mac.

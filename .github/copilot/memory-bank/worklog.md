# Worklog

## 2026-04-11

- Audited the repository structure and confirmed it is a multi-project .NET MAUI solution with shared UI/business logic and separate Android/iOS launchers.
- Confirmed the desktop target for this effort is macOS via Mac Catalyst.
- Confirmed phase 1 requires a usable desktop app with Bluetooth and gamepad support.
- Verified there was no existing `AGENTS.md`, `.github/`, or memory bank scaffolding.
- Verified the current machine is missing the MAUI workloads required for a full solution build.
- Added repo-level agent context via `AGENTS.md` and `.github/copilot/memory-bank/`.
- Switched the Apple project to target both `net8.0-ios` and `net8.0-maccatalyst`.
- Chose the multi-target Apple-project approach as the first desktop bootstrap path.
- Confirmed that local validation is currently blocked by MAUI workload installation permissions on this machine.
- Recorded that the repo uses MAUI 8 packages while this machine defaults to .NET SDK 10, which may matter once workload installation is unblocked.
- Installed a user-local .NET 8 SDK under `$HOME/.local-dotnet8` and restored the Apple workloads there.
- Ran `xcodebuild -runFirstLaunch` to finish Xcode setup on this Mac.
- Fixed the Mac Catalyst minimum-version mapping by splitting out `Info.maccatalyst.plist` and targeting Mac Catalyst `14.0`.
- Excluded `LaunchScreen.storyboard` from the Mac Catalyst target to avoid an `ibtool` dependency on the missing iOS 18.2 platform package.
- Verified that the Mac Catalyst target builds successfully and that the generated `.app` process starts on this Mac.
- Confirmed that full solution build is still blocked by unrelated mobile-environment prerequisites: Android SDK/JDK setup and iOS signing keys.

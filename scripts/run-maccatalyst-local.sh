#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.local-dotnet8}"

if [[ ! -x "$DOTNET_ROOT/dotnet" ]]; then
  echo "Missing local .NET 8 SDK at: $DOTNET_ROOT" >&2
  echo "Install it with:" >&2
  echo "  curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh" >&2
  echo "  bash /tmp/dotnet-install.sh --version 8.0.408 --install-dir \"$DOTNET_ROOT\" --no-path" >&2
  exit 1
fi

export DOTNET_ROOT
export PATH="$DOTNET_ROOT:$PATH"

cd "$REPO_ROOT"

dotnet build BrickController2/BrickController2.iOS/BrickController2.iOS.csproj -f net8.0-maccatalyst --nologo

open "$REPO_ROOT/BrickController2/BrickController2.iOS/bin/Debug/net8.0-maccatalyst/maccatalyst-arm64/BrickController2.iOS.app"

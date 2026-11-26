#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
command -v dotnet >/dev/null || { echo "BLOCKED: .NET 8 SDK is required" >&2; exit 2; }
command -v git >/dev/null || { echo "BLOCKED: git is required" >&2; exit 2; }
dotnet --version
work="$(mktemp -d)"
trap 'rm -rf "$work"' EXIT
git clone --quiet https://github.com/asgmodel/WasmAI.AutoGenerator.API.git "$work/auto"
git -C "$work/auto" checkout --quiet 4ef638f1d1c6209d633f76e1d5bc6b8e309dff02
git clone --quiet https://github.com/asgmodel/WasmAI.ConditionChecker.git "$work/checker"
git -C "$work/checker" checkout --quiet 8cba67f2d8b07a1cb6595b7d88317f0488186c6c
# Build in disposable directory; shared examples are linked from a temporary copy.
mkdir -p "$work/project"
cp -R "$root/tests" "$root/skills" "$work/project/"
project="$work/project/tests/ValidatorSmoke/ValidatorSmoke.csproj"
status=0
for mode in source package; do
  published=false
  if [[ "$mode" == package ]]; then published=true; fi
  echo "=== Checker target: $mode ==="
  if dotnet build "$project" --configuration Release --output "$work/out-$mode" \
    -p:AutoGeneratorSource="$work/auto" -p:CheckerSource="$work/checker" -p:UsePublishedChecker="$published" \
    && dotnet "$work/out-$mode/ValidatorSmoke.dll"; then
    echo "PASS: $mode"
  else
    echo "FAIL OR BLOCKED: $mode" >&2
    status=1
  fi
done
exit "$status"

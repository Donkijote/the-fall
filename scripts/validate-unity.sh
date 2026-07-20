#!/usr/bin/env bash
set -euo pipefail

task_mode="${1:-all}"
task_platform="${2:-macos}"
task_script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
task_project_root="$(cd "$task_script_dir/.." && pwd)"
task_project_version="$(sed -n 's/^m_EditorVersion: //p' "$task_project_root/ProjectSettings/ProjectVersion.txt")"
task_default_unity_editor="/Applications/Unity/Hub/Editor/$task_project_version/Unity.app/Contents/MacOS/Unity"
task_unity_editor="${UNITY_THE_FALL:-$task_default_unity_editor}"
export THE_FALL_BUILD_ROOT="${THE_FALL_BUILD_ROOT:-$task_project_root/Build/Smoke}"

usage() {
  echo "Usage: scripts/validate-unity.sh [tests|smoke|all] [macos|windows|android|ios]"
}

if [[ ! -x "$task_unity_editor" ]]; then
  echo "Unity editor is not executable at: $task_unity_editor" >&2
  echo "Set UNITY_THE_FALL to the executable matching ProjectSettings/ProjectVersion.txt." >&2
  exit 2
fi

mkdir -p "$task_project_root/Logs" "$THE_FALL_BUILD_ROOT"

run_unity() {
  local task_label="$1"
  local task_log="$2"
  shift 2

  echo "Running $task_label..."
  if ! "$task_unity_editor" "$@" -logFile "$task_log"; then
    echo "$task_label failed. Last 120 log lines:" >&2
    tail -n 120 "$task_log" >&2 || true
    return 1
  fi
  echo "$task_label passed. Log: $task_log"
}

run_tests() {
  run_unity \
    "foundation validation" \
    "$task_project_root/Logs/FoundationValidation.log" \
    -batchmode -nographics -quit \
    -projectPath "$task_project_root" \
    -executeMethod TheFall.Editor.FoundationSetup.Validate

  run_unity \
    "Edit Mode tests" \
    "$task_project_root/Logs/EditModeTests.log" \
    -batchmode -nographics \
    -projectPath "$task_project_root" \
    -runTests -testPlatform EditMode \
    -testResults "$task_project_root/Logs/EditModeResults.xml"

  run_unity \
    "Play Mode tests" \
    "$task_project_root/Logs/PlayModeTests.log" \
    -batchmode -nographics \
    -projectPath "$task_project_root" \
    -runTests -testPlatform PlayMode \
    -testResults "$task_project_root/Logs/PlayModeResults.xml"
}

run_smoke_build() {
  local task_build_target
  local task_build_method

  case "$task_platform" in
    macos)
      task_build_target="StandaloneOSX"
      task_build_method="TheFall.Editor.PlatformBuildSmoke.BuildMacOS"
      ;;
    windows)
      task_build_target="StandaloneWindows64"
      task_build_method="TheFall.Editor.PlatformBuildSmoke.BuildWindows"
      ;;
    android)
      task_build_target="Android"
      task_build_method="TheFall.Editor.PlatformBuildSmoke.BuildAndroid"
      ;;
    ios)
      task_build_target="iOS"
      task_build_method="TheFall.Editor.PlatformBuildSmoke.BuildIOS"
      ;;
    *)
      echo "Unsupported platform: $task_platform" >&2
      usage >&2
      exit 2
      ;;
  esac

  run_unity \
    "$task_platform build smoke" \
    "$task_project_root/Logs/BuildSmoke-${task_platform}.log" \
    -batchmode -nographics -quit \
    -projectPath "$task_project_root" \
    -buildTarget "$task_build_target" \
    -executeMethod "$task_build_method"
}

case "$task_mode" in
  tests)
    run_tests
    ;;
  smoke)
    run_smoke_build
    ;;
  all)
    run_tests
    run_smoke_build
    ;;
  *)
    echo "Unsupported validation mode: $task_mode" >&2
    usage >&2
    exit 2
    ;;
esac

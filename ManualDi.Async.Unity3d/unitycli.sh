#!/usr/bin/env bash
set -u

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

detect_platform() {
  case "$(uname -s 2>/dev/null || true)" in
    Darwin) echo "macos" ;;
    Linux) echo "linux" ;;
    MINGW*|MSYS*|CYGWIN*) echo "windows" ;;
    *) echo "unknown" ;;
  esac
}

PLATFORM="$(detect_platform)"
if [ "$PLATFORM" = "unknown" ]; then
  echo "Error: Unsupported host platform. Run unitycli.sh from Bash on Linux, macOS, or Windows Git Bash." >&2
  exit 1
fi

is_windows_platform() {
  [ "$PLATFORM" = "windows" ]
}

to_shell_path() {
  local path="$1"
  if is_windows_platform && command -v cygpath >/dev/null 2>&1; then
    cygpath -u "$path" 2>/dev/null || printf '%s\n' "$path"
  else
    printf '%s\n' "$path"
  fi
}

to_native_path() {
  local path="$1"
  if is_windows_platform && command -v cygpath >/dev/null 2>&1; then
    cygpath -m -a "$path" 2>/dev/null || printf '%s\n' "$path"
  else
    printf '%s\n' "$path"
  fi
}

PROJECT_NATIVE_ROOT="$(to_native_path "$PROJECT_ROOT")"
PROJECT_NATIVE_RESOLVE_LOG="$(to_native_path "$PROJECT_ROOT/Temp/unity_package_resolve.log")"

find_unity_path() {
  local configured_path="${UNITY_PATH:-${UNITY_EDITOR:-}}"
  if [ -n "$configured_path" ]; then
    local shell_configured_path=""
    shell_configured_path=$(to_shell_path "$configured_path" | tr -d '\r')
    if [ -f "$shell_configured_path" ]; then
      printf '%s\n' "$shell_configured_path"
      return 0
    fi
  fi

  if [ "${USE_UNITY_EDITOR_WRAPPER:-false}" = "true" ]; then
    local container_unity=""
    container_unity=$(command -v unity-editor 2>/dev/null | tr -d '\r')
    if [ -n "$container_unity" ]; then
      echo "$container_unity"
      return 0
    fi
  fi

  local version=""
  if [ -f "$PROJECT_ROOT/ProjectSettings/ProjectVersion.txt" ]; then
    version=$(grep "m_EditorVersion:" "$PROJECT_ROOT/ProjectSettings/ProjectVersion.txt" | awk '{print $2}' | tr -d '\r')
  fi

  local paths=()
  if [ -n "$version" ]; then
    if is_windows_platform; then
      [ -n "${ProgramFiles:-}" ] && paths+=("$ProgramFiles/Unity/Hub/Editor/$version/Editor/Unity.exe")
      [ -n "${ProgramW6432:-}" ] && paths+=("$ProgramW6432/Unity/Hub/Editor/$version/Editor/Unity.exe")
      [ -n "${LOCALAPPDATA:-}" ] && paths+=("$LOCALAPPDATA/Unity/Hub/Editor/$version/Editor/Unity.exe")
      paths+=(
        "C:/Program Files/Unity/Hub/Editor/$version/Editor/Unity.exe"
        "C:/Program Files (x86)/Unity/Hub/Editor/$version/Editor/Unity.exe"
        "C:/Unity/Hub/Editor/$version/Editor/Unity.exe"
      )
    elif [ "$PLATFORM" = "macos" ]; then
      paths=(
        "/Applications/Unity/Hub/Editor/$version/Unity.app/Contents/MacOS/Unity"
        "$HOME/Unity/Hub/Editor/$version/Unity.app/Contents/MacOS/Unity"
      )
    else
      paths=(
        "$HOME/Unity/Hub/Editor/$version/Editor/Unity"
        "/opt/unity/Editor/Unity"
        "/opt/Unity/Editor/Unity"
      )
    fi
  fi

  for p in "${paths[@]}"; do
    local shell_path=""
    shell_path=$(to_shell_path "$p" | tr -d '\r')
    if [ -f "$shell_path" ]; then
      printf '%s\n' "$shell_path"
      return 0
    fi
  done

  local command_unity=""
  local cmd
  for cmd in unity-editor Unity Unity.exe unity.exe unity; do
    command_unity=$(command -v "$cmd" 2>/dev/null | tr -d '\r')
    if [ -n "$command_unity" ]; then
      printf '%s\n' "$command_unity"
      return 0
    fi
  done

  if is_windows_platform && command -v where.exe >/dev/null 2>&1; then
    command_unity=$(where.exe unity 2>/dev/null | head -n 1 | tr -d '\r')
    if [ -n "$command_unity" ]; then
      shell_path=$(to_shell_path "$command_unity" | tr -d '\r')
      if [ -f "$shell_path" ]; then
        printf '%s\n' "$shell_path"
        return 0
      fi
    fi
  fi

  return 1
}

# Find the actual script in Packages or Library/PackageCache
SCRIPT_PATH=""

find_script_path() {
  SCRIPT_PATH=""
  # 1. Check in Library/PackageCache
  # Sort to pick the latest version if multiple exist
  local candidates=("$PROJECT_ROOT"/Library/PackageCache/com.pereviader.unityclirunner@*/CLI~/unitycli.sh)
  if [ ${#candidates[@]} -gt 0 ] && [ -f "${candidates[0]}" ]; then
    SCRIPT_PATH="${candidates[${#candidates[@]}-1]}"
  # 2. Check in Packages (local/development package)
  elif [ -f "$PROJECT_ROOT/Packages/com.pereviader.unityclirunner/CLI~/unitycli.sh" ]; then
    SCRIPT_PATH="$PROJECT_ROOT/Packages/com.pereviader.unityclirunner/CLI~/unitycli.sh"
  # 3. Check Packages/manifest.json for local file references
  elif [ -f "$PROJECT_ROOT/Packages/manifest.json" ]; then
    local local_path
    local_path=$(grep -o '"com.pereviader.unityclirunner"[[:space:]]*:[[:space:]]*"file:[^"]*"' "$PROJECT_ROOT/Packages/manifest.json" | sed 's/.*"file:\([^"]*\)".*/\1/')
    if [ -n "$local_path" ]; then
      # Unity resolves "file:" paths relative to the Packages/ folder
      local resolved_local_path
      resolved_local_path=$(cd "$PROJECT_ROOT/Packages" && cd "$local_path" 2>/dev/null && pwd)
      if [ -n "$resolved_local_path" ] && [ -f "$resolved_local_path/CLI~/unitycli.sh" ]; then
        SCRIPT_PATH="$resolved_local_path/CLI~/unitycli.sh"
      fi
    fi
  fi
}

find_script_path

if [ -z "$SCRIPT_PATH" ] || [ ! -f "$SCRIPT_PATH" ]; then
  echo "com.pereviader.unityclirunner not found in Library/PackageCache. Initializing Unity project to resolve packages..."
  UNITY_EXE=$(find_unity_path || true)
  if [ -n "$UNITY_EXE" ]; then
    mkdir -p "$PROJECT_ROOT/Temp"
    "$UNITY_EXE" -batchmode -nographics -projectPath "$PROJECT_NATIVE_ROOT" -logFile "$PROJECT_NATIVE_RESOLVE_LOG" -quit >/dev/null 2>&1 || true
    find_script_path
  else
    version=""
    if [ -f "$PROJECT_ROOT/ProjectSettings/ProjectVersion.txt" ]; then
      version=$(grep "m_EditorVersion:" "$PROJECT_ROOT/ProjectSettings/ProjectVersion.txt" | awk '{print $2}' | tr -d '\r')
    fi
    echo "Warning: Could not locate Unity executable for Unity version ${version:-unknown} to initialize packages automatically." >&2
  fi
fi

if [ -z "$SCRIPT_PATH" ] || [ ! -f "$SCRIPT_PATH" ]; then
  echo "Error: Could not find com.pereviader.unityclirunner package script in Library/PackageCache, Packages, or manifest.json references." >&2
  if [ -f "$PROJECT_ROOT/Temp/unity_package_resolve.log" ]; then
    echo "Check '$PROJECT_ROOT/Temp/unity_package_resolve.log' for details on why Unity failed to resolve packages." >&2
  fi
  echo "Please make sure the package is installed in your Unity project." >&2
  exit 1
fi

export UNITY_CLI_PROJECT_ROOT="$PROJECT_ROOT"
exec bash "$SCRIPT_PATH" "$@"

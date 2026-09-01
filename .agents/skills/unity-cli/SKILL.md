---
name: unity-cli
description: Run Unity tests, trigger AssetDatabase refresh or full recompilation, evaluate dynamic C# in-memory, execute static C# methods, or manage background Unity instances.
---

# Unity CLI (`unitycli.sh`)

Interact with Unity via high-performance background TCP sockets for sub-second compilation checks, unit testing, dynamic C# evaluation, and method execution.

## General Rules
- **Working Directory**: Always run with the current working directory set to the Unity project root (containing `Assets/`, `ProjectSettings/`).
- **Execution & Shell Quoting**:
  - On Windows, run using Git Bash (`bash ./unitycli.sh ...`).
  - In C#, string literals require double quotes (`"..."`). Single quotes (`'...'`) in C# denote single `char` literals and cause `error CS1012`.
  - In Bash, wrap the snippet in single quotes `'...'` so inner double quotes are preserved (e.g. `'Debug.Log("Hi");'`).
  - In **PowerShell**, PowerShell strips inner double quotes unless escaped: use triple quotes (e.g. `'Debug.Log("""Hi""");'`) or the stop-parsing token `bash --% ./unitycli.sh eval 'Debug.Log("Hi");'`.
- **Auto-Start**: If Unity is not running, `refresh`, `recompile`, `test`, `executemethod`, and `eval` automatically start a headless background instance in batchmode first.
- **Do Not Timeout**: Never prematurely abort or time out running commands; wait for completion or user cancellation.
- **Exit Codes**: `0` = Success; `1` = Compilation error, test failure, runtime exception, or invalid arguments.

---

## Command Selection Matrix

| Intent | Command | Refreshes AssetDB? | Stops PlayMode? | Live State? |
| :--- | :--- | :---: | :---: | :---: |
| Check compilation after code/asset edits | `refresh` | Yes | Yes | No |
| Clean full script assembly rebuild | `recompile` | Yes (Clean) | Yes | No |
| Run unit / integration tests | `test` | Yes | Yes | No |
| Run reusable static C# method | `executemethod` | Yes | Yes | No |
| Live C# query / debug / mutation | `eval` | No | **No** | **Yes** |
| Manage background Editor instance | `start` / `stop` / `status` | N/A | N/A | N/A |

---

## Commands Reference

### 1. Compilation & Diagnostics (`refresh`, `recompile`)
```bash
bash ./unitycli.sh refresh     # Import pending changes, wait for compile, print diagnostics
bash ./unitycli.sh recompile   # Clear compiler cache, force full assembly rebuild
```
- Outputs compiler diagnostics in standard `file(line,col): error/warning CSxxxx: <message>` format.
- `refresh` succeeds (exit 0) on warnings; exits 1 on compiler errors.

### 2. Running Tests (`test`)
```bash
bash ./unitycli.sh test                                # Run both EditMode & PlayMode tests
bash ./unitycli.sh test --editmode                     # EditMode only
bash ./unitycli.sh test --playmode                     # PlayMode only
bash ./unitycli.sh test --editmode --filter "MyTest"   # Filter by name (substring/regex)
bash ./unitycli.sh test --playmode --category "Smoke"  # Filter by NUnit category (supports '!Category')
```
- Automatically triggers AssetDatabase refresh and compilation before execution.
- Exits 0 on success; exits 1 on test failures, compile errors, or if 0 tests match the filter.
- Prints summary counts and failed-test stack traces for each failed leaf test.

### 3. Static Method Invocation (`executemethod`)
```bash
bash ./unitycli.sh executemethod Namespace.Class.Method [args...]
bash ./unitycli.sh executemethod MyEditor.BuildTools.Build
bash ./unitycli.sh executemethod MyEditor.Generator.CreateGrid 10 20.5 "forest" true
bash ./unitycli.sh executemethod MyEditor.Config.Apply '{"difficulty":2,"enableCheats":false}'
```
- Invokes public or non-public static methods (`FullyQualifiedType.Method`). Overloads are matched by parameter count.
- Refreshes AssetDatabase, compiles scripts, and exits Play Mode before running.
- **Argument Conversions**:
  - Primitives & Numbers: `string`, `int`, `float`, `double`, `bool`, `long`, `uint`, `ulong`, `byte`, `sbyte`, `short`, `ushort`, `char`, `decimal`.
  - Types: `Guid`, Enums (by name or int), `Nullable<T>`.
  - Complex Objects/Structs: Deserialized via `JsonUtility.FromJson`.
- **Return Values**: Primitives and strings are printed directly; complex objects are serialized as JSON (`JsonUtility.ToJson`); `void` methods print `Unity Response: SUCCESS`.
- **Console Logs**: Console messages (`Debug.Log`, `Debug.LogWarning`, `Debug.LogError`, `Console.WriteLine`) emitted during execution are redirected to standard output.

### 4. Dynamic C# Evaluation (`eval`)
```bash
# Expressions & Property Queries
bash ./unitycli.sh eval "Application.unityVersion"
bash ./unitycli.sh eval "SceneManager.GetActiveScene().name"
bash ./unitycli.sh eval "1 + 1"

# Multi-statement Blocks (use explicit 'return')
bash ./unitycli.sh eval "var count = GameObject.FindObjectsOfType<Camera>().Length; return count;"

# Statements & Mutations (preserve double quotes for C# strings)
bash ./unitycli.sh eval 'Debug.Log("Hello from CLI");'
bash ./unitycli.sh eval 'new GameObject("TestObject")'
bash ./unitycli.sh eval 'GameObject.Find("Main Camera")'
```
- **Instant In-Memory Execution**: Compiles snippets dynamically via Roslyn without domain reloads or disk files.
- **Preserves Live State**: Runs directly against the active Editor or running Play Mode state without stopping Play Mode or refreshing assets. (Run `refresh` first if pending script changes need to be compiled).
- **Auto-Imports**: `System`, `System.Collections.Generic`, `System.Linq`, `System.Reflection`, `System.Text`, `UnityEngine`, `UnityEngine.SceneManagement`, `UnityEditor`, `UnityEditor.SceneManagement`.
- **Auto-Wrapping**: Automatically wraps expressions (`return (...)`), void statements (`...; return null;`), or explicit `return` blocks.
- **Formatted Output**: Primitives, Booleans, Strings, GameObjects (name, active, tag, layer, components), Components, and Collections/IEnumerables (up to 100 items) are formatted automatically.
- **Console Logs**: Console messages (`Debug.Log`, `Debug.LogWarning`, `Debug.LogError`, `Console.WriteLine`) emitted during evaluation are captured and printed to standard output.

### 5. Background Unity Management (`start`, `stop`, `status`)
```bash
bash ./unitycli.sh start batchmode     # Start headless background instance (blocks until ready)
bash ./unitycli.sh start interactive   # Start visible GUI Editor (blocks until ready)
bash ./unitycli.sh status              # Report: 'Status: Ready' | 'Status: Not Running' | 'Status: Running Unreachable'
bash ./unitycli.sh stop                # Safely stop background instance
```

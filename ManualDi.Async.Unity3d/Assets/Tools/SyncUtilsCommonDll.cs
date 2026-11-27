using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

public static class SyncUtilsCommonDll
{
    public static string RepositoryRootPath => Path.GetFullPath(Path.Combine(Application.dataPath, "../.."));
    private static string Script => Path.GetFullPath(Path.Combine(Application.dataPath, "../GenerateUnityPackage/GeneratePackage.sh"));

    [MenuItem("Tools/Sync Dll")]
    public static void Execute()
    {
        var processStartInfoResult = GetProcessStartInfo();

        using var process = Process.Start(processStartInfoResult);
        if (process is null)
        {
            throw new InvalidOperationException("For some reason, could not start process to SyncUtilsCommonDll");
        }

        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();

        process.WaitForExit();

        var stdout = stdoutTask.GetAwaiter().GetResult();
        var stderr = stderrTask.GetAwaiter().GetResult();

        if (process.ExitCode != 0)
        {
            var combinedOutput = string.IsNullOrEmpty(stderr) ? stdout : $"{stdout}\n{stderr}";
            throw new InvalidOperationException("SyncUtilsCommonDll Build failed\n" + combinedOutput);
        }

        AssetDatabase.Refresh();
        UnityEngine.Debug.Log("Build succeeded\n" + stdout);
    }

    static ProcessStartInfo GetProcessStartInfo()
    {
        var bashPath = ResolveBashPath();

        return new ProcessStartInfo
        {
            FileName = bashPath,
            Arguments = $"\"{Script.Replace('\\', '/')}\" --skip-unity3d",
            WorkingDirectory = RepositoryRootPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
    }

    static string ResolveBashPath()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (File.Exists("/bin/bash"))
            {
                return "/bin/bash";
            }

            if (File.Exists("/usr/bin/bash"))
            {
                return "/usr/bin/bash";
            }

            return "bash";
        }

        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

        var candidates = new[]
        {
            Path.Combine(programFiles, "Git", "bin", "bash.exe"),
            Path.Combine(programFiles, "Git", "usr", "bin", "bash.exe"),
            Path.Combine(programFilesX86, "Git", "bin", "bash.exe"),
        };

        foreach (var candidate in candidates)
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrEmpty(pathEnv))
        {
            foreach (var dir in pathEnv.Split(Path.PathSeparator))
            {
                if (string.IsNullOrWhiteSpace(dir))
                {
                    continue;
                }

                var candidate = Path.Combine(dir.Trim(), "bash.exe");
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        var gitBashFallback = Path.Combine(programFiles, "Git", "git-bash.exe");
        if (File.Exists(gitBashFallback))
        {
            return gitBashFallback;
        }

        throw new InvalidOperationException("Could not find bash executable. Ensure Git Bash is installed or bash is on PATH.");
    }
}

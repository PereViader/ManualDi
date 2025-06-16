#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class SyncUtilsCommonDll
{
    public static string RepositoryRootPath => Path.Combine(Application.dataPath, "../..");
    private static string Script => Path.Combine(Application.dataPath, "../GenerateUnityPackage/GeneratePackage.sh");

    [MenuItem("Tools/Sync Dll")]
    public static void Execute()
    {
        var processStartInfoResult = GetProcessStartInfo();

        using var process = Process.Start(processStartInfoResult);
        if (process is null)
        {
            throw new InvalidOperationException("For some reson, could not start process to SyncUtilsCommonDll");
        }

        process.WaitForExit();

        AssetDatabase.Refresh();

        var output = process.StandardOutput.ReadToEnd();
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException("SyncUtilsCommonDll Build failed\n" + output);
        }

        UnityEngine.Debug.Log("Build succeded\n " + output);
    }

    static ProcessStartInfo GetProcessStartInfo()
    {
        if (!File.Exists("C:/Program Files/Git/git-bash.exe"))
        {
            throw new InvalidOperationException("Could not find git bash at C:/Program Files/Git/git-bash.exe");
        }

        return new ProcessStartInfo
        {
            FileName = "C:/Program Files/Git/git-bash.exe",
            Arguments = Script + " --skip-unity3d",
            WorkingDirectory = RepositoryRootPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };
    }
}
#endif
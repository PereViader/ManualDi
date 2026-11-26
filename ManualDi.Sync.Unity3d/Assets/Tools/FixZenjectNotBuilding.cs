using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

public static class FixZenjectNotBuilding
{
    public static async Task<bool> ApplyFixAsync()
    {
        var listRequest = Client.List(true, false);
        if (!listRequest.IsCompleted)
        {
            var tcs = new TaskCompletionSource<bool>();
            void CheckProgress()
            {
                if (!listRequest.IsCompleted)
                {
                    return;
                }

                EditorApplication.update -= CheckProgress;
                tcs.TrySetResult(true);
            }

            EditorApplication.update += CheckProgress;
            await tcs.Task;
        }

        if (listRequest.Status == StatusCode.Failure)
            throw new InvalidOperationException($"Failed to retrieve the Unity package list: {listRequest.Error?.message}");

        var packageInfo = listRequest.Result.FirstOrDefault(p => p.name == "com.svermeulen.extenject");
        if (packageInfo == null)
            throw new InvalidOperationException("Package 'com.svermeulen.extenject' was not found.");

        string packagePath = packageInfo.resolvedPath;
        string filePathToModify = Path.Combine(packagePath, "OptionalExtras/ReflectionBaking/Unity/UnityAssemblyResolver.cs");

        if (!File.Exists(filePathToModify))
            throw new FileNotFoundException("Could not find file to patch: " + filePathToModify, filePathToModify);

        var fileContent = File.ReadAllLines(filePathToModify).ToList();
        var fixLine = "                // @pere.viader fix";
        if (fileContent.Any(line => line.Contains(fixLine) || line.Contains("string.IsNullOrEmpty(assemblies[i].Location)")))
            return false; // Already applied

        const string anchor = "_appDomainAssemblyLocations[assemblies[i].FullName] = assemblies[i].Location;";
        var anchorIndex = fileContent.FindIndex(line => line.Contains(anchor));
        if (anchorIndex < 0)
        {
            throw new InvalidOperationException($"Could not find expected anchor line in {filePathToModify}");
        }

        var fix = new[]
        {
            fixLine,
            "                if (string.IsNullOrEmpty(assemblies[i].Location))",
            "                {",
            "                    continue;",
            "                }"
        };
        fileContent.InsertRange(anchorIndex, fix);

        File.WriteAllText(filePathToModify, string.Join('\n', fileContent));
        Debug.Log("Zenject fix applied successfully to: " + filePathToModify);
        AssetDatabase.Refresh();
        return true;
    }
}
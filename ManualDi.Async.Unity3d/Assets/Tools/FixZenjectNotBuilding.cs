using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

public static class FixZenjectNotBuilding
{
    public static bool ApplyFix()
    {
        var listRequest = Client.List(true, false);
        while (!listRequest.IsCompleted) { }

        if (listRequest.Status == StatusCode.Failure)
            throw new System.Exception("No se pudo obtener la lista de paquetes.");

        var packageInfo = listRequest.Result.FirstOrDefault(p => p.name == "com.svermeulen.extenject");
        if (packageInfo == null)
            throw new System.Exception("No se encontró el paquete 'com.svermeulen.extenject'.");

        string packagePath = packageInfo.resolvedPath;
        string filePathToModify = Path.Combine(packagePath, "OptionalExtras/ReflectionBaking/Unity/UnityAssemblyResolver.cs");

        if (!File.Exists(filePathToModify))
            throw new FileNotFoundException("No se pudo encontrar el archivo a modificar: " + filePathToModify);

        var fileContent = File.ReadAllLines(filePathToModify).ToList();
        var fixLine = "                // @pere.viader fix";
        if (fileContent.Contains(fixLine))
            return false; // Ya aplicado

        var fix = new[]
        {
            fixLine,
            "                if (string.IsNullOrEmpty(assemblies[i].Location))",
            "                {",
            "                    continue;",
            "                }"
        };
        fileContent.InsertRange(31, fix);

        File.WriteAllText(filePathToModify, string.Join('\n', fileContent));
        Debug.Log("Fix aplicado correctamente a: " + filePathToModify);
        AssetDatabase.Refresh();
        return true;
    }
}
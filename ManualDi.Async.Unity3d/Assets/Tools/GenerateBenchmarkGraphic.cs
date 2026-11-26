using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using Zenject.ReflectionBaking;

public static class GenerateBenchmarkGraphic
{
    public static string RepositoryRootPath => Path.Combine(Application.dataPath, "../..");

    private static void SetOptimized(bool state)
    {
        var path = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:ZenjectReflectionBakingSettings")[0]);
        var asset = AssetDatabase.LoadAssetAtPath<ZenjectReflectionBakingSettings>(path);
        
        typeof(ZenjectReflectionBakingSettings)
            .GetField("_isEnabledInBuilds", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(asset, state);
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
    }
    
    [MenuItem("Tools/Generate benchmark graphic")]
    public static async void Execute()
    {
        try
        {
            if (await FixZenjectNotBuilding.ApplyFixAsync())
            {
                UnityEngine.Debug.Log("Zenject fix applied, please re-run after recompilation is complete");
                return;
            }

            var testResults = await RunTest();

            var parsedResults = new[]
                {
                    "ManualDi.Async.Unity3d.Tests.ContainerPerformanceTest.ManualDi",
                    "ManualDi.Async.Unity3d.Tests.ContainerPerformanceTest.Reflex",
                    "ManualDi.Async.Unity3d.Tests.ContainerPerformanceTest.VContainer",
                    "ManualDi.Async.Unity3d.Tests.ContainerPerformanceTest.Zenject",
                }
                .Select(fullName =>
                {
                    var testResult = FindByName(testResults, fullName)
                        ?? throw new InvalidOperationException($"Could not find test result for '{fullName}'.");
                    return (Name: fullName.Split('.').Last(), Result: ParseMedianValuesWithRegex(testResult.Output));
                })
                .ToArray();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Container,Time,GC");
            foreach (var (name, (gcMedian, runtimeMedian)) in parsedResults)
            {
                sb.AppendLine($"{name},{runtimeMedian},{gcMedian}");
            }

            Debug.Log(sb.ToString());
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private static async Task<ITestResultAdaptor> RunTest()
    {
        var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
        var tcs = new TaskCompletionSource<ITestResultAdaptor>();
        var waitForTestComplete = new WaitForTestComplete(tcs);
        testRunnerApi.RegisterCallbacks(waitForTestComplete);

        try
        {
            var filter = new Filter()
            {
                testMode = TestMode.PlayMode,
                targetPlatform = BuildTarget.StandaloneWindows64,
            };
            testRunnerApi.Execute(new ExecutionSettings()
            {
                filters = new[] { filter },
            });

            return await tcs.Task;
        }
        finally
        {
            testRunnerApi.UnregisterCallbacks(waitForTestComplete);
            UnityEngine.Object.DestroyImmediate(testRunnerApi);
        }
    }

    public static (int GcMedian, int RuntimeMedian) ParseMedianValuesWithRegex(string data)
    {
        if (string.IsNullOrEmpty(data))
            return (0, 0);

        // Define the regular expression
        var regex = new Regex(
            @"\.GC\(\).*?Median:\s*(\d+),.*?" +
            @"Histogram.*?Median:\s*(\d+),",
            RegexOptions.Singleline
        );

        // Match the pattern against the input text
        Match match = regex.Match(data);

        int gcMedian = 0;
        int runtimeMedian = 0;

        if (match.Success)
        {
            // The first captured group is the GC Median
            if (match.Groups.Count > 1)
            {
                int.TryParse(match.Groups[1].Value, out gcMedian);
            }
            
            // The second captured group is the Runtime Median
            if (match.Groups.Count > 2)
            {
                int.TryParse(match.Groups[2].Value, out runtimeMedian);
            }
        }

        return (gcMedian, runtimeMedian);
    }
    
    private static ITestResultAdaptor FindByName(ITestResultAdaptor root, string name)
    {
        if (root == null)
            return null;

        if (root.FullName == name)
            return root;

        if (root.Children != null)
        {
            foreach (var child in root.Children)
            {
                var found = FindByName(child, name);
                if (found != null)
                    return found;
            }
        }
        return null;
    }
    
    private class WaitForTestComplete : ICallbacks
    {
        private readonly TaskCompletionSource<ITestResultAdaptor> tcs;

        public WaitForTestComplete(TaskCompletionSource<ITestResultAdaptor> tcs)
        {
            this.tcs = tcs;
        }

        public void RunStarted(ITestAdaptor testsToRun)
        {
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            tcs.TrySetResult(result);
        }

        public void TestStarted(ITestAdaptor test)
        {
        }

        public void TestFinished(ITestResultAdaptor result)
        {
        }
    }
}
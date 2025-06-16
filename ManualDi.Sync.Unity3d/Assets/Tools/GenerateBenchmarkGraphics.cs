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
        if (FixZenjectNotBuilding.ApplyFix())
        {
            Debug.Log("Zenject fix applied, please re-run after recompilation is complete");
            return;
        }
        
        var testResults = await RunTest();

        var parsedResults = new[]
            {
                "ManualDi.Sync.Unity3d.Tests.ContainerPerformanceTest.ManualDi",
                "ManualDi.Sync.Unity3d.Tests.ContainerPerformanceTest.Reflex",
                "ManualDi.Sync.Unity3d.Tests.ContainerPerformanceTest.VContainer",
                "ManualDi.Sync.Unity3d.Tests.ContainerPerformanceTest.Zenject",
            }
            .Select(x => (Name: x.Split(".").Last(), TestResult: FindByName(testResults, x)))
            .Select(x => (x.Name, ParseMedianValuesWithRegex(x.TestResult.Output)))
            .ToArray();
        
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Container,Time,GC");
        foreach (var (name, (gcMedian, runtimeMedian)) in parsedResults)
        {
            sb.AppendLine($"{name},{runtimeMedian},{gcMedian}");
        }
        
        Debug.Log(sb.ToString());
    }

    private static async Task<ITestResultAdaptor> RunTest()
    {
        var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
        var filter = new Filter()
        {
            testMode = TestMode.PlayMode,
            targetPlatform = BuildTarget.StandaloneWindows64,
        };
        testRunnerApi.Execute(new ExecutionSettings()
        {
            filters = new [] { filter },
        });
        
        var result = await WaitForTestComplete.Wait(testRunnerApi);
        return result;
    }

    public static (int GcMedian, int RuntimeMedian) ParseMedianValuesWithRegex(string data)
    {
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

        public static async Task<ITestResultAdaptor> Wait(TestRunnerApi api)
        {
            var tcs = new TaskCompletionSource<ITestResultAdaptor>();
            var waitForTestComplete = new WaitForTestComplete(tcs);
            api.RegisterCallbacks(waitForTestComplete);
            var result = await tcs.Task;
            api.UnregisterCallbacks(waitForTestComplete);
            return result;
        }
        
        public void RunStarted(ITestAdaptor testsToRun)
        {
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            tcs.SetResult(result);
        }

        public void TestStarted(ITestAdaptor test)
        {
        }

        public void TestFinished(ITestResultAdaptor result)
        {
        }
    }
}

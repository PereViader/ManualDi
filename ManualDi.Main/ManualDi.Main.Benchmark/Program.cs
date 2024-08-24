using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using ManualDi.Main.Benchmark;

BenchmarkRunner.Run<SimpleBenchmark>();

//Use it while developing
public class FastAndDirtyConfig : ManualConfig
{
    public FastAndDirtyConfig()
    {
        Add(DefaultConfig.Instance);
        AddJob(Job.Default
            .WithLaunchCount(1)
            .WithWarmupCount(1)
            .WithIterationCount(1)
        );
    }
}
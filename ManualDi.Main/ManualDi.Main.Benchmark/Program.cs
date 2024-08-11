using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<Benchmark>();

public class FastAndDirtyConfig : ManualConfig
{
    public FastAndDirtyConfig()
    {
        Add(DefaultConfig.Instance); // *** add default loggers, reporters etc? ***

        AddJob(Job.Default
            .WithLaunchCount(1)
            .WithWarmupCount(1)
            .WithIterationCount(1)
        );
    }
}
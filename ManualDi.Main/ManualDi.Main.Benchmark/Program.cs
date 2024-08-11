using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<Benchmark>(new FastAndDirtyConfig());
// var a = new Benchmark();
// a.ManualDi_Setup();
//
// int i = 0;

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
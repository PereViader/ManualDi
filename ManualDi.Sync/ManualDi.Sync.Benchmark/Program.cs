using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using ManualDi.Sync.Benchmark;

BenchmarkRunner.Run(typeof(Service1).Assembly, ManualConfig
    .Create(DefaultConfig.Instance)
    .WithOptions(ConfigOptions.JoinSummary)
    .WithOptions(ConfigOptions.DisableLogFile)
);
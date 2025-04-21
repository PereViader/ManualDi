using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using ManualDi.Async.Benchmark;

BenchmarkRunner.Run<Benchmark>(ManualConfig
    .Create(DefaultConfig.Instance)
    .WithOptions(ConfigOptions.JoinSummary)
    .WithOptions(ConfigOptions.DisableLogFile)
);
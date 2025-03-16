using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using ManualDi.Main.Benchmark;

BenchmarkRunner.Run<Benchmark>(ManualConfig
    .Create(DefaultConfig.Instance)
    .WithOptions(ConfigOptions.JoinSummary)
    .WithOptions(ConfigOptions.DisableLogFile)
);
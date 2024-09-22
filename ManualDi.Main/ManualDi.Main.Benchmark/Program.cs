using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using ManualDi.Main.Benchmark;

BenchmarkRunner.Run(typeof(Service1).Assembly, ManualConfig
    .Create(DefaultConfig.Instance)
    .WithOptions(ConfigOptions.JoinSummary)
    .WithOptions(ConfigOptions.DisableLogFile)
);
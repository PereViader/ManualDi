using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using ManualDi.Async.Benchmark;
using Perfolizer.Horology;
using Perfolizer.Metrology;

BenchmarkRunner.Run<Benchmark>(ManualConfig
    .Create(DefaultConfig.Instance)
    .WithSummaryStyle(new SummaryStyle(
        cultureInfo: System.Globalization.CultureInfo.InvariantCulture,
        printUnitsInHeader: true,
        sizeUnit: SizeUnit.KB,
        timeUnit: TimeUnit.Nanosecond,
        printZeroValuesInContent: true
    ))
    .WithOptions(ConfigOptions.JoinSummary)
    .WithOptions(ConfigOptions.DisableLogFile)
);
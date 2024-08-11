using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.dotTrace;
using ManualDi.Main;
using Microsoft.Extensions.DependencyInjection;

[DotTraceDiagnoser]
[SimpleJob, InProcess, MemoryDiagnoser]
public class Benchmark
{
    private ServiceProvider _microsoftDiContainer = default!;
    private IDiContainer _manualDiContainer = default!;

    [GlobalSetup]
    public void Setup()
    {
        SetupMicrosoft();
        SetupManualDi();
    }

    private void SetupMicrosoft()
    {
        _microsoftDiContainer = new ServiceCollection()
            .AddServices()
            .BuildServiceProvider();
    }

    private void SetupManualDi()
    {
        _manualDiContainer = new DiContainerBuilder()
            .Install(b =>
            {
                b.InstallServices();
            })
            .Build();
    }

    // [Benchmark]
    // public void MicrosoftDi_Setup()
    // {
    //     SetupMicrosoft();
    // }
    //
    // [Benchmark]
    // public void MicrosoftDi_Dispose()
    // {
    //     _microsoftDiContainer.Dispose();
    // }

    // [Benchmark]
    // public void MicrosoftDi_Resolve_ServiceC()
    // {
    //     _microsoftDiContainer.GetRequiredService<Service100>();
    // }

    [Benchmark]
    public void ManualDi_Resolve_ServiceC()
    {
        _manualDiContainer.Resolve<Service100>();
    }

    // [Benchmark]
    // public void ManualDi_Setup()
    // {
    //     SetupManualDi();
    // }
    //
    // [Benchmark]
    // public void ManualDi_Dispose()
    // {
    //     _manualDiContainer.Dispose();
    // }
}
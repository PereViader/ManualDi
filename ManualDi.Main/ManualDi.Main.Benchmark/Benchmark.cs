using BenchmarkDotNet.Attributes;
using ManualDi.Main;
using Microsoft.Extensions.DependencyInjection;

[BenchmarkDotNet.Diagnostics.dotTrace.DotTraceDiagnoser]
[MemoryDiagnoser]
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
        _manualDiContainer = new DiContainerBindings(bindingsCapacity: 100)
            .InstallServices()
            .Build();
    }

    [Benchmark]
    public void ManualDi_Setup()
    {
        SetupManualDi();
    }
    
    [Benchmark]
    public void MicrosoftDi_Setup()
    {
        SetupMicrosoft();
    }
    
    [Benchmark]
    public void ManualDi_Dispose()
    {
        _manualDiContainer.Dispose();
    }
    
    [Benchmark]
    public void MicrosoftDi_Dispose()
    {
        _microsoftDiContainer.Dispose();
    }

    [Benchmark]
    public void ManualDi_Resolve_Service()
    {
        _manualDiContainer.Resolve<Service100>();
    }
    
    [Benchmark]
    public void MicrosoftDi_Resolve_Service()
    {
        _microsoftDiContainer.GetRequiredService<Service100>();
    }
}
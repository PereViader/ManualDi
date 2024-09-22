using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace ManualDi.Main.Benchmark;

[BenchmarkDotNet.Diagnostics.dotTrace.DotTraceDiagnoser]
[MemoryDiagnoser]
public class SimpleBenchmark
{
    private ServiceProvider microsoftDiContainer = default!;
    private IDiContainer manualDiContainer = default!;
    
    [IterationSetup(Targets = new[] { nameof(MicrosoftDi_Resolve_Service) })]
    public void SetupMicrosoft()
    {
        microsoftDiContainer = new ServiceCollection()
            .AddServices()
            .BuildServiceProvider();
    }

    [IterationSetup(Targets = new [] { nameof(ManualDi_Resolve_Service)})]
    public void SetupManualDi()
    {
        manualDiContainer = new DiContainerBindings(bindingsCapacity: 100)
            .InstallServices()
            .Build();
    }

    #region Setup
    
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
    
    #endregion
    
    #region Service
    
    [Benchmark]
    public void ManualDi_Resolve_Service()
    {
        manualDiContainer.Resolve<Service100>();
    }
    
    [Benchmark]
    public void MicrosoftDi_Resolve_Service()
    {
        microsoftDiContainer.GetRequiredService<Service100>();
    }
    
    #endregion

    #region ServiceTwice

    [IterationSetup(Target = nameof(ManualDi_Resolve_ServiceTwice))]
    public void Setup_ManualDi_Resolve_ServiceTwice()
    {
        SetupManualDi();
        manualDiContainer.Resolve<Service100>();
    }
    
    [Benchmark]
    public void ManualDi_Resolve_ServiceTwice()
    {
        manualDiContainer.Resolve<Service100>();
    }
    
    [IterationSetup(Target = nameof(MicrosoftDi_Resolve_ServiceTwice))]
    public void Setup_MicrosoftDi_Resolve_ServiceTwice()
    {
        SetupMicrosoft();
        microsoftDiContainer.GetRequiredService<Service100>();
    }
    
    [Benchmark]
    public void MicrosoftDi_Resolve_ServiceTwice()
    {
        microsoftDiContainer.GetRequiredService<Service100>();
    }
    
    #endregion
}
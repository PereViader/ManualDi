using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace ManualDi.Main.Benchmark;

[MemoryDiagnoser]
public class Benchmark
{
    [Benchmark]
    public Service100 ManualDi()
    {
        var manualDiContainer = new DiContainerBindings(bindingsCapacity: 100)
            .InstallServices()
            .Build();
        
        return manualDiContainer.Resolve<Service100>();
    }
    
    [Benchmark]
    public Service100 MicrosoftDi()
    {
        var microsoftDiContainer = new ServiceCollection()
            .AddServices()
            .BuildServiceProvider();
        
        return microsoftDiContainer.GetRequiredService<Service100>();
    }
}
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using static ManualDi.Async.Benchmark.ManualDiInstallerExtensions;

namespace ManualDi.Async.Benchmark;

[MemoryDiagnoser]
public class Benchmark
{
    [Benchmark]
    public Service100 NoContainer()
    {
        return CreateWithoutContainer();
    }
    
    [Benchmark]
    public async Task<Service100> ManualDi()
    {
        var manualDiContainer = await new DiContainerBindings(bindingsCapacity: 100)
            .InstallServices()
            .Build(CancellationToken.None);
        
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
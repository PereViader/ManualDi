﻿using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace ManualDi.Main.Benchmark;

[MemoryDiagnoser]
public class BenchmarkMicrosoft
{
    private ServiceProvider microsoftDiContainer = default!;
    
    [Benchmark]
    [IterationSetup(Targets = [nameof(MicrosoftDi_Resolve)])]
    public void MicrosoftDi_Setup()
    {
        microsoftDiContainer = new ServiceCollection()
            .AddServices()
            .BuildServiceProvider();
    }
    
    [Benchmark]
    public void MicrosoftDi_Resolve()
    {
        microsoftDiContainer.GetRequiredService<Service100>();
    }
}

public static class ServiceCollectionExtensions
{
    public static ServiceCollection AddServices(this ServiceCollection serviceCollection)
    {
        serviceCollection
            .AddTransient<Service1>()
            .AddTransient<Service2>()
            .AddTransient<Service3>()
            .AddTransient<Service4>()
            .AddTransient<Service5>()
            .AddTransient<Service6>()
            .AddTransient<Service7>()
            .AddTransient<Service8>()
            .AddTransient<Service9>()
            .AddTransient<Service10>()
            .AddTransient<Service11>()
            .AddTransient<Service12>()
            .AddTransient<Service13>()
            .AddTransient<Service14>()
            .AddTransient<Service15>()
            .AddTransient<Service16>()
            .AddTransient<Service17>()
            .AddTransient<Service18>()
            .AddTransient<Service19>()
            .AddTransient<Service20>()
            .AddTransient<Service21>()
            .AddTransient<Service22>()
            .AddTransient<Service23>()
            .AddTransient<Service24>()
            .AddTransient<Service25>()
            .AddTransient<Service26>()
            .AddTransient<Service27>()
            .AddTransient<Service28>()
            .AddTransient<Service29>()
            .AddTransient<Service30>()
            .AddTransient<Service31>()
            .AddTransient<Service32>()
            .AddTransient<Service33>()
            .AddTransient<Service34>()
            .AddTransient<Service35>()
            .AddTransient<Service36>()
            .AddTransient<Service37>()
            .AddTransient<Service38>()
            .AddTransient<Service39>()
            .AddTransient<Service40>()
            .AddTransient<Service41>()
            .AddTransient<Service42>()
            .AddTransient<Service43>()
            .AddTransient<Service44>()
            .AddTransient<Service45>()
            .AddTransient<Service46>()
            .AddTransient<Service47>()
            .AddTransient<Service48>()
            .AddTransient<Service49>()
            .AddTransient<Service50>()
            .AddTransient<Service51>()
            .AddTransient<Service52>()
            .AddTransient<Service53>()
            .AddTransient<Service54>()
            .AddTransient<Service55>()
            .AddTransient<Service56>()
            .AddTransient<Service57>()
            .AddTransient<Service58>()
            .AddTransient<Service59>()
            .AddTransient<Service60>()
            .AddTransient<Service61>()
            .AddTransient<Service62>()
            .AddTransient<Service63>()
            .AddTransient<Service64>()
            .AddTransient<Service65>()
            .AddTransient<Service66>()
            .AddTransient<Service67>()
            .AddTransient<Service68>()
            .AddTransient<Service69>()
            .AddTransient<Service70>()
            .AddTransient<Service71>()
            .AddTransient<Service72>()
            .AddTransient<Service73>()
            .AddTransient<Service74>()
            .AddTransient<Service75>()
            .AddTransient<Service76>()
            .AddTransient<Service77>()
            .AddTransient<Service78>()
            .AddTransient<Service79>()
            .AddTransient<Service80>()
            .AddTransient<Service81>()
            .AddTransient<Service82>()
            .AddTransient<Service83>()
            .AddTransient<Service84>()
            .AddTransient<Service85>()
            .AddTransient<Service86>()
            .AddTransient<Service87>()
            .AddTransient<Service88>()
            .AddTransient<Service89>()
            .AddTransient<Service90>()
            .AddTransient<Service91>()
            .AddTransient<Service92>()
            .AddTransient<Service93>()
            .AddTransient<Service94>()
            .AddTransient<Service95>()
            .AddTransient<Service96>()
            .AddTransient<Service97>()
            .AddTransient<Service98>()
            .AddTransient<Service99>()
            .AddSingleton<Service100>();
        return serviceCollection;
    }
}
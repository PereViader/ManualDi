using BenchmarkDotNet.Attributes;

namespace ManualDi.Main.Benchmark;

[MemoryDiagnoser]
public class BenchmarkManualDi
{
    private IDiContainer manualDiContainer = default!;
    
    [Benchmark]
    [IterationSetup(Targets = [nameof(ManualDi_Resolve)])]
    public void ManualDi_Setup()
    {
        manualDiContainer = new DiContainerBindings(bindingsCapacity: 100)
            .InstallServices()
            .Build();
    }
    
    [Benchmark]
    public void ManualDi_Resolve()
    {
        manualDiContainer.Resolve<Service100>();
    }
}

public static class ManualDiInstallerExtensions
{
    public static DiContainerBindings InstallServices(this DiContainerBindings b)
    {
        b.Bind<Service1>().Default().Transient().FromConstructor();
        b.Bind<Service2>().Default().Transient().FromConstructor();
        b.Bind<Service3>().Default().Transient().FromConstructor();
        b.Bind<Service4>().Default().Transient().FromConstructor();
        b.Bind<Service5>().Default().Transient().FromConstructor();
        b.Bind<Service6>().Default().Transient().FromConstructor();
        b.Bind<Service7>().Default().Transient().FromConstructor();
        b.Bind<Service8>().Default().Transient().FromConstructor();
        b.Bind<Service9>().Default().Transient().FromConstructor();
        b.Bind<Service10>().Default().Transient().FromConstructor();
        b.Bind<Service11>().Default().Transient().FromConstructor();
        b.Bind<Service12>().Default().Transient().FromConstructor();
        b.Bind<Service13>().Default().Transient().FromConstructor();
        b.Bind<Service14>().Default().Transient().FromConstructor();
        b.Bind<Service15>().Default().Transient().FromConstructor();
        b.Bind<Service16>().Default().Transient().FromConstructor();
        b.Bind<Service17>().Default().Transient().FromConstructor();
        b.Bind<Service18>().Default().Transient().FromConstructor();
        b.Bind<Service19>().Default().Transient().FromConstructor();
        b.Bind<Service20>().Default().Transient().FromConstructor();
        b.Bind<Service21>().Default().Transient().FromConstructor();
        b.Bind<Service22>().Default().Transient().FromConstructor();
        b.Bind<Service23>().Default().Transient().FromConstructor();
        b.Bind<Service24>().Default().Transient().FromConstructor();
        b.Bind<Service25>().Default().Transient().FromConstructor();
        b.Bind<Service26>().Default().Transient().FromConstructor();
        b.Bind<Service27>().Default().Transient().FromConstructor();
        b.Bind<Service28>().Default().Transient().FromConstructor();
        b.Bind<Service29>().Default().Transient().FromConstructor();
        b.Bind<Service30>().Default().Transient().FromConstructor();
        b.Bind<Service31>().Default().Transient().FromConstructor();
        b.Bind<Service32>().Default().Transient().FromConstructor();
        b.Bind<Service33>().Default().Transient().FromConstructor();
        b.Bind<Service34>().Default().Transient().FromConstructor();
        b.Bind<Service35>().Default().Transient().FromConstructor();
        b.Bind<Service36>().Default().Transient().FromConstructor();
        b.Bind<Service37>().Default().Transient().FromConstructor();
        b.Bind<Service38>().Default().Transient().FromConstructor();
        b.Bind<Service39>().Default().Transient().FromConstructor();
        b.Bind<Service40>().Default().Transient().FromConstructor();
        b.Bind<Service41>().Default().Transient().FromConstructor();
        b.Bind<Service42>().Default().Transient().FromConstructor();
        b.Bind<Service43>().Default().Transient().FromConstructor();
        b.Bind<Service44>().Default().Transient().FromConstructor();
        b.Bind<Service45>().Default().Transient().FromConstructor();
        b.Bind<Service46>().Default().Transient().FromConstructor();
        b.Bind<Service47>().Default().Transient().FromConstructor();
        b.Bind<Service48>().Default().Transient().FromConstructor();
        b.Bind<Service49>().Default().Transient().FromConstructor();
        b.Bind<Service50>().Default().Transient().FromConstructor();
        b.Bind<Service51>().Default().Transient().FromConstructor();
        b.Bind<Service52>().Default().Transient().FromConstructor();
        b.Bind<Service53>().Default().Transient().FromConstructor();
        b.Bind<Service54>().Default().Transient().FromConstructor();
        b.Bind<Service55>().Default().Transient().FromConstructor();
        b.Bind<Service56>().Default().Transient().FromConstructor();
        b.Bind<Service57>().Default().Transient().FromConstructor();
        b.Bind<Service58>().Default().Transient().FromConstructor();
        b.Bind<Service59>().Default().Transient().FromConstructor();
        b.Bind<Service60>().Default().Transient().FromConstructor();
        b.Bind<Service61>().Default().Transient().FromConstructor();
        b.Bind<Service62>().Default().Transient().FromConstructor();
        b.Bind<Service63>().Default().Transient().FromConstructor();
        b.Bind<Service64>().Default().Transient().FromConstructor();
        b.Bind<Service65>().Default().Transient().FromConstructor();
        b.Bind<Service66>().Default().Transient().FromConstructor();
        b.Bind<Service67>().Default().Transient().FromConstructor();
        b.Bind<Service68>().Default().Transient().FromConstructor();
        b.Bind<Service69>().Default().Transient().FromConstructor();
        b.Bind<Service70>().Default().Transient().FromConstructor();
        b.Bind<Service71>().Default().Transient().FromConstructor();
        b.Bind<Service72>().Default().Transient().FromConstructor();
        b.Bind<Service73>().Default().Transient().FromConstructor();
        b.Bind<Service74>().Default().Transient().FromConstructor();
        b.Bind<Service75>().Default().Transient().FromConstructor();
        b.Bind<Service76>().Default().Transient().FromConstructor();
        b.Bind<Service77>().Default().Transient().FromConstructor();
        b.Bind<Service78>().Default().Transient().FromConstructor();
        b.Bind<Service79>().Default().Transient().FromConstructor();
        b.Bind<Service80>().Default().Transient().FromConstructor();
        b.Bind<Service81>().Default().Transient().FromConstructor();
        b.Bind<Service82>().Default().Transient().FromConstructor();
        b.Bind<Service83>().Default().Transient().FromConstructor();
        b.Bind<Service84>().Default().Transient().FromConstructor();
        b.Bind<Service85>().Default().Transient().FromConstructor();
        b.Bind<Service86>().Default().Transient().FromConstructor();
        b.Bind<Service87>().Default().Transient().FromConstructor();
        b.Bind<Service88>().Default().Transient().FromConstructor();
        b.Bind<Service89>().Default().Transient().FromConstructor();
        b.Bind<Service90>().Default().Transient().FromConstructor();
        b.Bind<Service91>().Default().Transient().FromConstructor();
        b.Bind<Service92>().Default().Transient().FromConstructor();
        b.Bind<Service93>().Default().Transient().FromConstructor();
        b.Bind<Service94>().Default().Transient().FromConstructor();
        b.Bind<Service95>().Default().Transient().FromConstructor();
        b.Bind<Service96>().Default().Transient().FromConstructor();
        b.Bind<Service97>().Default().Transient().FromConstructor();
        b.Bind<Service98>().Default().Transient().FromConstructor();
        b.Bind<Service99>().Default().Transient().FromConstructor();
        b.Bind<Service100>().Default().Single().FromConstructor();
        return b;
    }
}
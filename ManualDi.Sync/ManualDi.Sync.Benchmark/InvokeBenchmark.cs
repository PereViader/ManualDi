using BenchmarkDotNet.Attributes;
using System;
using ManualDi.Sync;

namespace ManualDi.Sync.Benchmark;

[MemoryDiagnoser]
public class InvokeBenchmark
{
    private IDiContainer container;
    private Action<int, string, IDiContainer> action;

    [GlobalSetup]
    public void Setup()
    {
        container = new DiContainerBindings()
            .Install(b =>
            {
                b.Bind<int>().FromInstance(42);
                b.Bind<string>().FromInstance("hello");
            })
            .Build();
        action = (i, s, c) => { };
    }

    [Benchmark]
    public object InvokeDelegate()
    {
        return container.InvokeDelegateUsingReflexion(action);
    }
}

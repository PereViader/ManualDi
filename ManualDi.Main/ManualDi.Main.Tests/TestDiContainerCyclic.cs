using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ManualDi.Main.Tests;

public class TestDiContainerCyclic
{
    private record A(B B);

    private class B
    {
        public void Inject(A _) { }
    }
    
    [Test]
    public async Task TestCyclic()
    {
        await using var container = await new DiContainerBindings().Install(b =>
        {
            b.Bind<B>()
                .FromMethod(c => new B())
                .Inject((o, c) => ((B)o).Inject(c.Resolve<A>()))
                .DependsOn(d => d.InjectionDependency<A>());
            
            b.Bind<A>()
                .FromMethod(c => new A(c.Resolve<B>()))
                .DependsOn(d => d.ConstructorDependency<B>());
        }).Build(CancellationToken.None);
    }
}
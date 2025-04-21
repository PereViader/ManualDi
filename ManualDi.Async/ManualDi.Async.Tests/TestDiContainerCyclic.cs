using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ManualDi.Async.Tests;

public class TestDiContainerCyclic
{
    private record A(B B);

    private class B
    {
        public void Inject(A _) { }
    }
    
    /// <summary>
    /// In this test, notice that B depends on A and A depends on B
    /// This is a cyclic dependency
    /// Because this is impossible to create, notice that B does not inject A on the constructor
    /// Instead it does it on a separate inject method
    /// This test should fail when the container is built but for some reason the dependencies are not
    /// created in the proper order
    /// </summary>
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
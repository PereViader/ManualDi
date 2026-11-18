using System;
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

    private record CyclicA(CyclicB B);
    private record CyclicB(CyclicA A);

    private record TransitiveA(TransitiveB B);
    private record TransitiveB(TransitiveC C);
    private record TransitiveC(TransitiveA A);

    private record NullableCyclicA(NullableCyclicB? B);
    private record NullableCyclicB(NullableCyclicA? A);
    
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

    [Test]
    public void TestDirectConstructorCycleThrows()
    {
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await using var container = await new DiContainerBindings().Install(b =>
            {
                b.Bind<CyclicA>()
                    .FromMethod(c => new CyclicA(c.Resolve<CyclicB>()))
                    .DependsOn(d => d.ConstructorDependency<CyclicB>());

                b.Bind<CyclicB>()
                    .FromMethod(c => new CyclicB(c.Resolve<CyclicA>()))
                    .DependsOn(d => d.ConstructorDependency<CyclicA>());
            }).Build(CancellationToken.None);
        });

        Assert.That(ex!.Message, Is.EqualTo($"Circular dependency detected when resolving type {typeof(CyclicA).FullName} from type {typeof(CyclicB).FullName}"));
    }

    [Test]
    public void TestTransitiveConstructorCycleThrows()
    {
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await using var container = await new DiContainerBindings().Install(b =>
            {
                b.Bind<TransitiveA>()
                    .FromMethod(c => new TransitiveA(c.Resolve<TransitiveB>()))
                    .DependsOn(d => d.ConstructorDependency<TransitiveB>());

                b.Bind<TransitiveB>()
                    .FromMethod(c => new TransitiveB(c.Resolve<TransitiveC>()))
                    .DependsOn(d => d.ConstructorDependency<TransitiveC>());

                b.Bind<TransitiveC>()
                    .FromMethod(c => new TransitiveC(c.Resolve<TransitiveA>()))
                    .DependsOn(d => d.ConstructorDependency<TransitiveA>());
            }).Build(CancellationToken.None);
        });

        Assert.That(ex!.Message, Is.EqualTo($"Circular dependency detected when resolving type {typeof(TransitiveA).FullName} from type {typeof(TransitiveC).FullName}"));
    }

    [Test]
    public void TestNullableConstructorCycleThrows()
    {
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await using var container = await new DiContainerBindings().Install(b =>
            {
                b.Bind<NullableCyclicA>()
                    .FromMethod(c => new NullableCyclicA(c.ResolveNullable<NullableCyclicB>()))
                    .DependsOn(d => d.NullableConstructorDependency<NullableCyclicB>());

                b.Bind<NullableCyclicB>()
                    .FromMethod(c => new NullableCyclicB(c.ResolveNullable<NullableCyclicA>()))
                    .DependsOn(d => d.NullableConstructorDependency<NullableCyclicA>());
            }).Build(CancellationToken.None);
        });

        Assert.That(ex!.Message, Is.EqualTo($"Circular dependency detected when resolving type {typeof(NullableCyclicA).FullName} from type {typeof(NullableCyclicB).FullName}"));
    }
}
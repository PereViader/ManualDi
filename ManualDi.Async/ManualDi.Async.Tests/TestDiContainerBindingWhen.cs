using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ManualDi.Async.Tests;

public class TestDiContainerBindingWhen
{
    private record NestedInt(int Value);

    [Test]
    public async Task TestWhenInjectedIntoType()
    {
        await using var container = await new DiContainerBindings().Install(b =>
        {
            b.Bind<NestedInt>().FromMethod(c => new NestedInt(c.Resolve<int>())).DependsOn(d => d.ConstructorDependency<int>());
            b.Bind<int>().FromInstance(1).When(x => x.InjectedIntoType<object>());
            b.Bind<int>().FromInstance(2).When(x => x.InjectedIntoType<NestedInt>());
        }).Build(CancellationToken.None);

        var nestedInt = container.Resolve<NestedInt>();
        Assert.That(nestedInt.Value, Is.EqualTo(2));
    }
    
    [Test]
    public async Task TestWhenInjectedIntoId()
    {
        await using var container = await new DiContainerBindings().Install(b =>
        {
            b.Bind<NestedInt>().FromMethod(c => new NestedInt(c.Resolve<int>())).DependsOn(d => d.ConstructorDependency<int>()).WithId("2");
            b.Bind<int>().FromInstance(1).When(x => x.InjectedIntoId("1"));
            b.Bind<int>().FromInstance(2).When(x => x.InjectedIntoId("2"));
        }).Build(CancellationToken.None);

        var nestedInt = container.Resolve<NestedInt>();
        Assert.That(nestedInt.Value, Is.EqualTo(2));
    }
    
    [Test]
    public async Task TestId()
    {
        var instance1 = new object();
        var instance2 = new object();

        await using var container = await new DiContainerBindings().Install(b =>
        {
            b.Bind<object>().FromInstance(instance1).WithId(nameof(instance1));
            b.Bind<object>().FromInstance(instance2).WithId(nameof(instance2));
        }).Build(CancellationToken.None);

        var resolution1 = container.Resolve<object>(static b => b.Id(nameof(instance1)));
        var resolution2 = container.Resolve<object>(static b => b.Id(nameof(instance2)));

        Assert.That(resolution1, Is.EqualTo(instance1));
        Assert.That(resolution2, Is.EqualTo(instance2));
    }
}
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ManualDi.Async.Tests;

public class TestDiContainerBindingWhen
{
    [ManualDi]
    internal class Special;
    [ManualDi]
    internal class Child;
    [ManualDi]
    internal class Root(Child child, Special special)
    {
        private readonly Child Child = child;
        private readonly Special Special = special;
    }

    private interface IInterface2;
    private interface IInterface1;

    [ManualDi]
    public class NestedInt(int value) : IInterface1, IInterface2
    {
        public int Value { get; } = value;
    }

    [Test]
    public async Task TestWhenInjectedIntoNested()
    {
        //This test checks that the Binding pointer is not lost
        //The Root depends on both Child and Special and special is the second dependency
        //If this succeeds, it means that the Root binding pointer is not lost
        await using var container = await new DiContainerBindings().Install(b =>
            {
                b.Bind<Special>().FromConstructor().When(x => x.InjectedIntoType<Root>());
                b.Bind<Root>().FromConstructor();
                b.Bind<Child>().FromConstructor();
            })
            .WithFailureDebugReport()
            .Build(CancellationToken.None);
    }

    [Test]
    public async Task TestWhenInjectedIntoType()
    {
        await using var container = await new DiContainerBindings().Install(b =>
        {
            b.Bind<NestedInt>().FromConstructor();
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
            b.Bind<NestedInt>().FromConstructor().WithId("2");
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

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task TestWhenInjectedIntoIdRedirectedBinding(bool isFirst)
    {
        var container = await new DiContainerBindings().Install(b =>
        {
            b.Bind<IInterface1, IInterface2, NestedInt>().FromMethod(c => new NestedInt(c.Resolve<int>())).WithId("2").DependsOn(d => d.ConstructorDependency<int>());
            b.Bind<int>().FromInstance(1).When(x => x.InjectedIntoId("1"));
            b.Bind<int>().FromInstance(2).When(x => x.InjectedIntoId("2"));
        }).Build(CancellationToken.None);

        var nestedInt = isFirst ? (NestedInt)container.Resolve<IInterface1>() : (NestedInt)container.Resolve<IInterface2>();
        Assert.That(nestedInt.Value, Is.EqualTo(2));
    }
}
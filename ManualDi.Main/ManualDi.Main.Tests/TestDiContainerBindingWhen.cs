using NUnit.Framework;

namespace ManualDi.Main.Tests;

public class TestDiContainerBindingWhen
{
    private record NestedInt(int Value);

    [Test]
    public void TestWhenInjectedIntoType()
    {
        var container = new DiContainerBindings().Install(b =>
        {
            b.Bind<NestedInt>().FromMethod(c => new NestedInt(c.Resolve<int>()));
            b.Bind<int>().FromInstance(1).When(x => x.InjectedIntoType<object>());
            b.Bind<int>().FromInstance(2).When(x => x.InjectedIntoType<NestedInt>());
        }).Build();

        var nestedInt = container.Resolve<NestedInt>();
        Assert.That(nestedInt.Value, Is.EqualTo(2));
    }
    
    [Test]
    public void TestWhenInjectedIntoId()
    {
        var container = new DiContainerBindings().Install(b =>
        {
            b.Bind<NestedInt>().FromMethod(c => new NestedInt(c.Resolve<int>())).WithId("2");
            b.Bind<int>().FromInstance(1).When(x => x.InjectedIntoId("1"));
            b.Bind<int>().FromInstance(2).When(x => x.InjectedIntoId("2"));
        }).Build();

        var nestedInt = container.Resolve<NestedInt>();
        Assert.That(nestedInt.Value, Is.EqualTo(2));
    }
}
using NUnit.Framework;

namespace ManualDi.Main.Tests;

public class TestDiContainerWouldResolve
{
    [Test]
    public void TestWouldResolveWithoutCondition()
    {
        var container = new DiContainerBindings().Install(b =>
        {
            b.Bind<int>().FromInstance(1);
        }).Build();

        Assert.That(container.WouldResolve<int>(), Is.True);
        Assert.That(container.WouldResolve<object>(), Is.False);
    }
    
    [Test]
    public void TestWouldResolveWithCondition()
    {
        var container = new DiContainerBindings().Install(b =>
        {
            b.Bind<object>().FromInstance(new()).WithId("3");
            b.Bind<int>().FromInstance(1).When(x => x.InjectedIntoId("3"));
        }).Build();

        Assert.That(container.WouldResolve<int>(), Is.False);
        Assert.That(container.WouldResolve<int, object>(), Is.True);
    }
}
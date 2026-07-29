using NUnit.Framework;

namespace ManualDi.Sync.Tests;

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
}
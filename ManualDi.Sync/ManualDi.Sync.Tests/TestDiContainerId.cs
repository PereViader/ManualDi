using NUnit.Framework;

namespace ManualDi.Sync.Tests;

public class TestDiContainerId
{
    [Test]
    public void TestId()
    {
        var instance1 = new object();
        var instance2 = new object();

        var container = new DiContainerBindings().Install(b =>
        {
            b.Bind<object>().FromInstance(instance1).WithId(nameof(instance1));
            b.Bind<object>().FromInstance(instance2).WithId(nameof(instance2));
        }).Build();

        var resolution1 = container.Resolve<object>(static b => b.Id(nameof(instance1)));
        var resolution2 = container.Resolve<object>(static b => b.Id(nameof(instance2)));

        Assert.That(resolution1, Is.EqualTo(instance1));
        Assert.That(resolution2, Is.EqualTo(instance2));
    }
}
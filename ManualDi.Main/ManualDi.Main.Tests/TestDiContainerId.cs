using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ManualDi.Main.Tests;

public class TestDiContainerId
{
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
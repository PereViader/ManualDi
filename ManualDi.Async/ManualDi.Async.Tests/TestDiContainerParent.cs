using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ManualDi.Async.Tests;

public class TestDiContainerParent
{
    [Test]
    public async Task TestResolve()
    {
        var instance = new object();

        await using var parentContainer = await new DiContainerBindings()
            .Install(b =>
            {
                b.Bind<object>().FromInstance(instance);
            })
            .Build(CancellationToken.None);

        await using var childContainer = await new DiContainerBindings()
            .WithParentContainer(parentContainer)
            .Build(CancellationToken.None);

        var resolution = childContainer.Resolve<object>();

        Assert.That(resolution, Is.EqualTo(instance));
    }

    [Test]
    public async Task TestResolveAll()
    {
        var instanceParent = new object();
        var instanceChild = new object();

        await using var parentContainer = await new DiContainerBindings()
            .Install(b =>
            {
                b.Bind<object>().FromInstance(instanceParent);
            })
            .Build(CancellationToken.None);

        await using var childContainer = await new DiContainerBindings()
            .WithParentContainer(parentContainer)
            .Install(b =>
            {
                b.Bind<object>().FromInstance(instanceChild);
            })
            .Build(CancellationToken.None);

        var resolution = childContainer.ResolveAll<object>();

        Assert.That(resolution, Is.EquivalentTo([instanceParent, instanceChild]));
    }
}
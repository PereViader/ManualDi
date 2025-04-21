using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ManualDi.Async.Tests;

public class TestDiContainerWouldResolve
{
    [Test]
    public async Task TestWouldResolveWithoutCondition()
    {
        await using var container = await new DiContainerBindings().Install(b =>
        {
            b.Bind<int>().FromInstance(1);
        }).Build(CancellationToken.None);

        Assert.That(container.WouldResolve<int>(), Is.True);
        Assert.That(container.WouldResolve<object>(), Is.False);
    }
    
    [Test]
    public async Task TestWouldResolveWithCondition()
    {
        await using var container = await new DiContainerBindings().Install(b =>
        {
            b.Bind<object>().FromInstance(new()).WithId("3");
            b.Bind<int>().FromInstance(1).When(x => x.InjectedIntoId("3"));
        }).Build(CancellationToken.None);

        Assert.That(container.WouldResolve<int>(), Is.False);
        Assert.That(container.WouldResolve<int, object>(), Is.True);
    }
}
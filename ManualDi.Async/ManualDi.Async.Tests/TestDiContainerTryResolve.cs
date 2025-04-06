using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ManualDi.Async.Tests;

public class TestDiContainerTryResolve
{
    [Test]
    public async Task TestTryResolveSuccess()
    {
        await using var container = await new DiContainerBindings().Install(b =>
        {
            b.Bind<int>().FromInstance(1);
        }).Build(CancellationToken.None);

        var success = container.TryResolve<int>(out var resolution);
        Assert.That(success, Is.True);
        Assert.That(resolution, Is.EqualTo(1));
    }

    [Test]
    public async Task TestTryResolveFailure()
    {
        await using var container = await new DiContainerBindings().Build(CancellationToken.None);

        var success = container.TryResolve<int>(out _);
        Assert.That(success, Is.False);
    }
}
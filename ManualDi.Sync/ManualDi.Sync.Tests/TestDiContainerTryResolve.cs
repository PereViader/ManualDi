using NUnit.Framework;

namespace ManualDi.Sync.Tests;

public class TestDiContainerTryResolve
{
    [Test]
    public void TestTryResolveSuccess()
    {
        var container = new DiContainerBindings().Install(b =>
        {
            b.Bind<int>().FromInstance(1);
        }).Build();

        var success = container.TryResolve<int>(out int resolution);
        Assert.That(success, Is.True);
        Assert.That(resolution, Is.EqualTo(1));
    }

    [Test]
    public void TestTryResolveFailure()
    {
        var container = new DiContainerBindings().Build();

        var success = container.TryResolve<int>(out _);
        Assert.That(success, Is.False);
    }
}
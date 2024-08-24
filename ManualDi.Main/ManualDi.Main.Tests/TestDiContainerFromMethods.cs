using NUnit.Framework;

namespace ManualDi.Main.Tests;

public class TestDiContainerFromMethods
{
    [Test]
    public void TestFromInstance()
    {
        var instance = new object();

        var container = new DiContainerBindings().Install(b =>
        {
            b.Bind<object>().FromInstance(instance);
        }).Build();

        var resolved = container.Resolve<object>();
        Assert.That(resolved, Is.EqualTo(instance));
    }

    [Test]
    public void TestFromMethod()
    {
        var instance = new object();
        var container = new DiContainerBindings().Install(b =>
        {
            b.Bind<object>().FromMethod(c => instance);
        }).Build();

        var resolved = container.Resolve<object>();
        Assert.That(resolved, Is.EqualTo(instance));
    }

    [Test]
    public void TestFromContainer()
    {
        int instance = 5;
        var container = new DiContainerBindings().Install(b =>
        {
            b.Bind<int>().FromInstance(instance);
            b.Bind<object, int>().FromContainer();
        }).Build();

        var resolved = container.Resolve<object>();
        Assert.That(resolved, Is.EqualTo(instance));
    }
}
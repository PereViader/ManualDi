using NUnit.Framework;

namespace ManualDi.Main.Tests;

public class TestDiContainerParent
{
    [Test]
    public void TestResolve()
    {
        var instance = new object();

        var parentContainer = new DiContainerBindings()
            .Install(b =>
            {
                b.Bind<object>().FromInstance(instance);
            })
            .Build();

        var childContainer = new DiContainerBindings()
            .WithParentContainer(parentContainer)
            .Build();

        var resolution = childContainer.Resolve<object>();

        Assert.That(resolution, Is.EqualTo(instance));
    }

    [Test]
    public void TestResolveAll()
    {
        var instanceParent = new object();
        var instanceChild = new object();

        var parentContainer = new DiContainerBindings()
            .Install(b =>
            {
                b.Bind<object>().FromInstance(instanceParent);
            })
            .Build();

        var childContainer = new DiContainerBindings()
            .WithParentContainer(parentContainer)
            .Install(b =>
            {
                b.Bind<object>().FromInstance(instanceChild);
            })
            .Build();

        var resolution = childContainer.ResolveAll<object>();

        Assert.That(resolution, Is.EquivalentTo(new [] { instanceParent, instanceChild }));
    }
}
using NUnit.Framework;
using System.Collections.Generic;

namespace ManualDi.Main.Tests;

public class TestDiContainerNonGenericResolve
{
    [Test]
    public void TestResolveWithoutConstraint()
    {
        var container = new DiContainerBuilder().Install(b =>
        {
            b.Bind<int>().FromInstance(5);
        }).Build();

        object resolved = container.Resolve(typeof(int));

        Assert.That(resolved, Is.EqualTo(5));
    }

    [Test]
    public void TestResolveWithConstraint()
    {
        var container = new DiContainerBuilder().Install(b =>
        {
            b.Bind<int>().FromInstance(2).WithMetadata("A");
            b.Bind<int>().FromInstance(5).WithMetadata("B");
        }).Build();

        object resolved = container.Resolve(typeof(int), x => x.WhereMetadata("B"));

        Assert.That(resolved, Is.EqualTo(5));
    }

    [Test]
    public void TestResolveAllWithoutConstraint()
    {
        var container = new DiContainerBuilder().Install(b =>
        {
            b.Bind<int>().FromInstance(2);
            b.Bind<int>().FromInstance(5);
        }).Build();

        List<object> resolved = container.ResolveAll(typeof(int));

        Assert.That(resolved, Is.EquivalentTo(new[] { 2, 5 }));
    }

    [Test]
    public void TestResolveAllWithConstraint()
    {
        var container = new DiContainerBuilder().Install(b =>
        {
            b.Bind<int>().FromInstance(2).WithMetadata("A");
            b.Bind<int>().FromInstance(5).WithMetadata("B");
        }).Build();

        List<object> resolved = container.ResolveAll(typeof(int), x => x.WhereMetadata("B"));

        Assert.That(resolved, Is.EquivalentTo(new[] { 5 }));
    }
}
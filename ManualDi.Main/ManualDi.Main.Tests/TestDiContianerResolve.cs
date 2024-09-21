﻿using NUnit.Framework;

namespace ManualDi.Main.Tests;

public class TestDiContianerTryResolve
{
    private record NestedInt(int Value);
    
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
    
    [Test]
    public void TestWhenInjectedIntoType()
    {
        var container = new DiContainerBindings().Install(b =>
        {
            b.Bind<NestedInt>().FromMethod(c => new NestedInt(c.Resolve<int>()));
            b.Bind<int>().FromInstance(1).When(x => x.InjectedIntoType<object>());
            b.Bind<int>().FromInstance(2).When(x => x.InjectedIntoType<NestedInt>());
        }).Build();

        var nestedInt = container.Resolve<NestedInt>();
        Assert.That(nestedInt.Value, Is.EqualTo(2));
    }
    
    [Test]
    public void TestWhenInjectedIntoId()
    {
        var container = new DiContainerBindings().Install(b =>
        {
            b.Bind<NestedInt>().FromMethod(c => new NestedInt(c.Resolve<int>())).WithId("2");
            b.Bind<int>().FromInstance(1).When(x => x.InjectedIntoId("1"));
            b.Bind<int>().FromInstance(2).When(x => x.InjectedIntoId("2"));
        }).Build();

        var nestedInt = container.Resolve<NestedInt>();
        Assert.That(nestedInt.Value, Is.EqualTo(2));
    }
}
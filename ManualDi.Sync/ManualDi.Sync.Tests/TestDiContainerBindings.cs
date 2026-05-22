using NSubstitute;
using NUnit.Framework;
using System;

namespace ManualDi.Sync.Tests;

public class TestDiContainerBindings
{
    private interface IInterface1;
    private interface IInterface2;

    private class Type : IInterface1, IInterface2;
    
    [Test]
    public void TestBindingReturnsInstanceAndCallsOnlyOnce()
    {
        int counter = 0;
        var container = new DiContainerBindings().Install(b =>
        {
            b.Bind<IInterface1, IInterface2, Type>()
                .FromMethod(c =>
                {
                    counter++;
                    return new Type();
                })
                .Inject(((o, c) => counter++))
                .Initialize((o, c) => counter++);
        }).Build();

        var instance1 = container.Resolve<IInterface1>();
        var instance2 = container.Resolve<IInterface2>();

        Assert.That(counter, Is.EqualTo(3));
        Assert.That(instance1, Is.EqualTo(instance2));
    }
    
    [Test]
    public void TestUnbind()
    {
        var container = new DiContainerBindings().Install(b =>
        {
            b.Bind<int>().FromInstance(1);
            b.Unbind<int>();
        }).Build();

        var resolved = container.ResolveNullableValue<int>();
        Assert.That(resolved.HasValue, Is.EqualTo(false));
    }
    
    [Test]
    public void TestQueueDispose()
    {
        var action = Substitute.For<Action>();

        var container = new DiContainerBindings()
            .Install(b => b.QueueDispose(action))
            .Build();

        action.DidNotReceive().Invoke();

        container.Dispose();

        action.Received(1).Invoke();
    }
    
    [Test]
    public void TestQueueInject()
    {
        var injectionDelegate = Substitute.For<ContainerDelegate>();

        var container = new DiContainerBindings()
            .Install(b => b.QueueInjection(injectionDelegate))
            .Build();

        injectionDelegate.Received(1).Invoke(Arg.Is<IDiContainer>(container));
    }

    [Test]
    public void TestQueueInitialization()
    {
        var initializationDelegate = Substitute.For<ContainerDelegate>();

        var container = new DiContainerBindings()
            .Install(b => b.QueueInitialization(initializationDelegate))
            .Build();

        initializationDelegate.Received(1).Invoke(Arg.Is<IDiContainer>(container));
    }
    
    [Test]
    public void TestQueueStartup()
    {
        var startupDelegate = Substitute.For<ContainerDelegate>();

        var container = new DiContainerBindings()
            .Install(b => b.QueueStartup(startupDelegate))
            .Build();

        startupDelegate.Received(1).Invoke(Arg.Is<IDiContainer>(container));
    }
    
    [Test]
    public void TestQueueOrder()
    {
        var injectionDelegate = Substitute.For<ContainerDelegate>();
        var initializationDelegate = Substitute.For<ContainerDelegate>();
        var startupDelegate = Substitute.For<ContainerDelegate>();
        var disposeDelegate = Substitute.For<Action>();

        var container = new DiContainerBindings()
            .Install(b =>
            {
                b.QueueInjection(injectionDelegate);
                b.QueueInitialization(initializationDelegate);
                b.QueueStartup(startupDelegate);
                b.QueueDispose(disposeDelegate);
            })
            .Build();
        
        container.Dispose();
        
        Received.InOrder(() =>
        {
            injectionDelegate.Invoke(Arg.Is<IDiContainer>(container));
            initializationDelegate.Invoke(Arg.Is<IDiContainer>(container));
            startupDelegate.Invoke(Arg.Is<IDiContainer>(container));
            disposeDelegate.Invoke();
        });
    }

    private class Other : IInterface2;

    [Test]
    public void TestMultipleApparentTypesDoesNotCrossWireBindings()
    {
        var container = new DiContainerBindings().Install(b =>
        {
            b.Bind<IInterface1, IInterface2, Type>().FromMethod(_ => new Type());
            b.Bind<IInterface2, Other>().FromMethod(_ => new Other());
        }).Build();

        var resolutions1 = container.ResolveAll<IInterface1>();
        var resolutions2 = container.ResolveAll<IInterface2>();

        Assert.That(resolutions1.Count, Is.EqualTo(1));
        Assert.That(resolutions1[0], Is.TypeOf<Type>());
        
        Assert.That(resolutions2.Count, Is.EqualTo(2));
        Assert.That(resolutions2[0], Is.TypeOf<Type>());
        Assert.That(resolutions2[1], Is.TypeOf<Other>());
    }
}
using NSubstitute;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ManualDi.Async.Tests;

public class TestDiContainerBindings
{
    private interface IInterface1;
    private interface IInterface2;

    private class Type : IInterface1, IInterface2;
    
    [Test]
    public async Task TestBindingReturnsInstanceAndCallsOnlyOnce()
    {
        int counter = 0;
        var container = await new DiContainerBindings().Install(b =>
        {
            b.Bind<IInterface1, IInterface2, Type>()
                .FromMethod(c =>
                {
                    counter++;
                    return new Type();
                })
                .Inject(((o, c) => counter++))
                .Initialize((o) => counter++);
        }).Build(CancellationToken.None);

        var instance1 = container.Resolve<IInterface1>();
        var instance2 = container.Resolve<IInterface2>();

        Assert.That(counter, Is.EqualTo(3));
        Assert.That(instance1, Is.EqualTo(instance2));
    }
    
    [Test]
    public async Task TestQueueDispose()
    {
        var action = Substitute.For<Action>();

        var container = await new DiContainerBindings()
            .Install(b => b.QueueDispose(action))
            .Build(CancellationToken.None);

        action.DidNotReceive().Invoke();

        await container.DisposeAsync();

        action.Received(1).Invoke();
    }
    
    [Test]
    public async Task TestQueueInject()
    {
        var injectionDelegate = Substitute.For<ContainerDelegate>();

        await using var container = await new DiContainerBindings()
            .Install(b => b.QueueInject(injectionDelegate))
            .Build(CancellationToken.None);

        injectionDelegate.Received(1).Invoke(Arg.Is<IDiContainer>(container));
    }

    [Test]
    public async Task TestQueueInitialize()
    {
        var initializationDelegate = Substitute.For<ContainerDelegate>();

        await using var container = await new DiContainerBindings()
            .Install(b => b.QueueInitialize(initializationDelegate))
            .Build(CancellationToken.None);

        initializationDelegate.Received(1).Invoke(Arg.Is<IDiContainer>(container));
    }
    
    [Test]
    public async Task TestQueueInitializeAsync()
    {
        var initializationDelegate = Substitute.For<AsyncContainerDelegate>();

        await using var container = await new DiContainerBindings()
            .Install(b => b.QueueInitializeAsync(initializationDelegate))
            .Build(CancellationToken.None);

        await initializationDelegate.Received(1).Invoke(Arg.Is<IDiContainer>(container), Arg.Any<CancellationToken>());
    }
    
    [Test]
    public async Task TestQueueStartup()
    {
        var startupDelegate = Substitute.For<ContainerDelegate>();

        await using var container = await new DiContainerBindings()
            .Install(b => b.QueueStartup(startupDelegate))
            .Build(CancellationToken.None);

        startupDelegate.Received(1).Invoke(Arg.Is<IDiContainer>(container));
    }
    
    [Test]
    public async Task TestQueueOrder()
    {
        var injectionDelegate = Substitute.For<ContainerDelegate>();
        var initializationDelegate = Substitute.For<ContainerDelegate>();
        var startupDelegate = Substitute.For<ContainerDelegate>();
        var disposeDelegate = Substitute.For<Action>();

        var container = await new DiContainerBindings()
            .Install(b =>
            {
                b.QueueInject(injectionDelegate);
                b.QueueInitialize(initializationDelegate);
                b.QueueStartup(startupDelegate);
                b.QueueDispose(disposeDelegate);
            })
            .Build(CancellationToken.None);
        
        await container.DisposeAsync();
        
        Received.InOrder(() =>
        {
            injectionDelegate.Invoke(Arg.Is<IDiContainer>(container));
            initializationDelegate.Invoke(Arg.Is<IDiContainer>(container));
            startupDelegate.Invoke(Arg.Is<IDiContainer>(container));
            disposeDelegate.Invoke();
        });
    }
}
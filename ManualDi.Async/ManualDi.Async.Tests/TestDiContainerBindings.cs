using NSubstitute;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ManualDi.Async.Tests;

public class TestDiContainerBindings
{
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
            .Install(b => b.QueueInjection(injectionDelegate))
            .Build(CancellationToken.None);

        injectionDelegate.Received(1).Invoke(Arg.Is<IDiContainer>(container));
    }

    [Test]
    public async Task TestQueueInitialization()
    {
        var initializationDelegate = Substitute.For<ContainerDelegate>();

        await using var container = await new DiContainerBindings()
            .Install(b => b.QueueInitialization(initializationDelegate))
            .Build(CancellationToken.None);

        initializationDelegate.Received(1).Invoke(Arg.Is<IDiContainer>(container));
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
                b.QueueInjection(injectionDelegate);
                b.QueueInitialization(initializationDelegate);
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
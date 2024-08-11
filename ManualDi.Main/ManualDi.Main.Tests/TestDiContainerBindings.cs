using NSubstitute;
using NUnit.Framework;
using System;

namespace ManualDi.Main.Tests;

public class TestDiContainerBindings
{
    [Test]
    public void TestQueueDispose()
    {
        var action = Substitute.For<Action>();

        var container = new DiContainerBindings()
            .Install(x => x.QueueDispose(action))
            .Build();

        action.DidNotReceive().Invoke();

        container.Dispose();

        action.Received(1).Invoke();
    }

    [Test]
    public void TestQueueInitialization()
    {
        var initializationDelegate = Substitute.For<ContainerDelegate>();

        var container = new DiContainerBindings()
            .Install(x => x.QueueInitialization(initializationDelegate))
            .Build();

        initializationDelegate.Received(1).Invoke(Arg.Is<IDiContainer>(container));
    }
}
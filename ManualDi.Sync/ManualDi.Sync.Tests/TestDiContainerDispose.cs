using NSubstitute;
using NUnit.Framework;
using System;

namespace ManualDi.Sync.Tests;

public class TestDiContainerDispose
{
    public interface IA : IDisposable { }
    public interface IB : IDisposable { }

    [Test]
    public void TestDisposeCalledByDefault()
    {
        IDiContainer container = new DiContainerBindings().Install(b =>
        {
            b.Bind<IDisposable>().FromInstance(Substitute.For<IDisposable>());
        }).Build();

        var disposable = container.Resolve<IDisposable>();
            
        container.Dispose();
            
        disposable.Received(1).Dispose();
    }
        
    [Test]
    public void TestSkipDisposablePreventsDispose()
    {
        IDiContainer container = new DiContainerBindings().Install(b =>
        {
            b.Bind<IDisposable>().FromInstance(Substitute.For<IDisposable>()).SkipDisposable();
        }).Build();

        var disposable = container.Resolve<IDisposable>();
            
        container.Dispose();
            
        disposable.DidNotReceive().Dispose();
    }
        
    [Test]
    public void TestDisposeOrder()
    {
        var disposable1 = Substitute.For<IA>();
        var disposable2 = Substitute.For<IB>();
            
        IDiContainer container = new DiContainerBindings().Install(b =>
        {
            b.Bind<IA>().FromMethod(c =>
            {
                _ = c.Resolve<IB>();
                return disposable1;
            });

            b.Bind<IB>().FromInstance(disposable2);
        }).Build();

        _ = container.Resolve<IA>();
            
        container.Dispose();
            
        Received.InOrder(() => {
            disposable1.Dispose();
            disposable2.Dispose();
        });
    }

    [Test]
    public void TestQueueDisposeOrder()
    {
        var action1 = Substitute.For<Action>();
        var action2 = Substitute.For<Action>();

        IDiContainer container = new DiContainerBindings().Build();
        container.QueueDispose(action1);
        container.QueueDispose(action2);

        container.Dispose();

        Received.InOrder(() =>
        {
            action2.Invoke();
            action1.Invoke();
        });
    }

    [Test]
    public void TestBindingsQueueDisposeOrder()
    {
        var action1 = Substitute.For<Action>();
        var action2 = Substitute.For<Action>();
        var disposable = Substitute.For<IA>();

        IDiContainer container = new DiContainerBindings().Install(b =>
        {
            b.QueueDispose(action1);
            b.QueueDispose(action2);
            b.Bind<IA>().FromInstance(disposable);
        }).Build();

        container.Dispose();

        Received.InOrder(() =>
        {
            disposable.Dispose();
            action2.Invoke();
            action1.Invoke();
        });
    }

    [Test]
    public void TestDisposeCustomOrder()
    {
        var instance1 = new object();
        var instance2 = new object();
        var dispose1 = Substitute.For<InstanceContainerDelegate>();
        var dispose2 = Substitute.For<InstanceContainerDelegate>();

        IDiContainer container = new DiContainerBindings().Install(b =>
        {
            b.Bind<IA>().FromMethod(c =>
            {
                _ = c.Resolve<IB>();
                return Substitute.For<IA>();
            }).SkipDisposable().Dispose((o, c) => dispose1.Invoke(instance1, c));

            b.Bind<IB>().FromInstance(Substitute.For<IB>())
                .SkipDisposable()
                .Dispose((o, c) => dispose2.Invoke(instance2, c));
        }).Build();

        container.Dispose();

        Received.InOrder(() =>
        {
            dispose1.Invoke(instance1, container);
            dispose2.Invoke(instance2, container);
        });
    }
        
    [Test]
    public void TestDisposeCustom()
    {
        var instance = new object();
        var disposeAction = Substitute.For<InstanceContainerDelegate>();

        IDiContainer container = new DiContainerBindings().Install(b =>
        {
            b.Bind<object>()
                .FromInstance(instance)
                .Dispose(disposeAction);
        }).Build();

        _ = container.Resolve<object>();

        disposeAction.DidNotReceive().Invoke(Arg.Any<object>(), Arg.Any<IDiContainer>());

        container.Dispose();

        disposeAction.Received(1).Invoke(Arg.Is<object>(instance), Arg.Is<IDiContainer>(container));
        disposeAction.ClearReceivedCalls();

        container.Dispose();

        disposeAction.DidNotReceive().Invoke(Arg.Any<object>(), Arg.Any<IDiContainer>());
    }
}
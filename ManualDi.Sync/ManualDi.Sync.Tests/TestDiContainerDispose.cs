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
    public void TestDontDisposePreventsDispose()
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
            disposable2.Dispose();
            disposable1.Dispose();
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
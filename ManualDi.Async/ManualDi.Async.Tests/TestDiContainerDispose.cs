using NSubstitute;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ManualDi.Async.Tests;

public class TestDiContainerDispose
{
    public interface IA : IDisposable { }
    public interface IB : IDisposable { }

    [Test]
    public async Task TestDisposeCalledByDefault()
    {
        IDiContainer container = await new DiContainerBindings().Install(b =>
        {
            b.Bind<IDisposable>().FromInstance(Substitute.For<IDisposable>());
        }).Build(CancellationToken.None);

        var disposable = container.Resolve<IDisposable>();
            
        await container.DisposeAsync();
            
        disposable.Received(1).Dispose();
    }
        
    [Test]
    public async Task TestDontDisposePreventsDispose()
    {
        IDiContainer container = await new DiContainerBindings().Install(b =>
        {
            b.Bind<IDisposable>().FromInstance(Substitute.For<IDisposable>()).DontDispose();
        }).Build(CancellationToken.None);

        var disposable = container.Resolve<IDisposable>();
            
        await container.DisposeAsync();
            
        disposable.DidNotReceive().Dispose();
    }
        
    [Test]
    public async Task TestDisposeOrder()
    {
        var disposable1 = Substitute.For<IA>();
        var disposable2 = Substitute.For<IB>();
            
        var container = await new DiContainerBindings().Install(b =>
        {
            b.Bind<IA>()
                .FromMethod(c =>
                {
                    _ = c.Resolve<IB>();
                    return disposable1;
                })
                .DependsOn(d => d.ConstructorDependency<IB>());

            b.Bind<IB>().FromInstance(disposable2);
        }).Build(CancellationToken.None);
        
        await container.DisposeAsync();
            
        Received.InOrder(() => {
            disposable2.Dispose();
            disposable1.Dispose();
        });
    }
        
    [Test]
    public async Task TestDisposeCustom()
    {
        var instance = new object();
        var disposeAction = Substitute.For<DisposeObjectDelegate>();

        IDiContainer container = await new DiContainerBindings().Install(b =>
        {
            b.Bind<object>()
                .FromInstance(instance)
                .Dispose(disposeAction);
        }).Build(CancellationToken.None);

        _ = container.Resolve<object>();

        disposeAction.DidNotReceive().Invoke(Arg.Any<object>());

        await container.DisposeAsync();

        disposeAction.Received(1).Invoke(Arg.Is<object>(instance));
        disposeAction.ClearReceivedCalls();

        await container.DisposeAsync();

        disposeAction.DidNotReceive().Invoke(Arg.Any<object>());
    }
}
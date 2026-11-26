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
            b.Bind<IDisposable>().FromInstance(Substitute.For<IDisposable>()).SkipDisposable();
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
            disposable1.Dispose();
            disposable2.Dispose();
        });
    }

    public interface IAsyncA : IAsyncDisposable { }
    public interface IAsyncB : IAsyncDisposable { }

    [Test]
    public async Task TestDisposeAsyncOrder()
    {
        var disposable1 = Substitute.For<IAsyncA>();
        var disposable2 = Substitute.For<IB>();

        var container = await new DiContainerBindings().Install(b =>
        {
            b.Bind<IAsyncA>()
                .FromInstance(disposable1)
                .DependsOn(d => d.ConstructorDependency<IB>());

            b.Bind<IB>().FromInstance(disposable2);
        }).Build(CancellationToken.None);

        await container.DisposeAsync();

        Received.InOrder(() =>
        {
            disposable1.DisposeAsync();
            disposable2.Dispose();
        });
    }

    [Test]
    public async Task TestQueueDisposeOrder()
    {
        var action1 = Substitute.For<Action>();
        var asyncAction2 = Substitute.For<Func<ValueTask>>();

        var container = await new DiContainerBindings().Build(CancellationToken.None);
        container.QueueDispose(action1);
        container.QueueAsyncDispose(asyncAction2);

        await container.DisposeAsync();

        Received.InOrder(() =>
        {
            asyncAction2.Invoke();
            action1.Invoke();
        });
    }

    [Test]
    public async Task TestBindingsQueueDisposeOrder()
    {
        var action1 = Substitute.For<Action>();
        var asyncAction2 = Substitute.For<Func<ValueTask>>();
        var disposable = Substitute.For<IA>();

        var container = await new DiContainerBindings().Install(b =>
        {
            b.QueueDispose(action1);
            b.QueueAsyncDispose(asyncAction2);
            b.Bind<IA>().FromInstance(disposable);
        }).Build(CancellationToken.None);

        await container.DisposeAsync();

        Received.InOrder(() =>
        {
            disposable.Dispose();
            asyncAction2.Invoke();
            action1.Invoke();
        });
    }

    [Test]
    public async Task TestDisposeCustomOrder()
    {
        var dispose1 = Substitute.For<DisposeObjectDelegate>();
        var dispose2 = Substitute.For<AsyncDisposeObjectDelegate>();
        var a = Substitute.For<IA>();
        var bInstance = Substitute.For<IAsyncB>();

        var container = await new DiContainerBindings().Install(b =>
        {
            b.Bind<IA>()
                .FromInstance(a)
                .DependsOn(d => d.ConstructorDependency<IAsyncB>())
                .SkipDisposable()
                .Dispose(dispose1);

            b.Bind<IAsyncB>()
                .FromInstance(bInstance)
                .SkipDisposable()
                .DisposeAsync(dispose2);
        }).Build(CancellationToken.None);

        await container.DisposeAsync();

        Received.InOrder(() =>
        {
            dispose1.Invoke(a);
            dispose2.Invoke(bInstance);
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
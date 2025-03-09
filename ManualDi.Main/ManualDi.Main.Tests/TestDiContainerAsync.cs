using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;

namespace ManualDi.Main.Tests;

public class TestDiContainerAsync
{
    [Test]
    public async Task TestAsyncCreate()
    {
        await using var container = await new DiContainerBindings().Install(b =>
        {
            b.BindAsync<int>().FromMethodAsync((_, _) => Task.FromResult(1));
        }).Build(CancellationToken.None);

        var value = container.Resolve<int>();
        Assert.That(value, Is.EqualTo(1)); 
    }
    
    [Test]
    public async Task TestAsyncSingleCalledOnce()
    {
        var createDelegate = Substitute.For<FromAsyncDelegate<int>>();
        createDelegate
            .Invoke(Arg.Any<IDiContainer>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(1));
        
        await using var container = await new DiContainerBindings().Install(b =>
        {
            b.BindAsync<int>().FromMethodAsync(createDelegate);
        }).Build(CancellationToken.None);
        
        await createDelegate.Received(1).Invoke(Arg.Any<IDiContainer>(), Arg.Any<CancellationToken>())!;
    }
    
    [Test]
    public async Task TestAsyncDifferentApparentAndConcrete()
    {
        var createDelegate = Substitute.For<FromAsyncDelegate<int>>();
        createDelegate
            .Invoke(Arg.Any<IDiContainer>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(1));
        
        await using var container = await new DiContainerBindings().Install(b =>
        {
            b.BindAsync<object, int>().FromMethodAsync(createDelegate);
        }).Build(CancellationToken.None);

        var resolution = container.Resolve<object>();
        Assert.That(resolution, Is.EqualTo(1));
    }
    
    [Test]
    public async Task TestAsyncBindingOrder()
    {
        var createDelegate = Substitute.For<FromAsyncDelegate<int>>();
        var injectDelegate = Substitute.For<InjectDelegate<int>>();
        var injectAsyncDelegate = Substitute.For<InjectAsyncDelegate<int>>();
        var initializeDelegate = Substitute.For<InitializeDelegate<int>>();
        var initializeAsyncDelegate = Substitute.For<InitializeAsyncDelegate<int>>();
        var disposeDelegate = Substitute.For<DisposeObjectDelegate<int>>();
        var disposeAsyncDelegate = Substitute.For<AsyncDisposeObjectDelegate<int>>();
        
        var container = await new DiContainerBindings().Install(b =>
        {
            b.BindAsync<int>()
                .FromMethodAsync(createDelegate)
                .Inject(injectDelegate)
                .InjectAsync(injectAsyncDelegate)
                .Initialize(initializeDelegate)
                .InitializeAsync(initializeAsyncDelegate)
                .Dispose(disposeDelegate)
                .DisposeAsync(disposeAsyncDelegate);
        }).Build(CancellationToken.None);

        await container.DisposeAsync();

        Received.InOrder(() =>
        {
            createDelegate.Invoke(Arg.Any<IDiContainer>(), Arg.Any<CancellationToken>());
            injectAsyncDelegate.Invoke(Arg.Any<int>(), Arg.Any<IDiContainer>(), Arg.Any<CancellationToken>());
            injectDelegate.Invoke(Arg.Any<int>(), Arg.Any<IDiContainer>());
            initializeAsyncDelegate.Invoke(Arg.Any<int>(), Arg.Any<CancellationToken>());
            initializeDelegate.Invoke(Arg.Any<int>());
            disposeAsyncDelegate.Invoke(Arg.Any<int>());
            disposeDelegate.Invoke(Arg.Any<int>());
        });
    }

    private class DependencyOrder1
    {
        public DependencyOrder1(DependencyOrder2 other) { }
        public ValueTask InjectAsync(DependencyOrder2 other) => ValueTask.CompletedTask;
    }

    private class DependencyOrder2
    {
    }
    
    [Test, Ignore("Do not use while refactoring"), CancelAfter(1000)]
    public async Task TestAsyncDependencyOrder()
    {
        var afterCreate2 = Substitute.For<Action>();
        var beforeInject = Substitute.For<Action>();
        var afterInject = Substitute.For<Action>();

        var taskCompletionSource = new TaskCompletionSource();
        
        await using var container = await new DiContainerBindings().Install(b =>
        {
            b.BindAsync<DependencyOrder1>()
                .FromMethod(c => new DependencyOrder1(c.Resolve<DependencyOrder2>()))
                .InjectAsync(async (o, c, ct) =>
                {
                    beforeInject.Invoke();
                    var resolvedTask = c.Resolve<DependencyOrder2>();
                    await o.InjectAsync(resolvedTask);
                    afterInject.Invoke();
                });

            b.BindAsync<DependencyOrder2>()
                .FromMethodAsync(async (c, ct) =>
                {
                    await taskCompletionSource.Task;
                    afterCreate2.Invoke();
                    return new DependencyOrder2();
                });
        }).Build(CancellationToken.None);
        
        taskCompletionSource.SetResult();

        Received.InOrder(() =>
        {
            beforeInject.Received(1).Invoke();
            afterCreate2.Received(1).Invoke();
            afterInject.Received(1).Invoke();
        });
    }
}
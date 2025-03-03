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
        await using var container = new DiContainerBindings().Install(b =>
        {
            b.BindAsync<int>().FromMethodAsync((_, _) => Task.FromResult(1));
        }).Build();

        var value = await container.ResolveAsync<int>();
        Assert.That(value, Is.EqualTo(1)); 
    }
    
    [Test]
    public async Task TestAsyncSingleCalledOnce()
    {
        var createDelegate = Substitute.For<CreateAsyncDelegate<int>>();
        createDelegate
            .Invoke(Arg.Any<IDiContainer>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(1));
        
        await using var container = new DiContainerBindings().Install(b =>
        {
            b.BindAsync<int>().FromMethodAsync(createDelegate).Single();
        }).Build();

        await container.ResolveAsync<int>();
        await container.ResolveAsync<int>();
        
        await createDelegate.Received(1).Invoke(Arg.Any<IDiContainer>(), Arg.Any<CancellationToken>())!;
    }
    
    [Test]
    public async Task TestAsyncTransientCalledTwice()
    {
        var createDelegate = Substitute.For<CreateAsyncDelegate<int>>();
        createDelegate
            .Invoke(Arg.Any<IDiContainer>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(1));
        
        await using var container = new DiContainerBindings().Install(b =>
        {
            b.BindAsync<int>().FromMethodAsync(createDelegate).Transient();
        }).Build();

        await container.ResolveAsync<int>();
        await container.ResolveAsync<int>();
        
        await createDelegate.Received(2).Invoke(Arg.Any<IDiContainer>(), Arg.Any<CancellationToken>())!;
    }
    
    [Test]
    public async Task TestAsyncDifferentApparentAndConcrete()
    {
        var createDelegate = Substitute.For<CreateAsyncDelegate<int>>();
        createDelegate
            .Invoke(Arg.Any<IDiContainer>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(1));
        
        await using var container = new DiContainerBindings().Install(b =>
        {
            b.BindAsync<object, int>().FromMethodAsync(createDelegate);
        }).Build();

        var resolution = await container.ResolveAsync<object>();
        Assert.That(resolution, Is.EqualTo(1));
    }
    
    [Test]
    public async Task TestAsyncBindingOrder()
    {
        var createDelegate = Substitute.For<CreateAsyncDelegate<int>>();
        var injectDelegate = Substitute.For<InstanceContainerDelegate<int>>();
        var injectAsyncDelegate = Substitute.For<InstanceContainerAsyncDelegate<int>>();
        var initializeDelegate = Substitute.For<InstanceContainerDelegate<int>>();
        var initializeAsyncDelegate = Substitute.For<InstanceContainerAsyncDelegate<int>>();
        var disposeDelegate = Substitute.For<DisposeObjectDelegate<int>>();
        var disposeAsyncDelegate = Substitute.For<AsyncDisposeObjectDelegate<int>>();
        
        var container = new DiContainerBindings().Install(b =>
        {
            b.BindAsync<int>()
                .FromMethodAsync(createDelegate)
                .Inject(injectDelegate)
                .InjectAsync(injectAsyncDelegate)
                .Initialize(initializeDelegate)
                .InitializeAsync(initializeAsyncDelegate)
                .Dispose(disposeDelegate)
                .DisposeAsync(disposeAsyncDelegate);
        }).Build();

        await container.ResolveAsync<int>();
        await container.DisposeAsync();

        Received.InOrder(() =>
        {
            createDelegate.Invoke(Arg.Any<IDiContainer>(), Arg.Any<CancellationToken>());
            injectAsyncDelegate.Invoke(Arg.Any<int>(), Arg.Any<IDiContainer>(), Arg.Any<CancellationToken>());
            injectDelegate.Invoke(Arg.Any<int>(), Arg.Any<IDiContainer>());
            initializeAsyncDelegate.Invoke(Arg.Any<int>(), Arg.Any<IDiContainer>(), Arg.Any<CancellationToken>());
            initializeDelegate.Invoke(Arg.Any<int>(), Arg.Any<IDiContainer>());
            disposeAsyncDelegate.Invoke(Arg.Any<int>());
            disposeDelegate.Invoke(Arg.Any<int>());
        });
    }

    private class DependencyOrder1
    {
        public DependencyOrder1(Task<DependencyOrder2> other) { }
        public async ValueTask InjectAsync(Task<DependencyOrder2> other) => await other;
    }

    private class DependencyOrder2
    {
    }
    
    [Test, Timeout(1000)]
    public async Task TestAsyncDependencyOrder()
    {
        var afterCreate2 = Substitute.For<Action>();
        var beforeInject = Substitute.For<Action>();
        var afterInject = Substitute.For<Action>();

        var taskCompletionSource = new TaskCompletionSource();
        
        await using var container = new DiContainerBindings().Install(b =>
        {
            b.BindAsync<DependencyOrder1>()
                .FromMethod(c => new DependencyOrder1(c.ResolveAsync<DependencyOrder2>()))
                .InjectAsync(async (o, c, ct) =>
                {
                    beforeInject.Invoke();
                    var resolvedTask = c.ResolveAsync<DependencyOrder2>();
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
        }).Build();

        afterCreate2.DidNotReceive().Invoke();
        beforeInject.DidNotReceive().Invoke();
        afterInject.DidNotReceive().Invoke();
        
        var resolution = container.ResolveAsync<DependencyOrder1>();
        
        beforeInject.Received(1).Invoke();
        afterInject.DidNotReceive().Invoke();
        
        taskCompletionSource.SetResult();
        await resolution;
        
        afterCreate2.Received(1).Invoke();
        afterInject.Received(1).Invoke();
    }
}
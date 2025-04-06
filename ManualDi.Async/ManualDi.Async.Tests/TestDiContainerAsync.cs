using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;

namespace ManualDi.Async.Tests;

public class TestDiContainerAsync
{
    [Test]
    public async Task TestAsyncCreate()
    {
        await using var container = await new DiContainerBindings().Install(b =>
        {
            b.Bind<int>().FromMethodAsync((_, _) => Task.FromResult<object?>(1));
        }).Build(CancellationToken.None);

        var value = container.Resolve<int>();
        Assert.That(value, Is.EqualTo(1)); 
    }
    
    [Test]
    public async Task TestAsyncSingleCalledOnce()
    {
        var createDelegate = Substitute.For<FromAsyncDelegate>();
        createDelegate
            .Invoke(Arg.Any<IDiContainer>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(1));
        
        await using var container = await new DiContainerBindings().Install(b =>
        {
            b.Bind<int>().FromMethodAsync(createDelegate);
        }).Build(CancellationToken.None);
        
        await createDelegate.Received(1).Invoke(Arg.Any<IDiContainer>(), Arg.Any<CancellationToken>())!;
    }
    
    [Test]
    public async Task TestAsyncDifferentApparentAndConcrete()
    {
        var createDelegate = Substitute.For<FromAsyncDelegate>();
        createDelegate
            .Invoke(Arg.Any<IDiContainer>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<object?>(1));
        
        await using var container = await new DiContainerBindings().Install(b =>
        {
            b.Bind<object, int>().FromMethodAsync(createDelegate);
        }).Build(CancellationToken.None);

        var resolution = container.Resolve<object>();
        Assert.That(resolution, Is.EqualTo(1));
    }
    
    [Test]
    public async Task TestAsyncBindingOrder()
    {
        var createDelegate = Substitute.For<FromAsyncDelegate>();
        (await createDelegate.Invoke(Arg.Any<IDiContainer>(), Arg.Any<CancellationToken>())).Returns(Task.FromResult<object?>(1));
        var injectDelegate1 = Substitute.For<InjectDelegate>();
        var injectDelegate2 = Substitute.For<InjectDelegate>();
        var injectAsyncDelegate = Substitute.For<InjectAsyncDelegate>();
        var initializeDelegate1 = Substitute.For<InitializeDelegate>();
        var initializeDelegate2 = Substitute.For<InitializeDelegate>();
        var initializeAsyncDelegate = Substitute.For<InitializeAsyncDelegate>();
        var disposeDelegate = Substitute.For<DisposeObjectDelegate>();
        var disposeAsyncDelegate = Substitute.For<AsyncDisposeObjectDelegate>();
        
        var container = await new DiContainerBindings().Install(b =>
        {
            b.Bind<int>()
                .FromMethodAsync(createDelegate)
                .Inject(injectDelegate1)
                .InjectAsync(injectAsyncDelegate)
                .Inject(injectDelegate2)
                .Initialize(initializeDelegate1)
                .InitializeAsync(initializeAsyncDelegate)
                .Initialize(initializeDelegate2)
                .DisposeAsync(disposeAsyncDelegate)
                .Dispose(disposeDelegate);
        }).Build(CancellationToken.None);

        await container.DisposeAsync();

        Received.InOrder(() =>
        {
            createDelegate.Invoke(Arg.Any<IDiContainer>(), Arg.Any<CancellationToken>());
            injectDelegate1.Invoke(Arg.Any<int>(), Arg.Any<IDiContainer>());
            injectAsyncDelegate.Invoke(Arg.Any<int>(), Arg.Any<IDiContainer>(), Arg.Any<CancellationToken>());
            injectDelegate2.Invoke(Arg.Any<int>(), Arg.Any<IDiContainer>());
            initializeDelegate1.Invoke(Arg.Any<int>());
            initializeAsyncDelegate.Invoke(Arg.Any<int>(), Arg.Any<CancellationToken>());
            initializeDelegate2.Invoke(Arg.Any<int>());
            disposeAsyncDelegate.Invoke(Arg.Any<int>());
            disposeDelegate.Invoke(Arg.Any<int>());
        });
    }
}
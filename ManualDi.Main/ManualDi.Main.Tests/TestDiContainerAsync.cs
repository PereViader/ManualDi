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
}
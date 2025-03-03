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
        var container = new DiContainerBindings().Install(b =>
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
        
        var container = new DiContainerBindings().Install(b =>
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
        
        var container = new DiContainerBindings().Install(b =>
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
        
        var container = new DiContainerBindings().Install(b =>
        {
            b.BindAsync<object, int>().FromMethodAsync(createDelegate);
        }).Build();

        var resolution = await container.ResolveAsync<object>();
        Assert.That(resolution, Is.EqualTo(1));
    }
}
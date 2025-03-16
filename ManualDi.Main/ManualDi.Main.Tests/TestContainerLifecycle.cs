using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;

namespace ManualDi.Main.Tests;

public class TestContainerLifecycle
{
    public interface IChild1 : IDisposable
    {
        void Inject();
        void Initialize();
    }
    
    public interface IChild1Child : IAsyncDisposable
    {
        void Inject();
        Task InitializeAsync();
    }
    
    public interface IChild2 : IAsyncDisposable
    {
        void Inject();
        Task InitializeAsync();
    }
    
    public interface IChild2Child : IDisposable
    {
        void Inject();
        void Initialize();
    }

    public interface IStartup : IAsyncDisposable
    {
        void Inject();
        Task InitializeAsync();
        void Run();
    }

    [Test]
    public async Task TestLifecycle()
    {
        var child1 = Substitute.For<IChild1>();
        var child2 = Substitute.For<IChild2>();
        var child1Child = Substitute.For<IChild1Child>();
        var child2Child = Substitute.For<IChild2Child>();
        var startup = Substitute.For<IStartup>();

        var container = await new DiContainerBindings().Install(b =>
        {
            b.Bind<IChild1>()
                .FromMethod(_ => child1, d => d.Dependency<IChild1Child>())
                .Inject((o, c) => o.Inject())
                .Initialize(o => o.Initialize());

            b.Bind<IChild2Child>()
                .FromMethod(_ => child2Child)
                .Inject((o, c) => o.Inject())
                .Initialize(o => o.Initialize());

            b.BindAsync<IChild1Child>()
                .FromMethod(_ => child1Child)
                .Inject((o, c) => o.Inject())
                .InitializeAsync((o, _) => o.InitializeAsync());

            b.BindAsync<IChild2>()
                .FromMethod(_ => child2, d => d.Dependency<IChild2Child>())
                .Inject((o, c) => o.Inject())
                .InitializeAsync((o, _) => o.InitializeAsync());

            b.BindAsync<IStartup>()
                .FromMethod(_ => startup, d =>
                {
                    d.Dependency<IChild1>();
                    d.Dependency<IChild2>();
                })
                .Inject((o, c) => o.Inject())
                .Initialize(o => o.InitializeAsync());

            
            b.WithStartup<IStartup>(e => e.Run());
        }).Build(CancellationToken.None);
        
        await container.DisposeAsync();
        
        Received.InOrder(() =>
        {
            child1Child.Inject();
            child1.Inject();
            child2Child.Inject();
            child2.Inject();
            startup.Inject();
            
            child1Child.InitializeAsync();
            child1.Initialize();
            child2Child.Initialize();
            child2.InitializeAsync();
            startup.InitializeAsync();

            startup.Run();
        });
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;

namespace ManualDi.Async.Tests;

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
                .FromMethod(_ => child1)
                .DependsOn(d => d.ConstructorDependency<IChild1Child>())
                .Inject((o, c) => ((IChild1)o).Inject())
                .Initialize(o => ((IChild1)o).Initialize());

            b.Bind<IChild2Child>()
                .FromInstance(child2Child)
                .Inject((o, c) => ((IChild2Child)o).Inject())
                .Initialize(o => ((IChild2Child)o).Initialize());

            b.Bind<IChild1Child>()
                .FromInstance(child1Child)
                .Inject((o, c) => ((IChild1Child)o).Inject())
                .InitializeAsync((o, _) => ((IChild1Child)o).InitializeAsync());

            b.Bind<IChild2>()
                .FromInstance(child2)
                .DependsOn(d => d.ConstructorDependency<IChild2Child>())
                .Inject((o, c) => ((IChild2)o).Inject())
                .InitializeAsync((o, _) => ((IChild2)o).InitializeAsync());

            b.Bind<IStartup>()
                .FromInstance(startup)
                .DependsOn(d =>
                {
                    d.ConstructorDependency<IChild1>();
                    d.ConstructorDependency<IChild2>();
                })
                .Inject((o, c) => ((IStartup)o).Inject())
                .Initialize(o => ((IStartup)o).InitializeAsync());

            
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
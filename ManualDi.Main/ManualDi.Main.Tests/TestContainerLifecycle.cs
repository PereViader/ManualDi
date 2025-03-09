using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;

namespace ManualDi.Main.Tests;

public class TestContainerLifecycle
{
    public interface INonLazy
    {
        void Inject(IInjectChild injectChild);
        void Initialize();
        void Dispose();
    }

    public interface IInjectChild
    {
        void Inject();
        void Initialize();
        void Dispose();
    }

    public interface IInitChild
    {
        void Inject();
        void Initialize();
        void Dispose();
    }

    public interface IStartup
    {
        void Run();
        void Dispose();
    }
    
    public interface IResolveAfter
    {
        void Run();
        void Dispose();
    }

    [Test]
    public async Task TestLifecycle()
    {
        var nonLazy = Substitute.For<INonLazy>();
        var injectChild = Substitute.For<IInjectChild>();
        var initChild = Substitute.For<IInitChild>();
        var startup = Substitute.For<IStartup>();
        var resolveAfter = Substitute.For<IResolveAfter>();

        await using var container = await new DiContainerBindings().Install(b =>
        {
            b.Bind<INonLazy>()
                .FromInstance(nonLazy)
                .Inject((o, c) => o.Inject(c.Resolve<IInjectChild>()))
                .Initialize(o => o.Initialize())
                .Dispose(o => o.Dispose());
            b.Bind<IInjectChild>()
                .FromInstance(injectChild)
                .Inject((o, c) => o.Inject())
                .Initialize(o => o.Initialize())
                .Dispose(o => o.Dispose());

            b.Bind<IInitChild>()
                .FromInstance(initChild)
                .Inject((o, c) => o.Inject())
                .Initialize(o => o.Initialize())
                .Dispose(o => o.Dispose());

            b.Bind<IStartup>()
                .FromInstance(startup)
                .Dispose(o=> o.Dispose());

            b.Bind<IResolveAfter>()
                .FromInstance(resolveAfter)
                .Dispose(o=> o.Dispose());
            
            b.WithStartup<IStartup>(e => e.Run());
        }).Build(CancellationToken.None);

        container.Resolve<IResolveAfter>().Run();
        
        await container.DisposeAsync();
        
        Received.InOrder(() =>
        {
            injectChild.Inject();
            nonLazy.Inject(Arg.Is(injectChild));
            injectChild.Initialize();
            initChild.Inject();
            initChild.Initialize();
            nonLazy.Initialize();
            
            startup.Run();
            
            resolveAfter.Run();
            
            injectChild.Dispose();
            nonLazy.Dispose();
            initChild.Dispose();
            startup.Dispose();
            resolveAfter.Dispose();
        });
    }
}
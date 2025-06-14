using System;
using NSubstitute;
using NUnit.Framework;

namespace ManualDi.Sync.Tests;

public class TestContainerLifecycle
{
    public interface INonLazy
    {
        void Inject(IInjectChild injectChild);
        void Initialize(IInitChild initChild);
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
    public void TestLifecycle()
    {
        var injectChild = Substitute.For<IInjectChild>();
        var initChild = Substitute.For<IInitChild>();
        var startup = Substitute.For<IStartup>();
        var resolveAfter = Substitute.For<IResolveAfter>();

        var container = new DiContainerBindings().Install(b =>
        {
            b.Bind<IInjectChild>()
                .FromInstance(injectChild)
                .Inject((o, c) => ((IInjectChild)o).Inject())
                .Initialize((o, c) => ((IInjectChild)o).Initialize())
                .Dispose((o, c) => ((IInjectChild)o).Dispose());

            b.Bind<IInitChild>()
                .FromInstance(initChild)
                .Inject((o, c) => ((IInitChild)o).Inject())
                .Initialize((o, c) => ((IInitChild)o).Initialize())
                .Dispose((o, c) => ((IInitChild)o).Dispose());

            b.Bind<IStartup>()
                .FromInstance(startup)
                .Dispose((o, c) => ((IStartup)o).Dispose());

            b.Bind<IResolveAfter>()
                .FromInstance(resolveAfter)
                .Dispose((o, c) => ((IResolveAfter)o).Dispose());
            
            b.QueueStartup<IStartup>(e => e.Run());
        }).Build();

        container.Resolve<IResolveAfter>().Run();
        
        container.Dispose();
        
        Received.InOrder(() =>
        {
            injectChild.Inject();
            injectChild.Initialize();
            initChild.Inject();
            initChild.Initialize();
            
            startup.Run();
            
            resolveAfter.Run();
            
            injectChild.Dispose();
            initChild.Dispose();
            startup.Dispose();
            resolveAfter.Dispose();
        });
    }
}
﻿using System;
using NSubstitute;
using NUnit.Framework;

namespace ManualDi.Main.Tests;

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
        var nonLazy = Substitute.For<INonLazy>();
        var injectChild = Substitute.For<IInjectChild>();
        var initChild = Substitute.For<IInitChild>();
        var startup = Substitute.For<IStartup>();
        var resolveAfter = Substitute.For<IResolveAfter>();

        var container = new DiContainerBindings().Install(b =>
        {
            b.Bind<INonLazy>()
                .FromInstance(nonLazy)
                .Inject((o, c) => o.Inject(c.Resolve<IInjectChild>()))
                .Initialize((o, c) => o.Initialize(c.Resolve<IInitChild>()))
                .Dispose((o, c) => o.Dispose())
                .NonLazy();

            b.Bind<IInjectChild>()
                .FromInstance(injectChild)
                .Inject((o, c) => o.Inject())
                .Initialize((o, c) => o.Initialize())
                .Dispose((o, c) => o.Dispose());

            b.Bind<IInitChild>()
                .FromInstance(initChild)
                .Inject((o, c) => o.Inject())
                .Initialize((o, c) => o.Initialize())
                .Dispose((o, c) => o.Dispose());

            b.Bind<IStartup>()
                .FromInstance(startup)
                .Dispose((o, c) => o.Dispose());

            b.Bind<IResolveAfter>()
                .FromInstance(resolveAfter)
                .Dispose((o, c) => o.Dispose());
            
            b.WithStartup<IStartup>(e => e.Run());
        }).Build();

        container.Resolve<IResolveAfter>().Run();
        
        container.Dispose();
        
        Received.InOrder(() =>
        {
            injectChild.Inject();
            nonLazy.Inject(Arg.Is(injectChild));
            injectChild.Initialize();
            initChild.Inject();
            initChild.Initialize();
            nonLazy.Initialize(Arg.Is(initChild));
            
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
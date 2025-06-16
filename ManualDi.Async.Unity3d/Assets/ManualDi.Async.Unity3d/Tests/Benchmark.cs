using System;
using System.Runtime.CompilerServices;
using System.Threading;
using NUnit.Framework;
using Unity.PerformanceTesting;
using VContainer;

namespace ManualDi.Async.Unity3d.Tests
{
    public class ContainerPerformanceTest
    {
        const int MeasurementCount = 100;
        const int WarmupCount = 100;

        [Test]
        [Performance]
        public void ManualDi()
        {
            RunBenchmark(() =>
            {
                var manualDiBuilder = new ManualDi.Async.DiContainerBindings();
                manualDiBuilder.Bind<IFirstService, FirstService>().Default().FromConstructor();
                manualDiBuilder.Bind<ISecondService, SecondService>().Default().FromConstructor();
                manualDiBuilder.Bind<IThirdService, ThirdService>().Default().FromConstructor();
                manualDiBuilder.Bind<ISubObjectA, SubObjectA>().Default().FromConstructor();
                manualDiBuilder.Bind<ISubObjectB, SubObjectB>().Default().FromConstructor();
                manualDiBuilder.Bind<ISubObjectC, SubObjectC>().Default().FromConstructor();
                manualDiBuilder.Bind<IComplex1, Complex1>().Default().FromConstructor();
                manualDiBuilder.Bind<IComplex2, Complex2>().Default().FromConstructor();
                manualDiBuilder.Bind<IComplex3, Complex3>().Default().FromConstructor();
                manualDiBuilder.Bind<ISubObjectOne, SubObjectOne>().Default().FromConstructor();
                manualDiBuilder.Bind<ISubObjectTwo, SubObjectTwo>().Default().FromConstructor();
                manualDiBuilder.Bind<ISubObjectThree, SubObjectThree>().Default().FromConstructor();
                manualDiBuilder.Bind<IServiceA, ServiceA>().Default().FromConstructor();
                manualDiBuilder.Bind<IServiceB, ServiceB>().Default().FromConstructor();
                manualDiBuilder.Bind<IServiceC, ServiceC>().Default().FromConstructor();
                var manualDiContainer = manualDiBuilder.Build(CancellationToken.None).Result;
                
                manualDiContainer.Resolve<IComplex1>();
                manualDiContainer.Resolve<IComplex2>();
                manualDiContainer.Resolve<IComplex3>();
            });
        }

        [Test]
        [Performance]
        public void Reflex()
        {
            RunBenchmark(() =>
            {
                var reflexContainer = new Reflex.Container();
                reflexContainer.Bind<IFirstService>().To<FirstService>().AsSingletonLazy();
                reflexContainer.Bind<ISecondService>().To<SecondService>().AsSingletonLazy();
                reflexContainer.Bind<IThirdService>().To<ThirdService>().AsSingletonLazy();
                reflexContainer.Bind<ISubObjectA>().To<SubObjectA>().AsSingletonLazy();
                reflexContainer.Bind<ISubObjectB>().To<SubObjectB>().AsSingletonLazy();
                reflexContainer.Bind<ISubObjectC>().To<SubObjectC>().AsSingletonLazy();
                reflexContainer.Bind<IComplex1>().To<Complex1>().AsSingletonLazy();
                reflexContainer.Bind<IComplex2>().To<Complex2>().AsSingletonLazy();
                reflexContainer.Bind<IComplex3>().To<Complex3>().AsSingletonLazy();
                reflexContainer.Bind<ISubObjectOne>().To<SubObjectOne>().AsSingletonLazy();
                reflexContainer.Bind<ISubObjectTwo>().To<SubObjectTwo>().AsSingletonLazy();
                reflexContainer.Bind<ISubObjectThree>().To<SubObjectThree>().AsSingletonLazy();
                reflexContainer.Bind<IServiceA>().To<ServiceA>().AsSingletonLazy();
                reflexContainer.Bind<IServiceB>().To<ServiceB>().AsSingletonLazy();
                reflexContainer.Bind<IServiceC>().To<ServiceC>().AsSingletonLazy();

                reflexContainer.Resolve<IComplex1>();
                reflexContainer.Resolve<IComplex2>();
                reflexContainer.Resolve<IComplex3>();
            });
        }

        [Test]
        [Performance]
        public void VContainer()
        {
            RunBenchmark(() =>
            {
                var vContainerBuilder = new ContainerBuilder();
                vContainerBuilder.Register<IFirstService, FirstService>(Lifetime.Singleton);
                vContainerBuilder.Register<ISecondService, SecondService>(Lifetime.Singleton);
                vContainerBuilder.Register<IThirdService, ThirdService>(Lifetime.Singleton);
                vContainerBuilder.Register<ISubObjectA, SubObjectA>(Lifetime.Singleton);
                vContainerBuilder.Register<ISubObjectB, SubObjectB>(Lifetime.Singleton);
                vContainerBuilder.Register<ISubObjectC, SubObjectC>(Lifetime.Singleton);
                vContainerBuilder.Register<IComplex1, Complex1>(Lifetime.Singleton);
                vContainerBuilder.Register<IComplex2, Complex2>(Lifetime.Singleton);
                vContainerBuilder.Register<IComplex3, Complex3>(Lifetime.Singleton);
                vContainerBuilder.Register<ISubObjectOne, SubObjectOne>(Lifetime.Singleton);
                vContainerBuilder.Register<ISubObjectTwo, SubObjectTwo>(Lifetime.Singleton);
                vContainerBuilder.Register<ISubObjectThree, SubObjectThree>(Lifetime.Singleton);
                vContainerBuilder.Register<IServiceA, ServiceA>(Lifetime.Singleton);
                vContainerBuilder.Register<IServiceB, ServiceB>(Lifetime.Singleton);
                vContainerBuilder.Register<IServiceC, ServiceC>(Lifetime.Singleton);
                var vContainer = vContainerBuilder.Build();

                vContainer.Resolve<IComplex1>();
                vContainer.Resolve<IComplex2>();
                vContainer.Resolve<IComplex3>();
            });
        }

        [Test]
        [Performance]
        public void Zenject()
        {
            RunBenchmark(() =>
            {
                var zenjectContainer = new Zenject.DiContainer();
                zenjectContainer.Bind<IFirstService>().To<FirstService>().AsSingle();
                zenjectContainer.Bind<ISecondService>().To<SecondService>().AsSingle();
                zenjectContainer.Bind<IThirdService>().To<ThirdService>().AsSingle();
                zenjectContainer.Bind<ISubObjectA>().To<SubObjectA>().AsSingle();
                zenjectContainer.Bind<ISubObjectB>().To<SubObjectB>().AsSingle();
                zenjectContainer.Bind<ISubObjectC>().To<SubObjectC>().AsSingle();
                zenjectContainer.Bind<IComplex1>().To<Complex1>().AsSingle();
                zenjectContainer.Bind<IComplex2>().To<Complex2>().AsSingle();
                zenjectContainer.Bind<IComplex3>().To<Complex3>().AsSingle();
                zenjectContainer.Bind<ISubObjectOne>().To<SubObjectOne>().AsSingle();
                zenjectContainer.Bind<ISubObjectTwo>().To<SubObjectTwo>().AsSingle();
                zenjectContainer.Bind<ISubObjectThree>().To<SubObjectThree>().AsSingle();
                zenjectContainer.Bind<IServiceA>().To<ServiceA>().AsSingle();
                zenjectContainer.Bind<IServiceB>().To<ServiceB>().AsSingle();
                zenjectContainer.Bind<IServiceC>().To<ServiceC>().AsSingle();
                
                zenjectContainer.Resolve<IComplex1>();
                zenjectContainer.Resolve<IComplex2>();
                zenjectContainer.Resolve<IComplex3>();
            });
        }

        private static void RunBenchmark(Action action, [CallerMemberName] string name = "")
        {
            Measure
                .Method(action)
                .SampleGroup(new SampleGroup(name, SampleUnit.Nanosecond))
                .GC()
                .MeasurementCount(MeasurementCount)
                .WarmupCount(WarmupCount)
                .Run();
        }
    }
}

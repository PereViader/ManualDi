using ManualDi.Main;
using NUnit.Framework;
using Unity.PerformanceTesting;
using VContainer;

namespace ManualDi.Unity3d.Tests
{
    public class ContainerPerformanceTest
    {
        const int MeasurementCount = 100;
        const int WarmupCount = 100;

        [Test]
        [Performance]
        public void Benchmark()
        {
            Measure
                .Method(() =>
                {
                    var manualDiBuilder = new ManualDi.Main.DiContainerBindings();
                    manualDiBuilder.Bind<IFirstService, FirstService>().Default().FromConstructor();
                    manualDiBuilder.Bind<ISecondService, SecondService>().Default().FromConstructor();
                    manualDiBuilder.Bind<IThirdService, ThirdService>().Default().FromConstructor();
                    manualDiBuilder.Bind<ISubObjectA, SubObjectA>().Default().Transient().FromConstructor();
                    manualDiBuilder.Bind<ISubObjectB, SubObjectB>().Default().Transient().FromConstructor();
                    manualDiBuilder.Bind<ISubObjectC, SubObjectC>().Default().Transient().FromConstructor();
                    manualDiBuilder.Bind<IComplex1, Complex1>().Default().Transient().FromConstructor();
                    manualDiBuilder.Bind<IComplex2, Complex2>().Default().Transient().FromConstructor();
                    manualDiBuilder.Bind<IComplex3, Complex3>().Default().Transient().FromConstructor();
                    manualDiBuilder.Bind<ISubObjectOne, SubObjectOne>().Default().FromConstructor();
                    manualDiBuilder.Bind<ISubObjectTwo, SubObjectTwo>().Default().Transient().FromConstructor();
                    manualDiBuilder.Bind<ISubObjectThree, SubObjectThree>().Default().Transient().FromConstructor();
                    var manualDiContainer = manualDiBuilder.Build();
                    
                    manualDiContainer.Resolve<IComplex1>();
                    manualDiContainer.Resolve<IComplex2>();
                    manualDiContainer.Resolve<IComplex3>();
                })
                .SampleGroup(new SampleGroup("ManualDi", SampleUnit.Nanosecond))
                .GC()
                .MeasurementCount(MeasurementCount)
                .WarmupCount(WarmupCount)
                .Run();
            
            Measure
                .Method(() =>
                {
                    var reflexContainer = new Reflex.Container();
                    reflexContainer.Bind<IFirstService>().To<FirstService>().AsSingletonLazy();
                    reflexContainer.Bind<ISecondService>().To<SecondService>().AsSingletonLazy();
                    reflexContainer.Bind<IThirdService>().To<ThirdService>().AsSingletonLazy();
                    reflexContainer.Bind<ISubObjectA>().To<SubObjectA>().AsTransient();
                    reflexContainer.Bind<ISubObjectB>().To<SubObjectB>().AsTransient();
                    reflexContainer.Bind<ISubObjectC>().To<SubObjectC>().AsTransient();
                    reflexContainer.Bind<IComplex1>().To<Complex1>().AsTransient();
                    reflexContainer.Bind<IComplex2>().To<Complex2>().AsTransient();
                    reflexContainer.Bind<IComplex3>().To<Complex3>().AsTransient();
                    reflexContainer.Bind<ISubObjectOne>().To<SubObjectOne>().AsTransient();
                    reflexContainer.Bind<ISubObjectTwo>().To<SubObjectTwo>().AsTransient();
                    reflexContainer.Bind<ISubObjectThree>().To<SubObjectThree>().AsTransient();
                    
                    reflexContainer.Resolve<IComplex1>();
                    reflexContainer.Resolve<IComplex2>();
                    reflexContainer.Resolve<IComplex3>();
                })
                .SampleGroup(new SampleGroup("Reflex", SampleUnit.Nanosecond))
                .GC()
                .MeasurementCount(MeasurementCount)
                .WarmupCount(WarmupCount)
                .Run();
            
            Measure
                .Method(() =>
                {
                    var vContainerBuilder = new ContainerBuilder();
                    vContainerBuilder.Register<IFirstService, FirstService>(Lifetime.Singleton);
                    vContainerBuilder.Register<ISecondService, SecondService>(Lifetime.Singleton);
                    vContainerBuilder.Register<IThirdService, ThirdService>(Lifetime.Singleton);
                    vContainerBuilder.Register<ISubObjectA, SubObjectA>(Lifetime.Transient);
                    vContainerBuilder.Register<ISubObjectB, SubObjectB>(Lifetime.Transient);
                    vContainerBuilder.Register<ISubObjectC, SubObjectC>(Lifetime.Transient);
                    vContainerBuilder.Register<IComplex1, Complex1>(Lifetime.Transient);
                    vContainerBuilder.Register<IComplex2, Complex2>(Lifetime.Transient);
                    vContainerBuilder.Register<IComplex3, Complex3>(Lifetime.Transient);
                    vContainerBuilder.Register<ISubObjectOne, SubObjectOne>(Lifetime.Transient);
                    vContainerBuilder.Register<ISubObjectTwo, SubObjectTwo>(Lifetime.Transient);
                    vContainerBuilder.Register<ISubObjectThree, SubObjectThree>(Lifetime.Transient);
                    var vContainer = vContainerBuilder.Build();
                    
                    vContainer.Resolve<IComplex1>();
                    vContainer.Resolve<IComplex2>();
                    vContainer.Resolve<IComplex3>();
                })
                .SampleGroup(new SampleGroup("VContainer", SampleUnit.Nanosecond))
                .GC()
                .WarmupCount(WarmupCount)
                .MeasurementCount(MeasurementCount)
                .Run();
            
            Measure
                .Method(() =>
                {
                    var zenjectContainer = new Zenject.DiContainer();
                    zenjectContainer.Bind<IFirstService>().To<FirstService>().AsSingle();
                    zenjectContainer.Bind<ISecondService>().To<SecondService>().AsSingle();
                    zenjectContainer.Bind<IThirdService>().To<ThirdService>().AsSingle();
                    zenjectContainer.Bind<ISubObjectA>().To<SubObjectA>().AsTransient();
                    zenjectContainer.Bind<ISubObjectB>().To<SubObjectB>().AsTransient();
                    zenjectContainer.Bind<ISubObjectC>().To<SubObjectC>().AsTransient();
                    zenjectContainer.Bind<IComplex1>().To<Complex1>().AsTransient();
                    zenjectContainer.Bind<IComplex2>().To<Complex2>().AsTransient();
                    zenjectContainer.Bind<IComplex3>().To<Complex3>().AsTransient();
                    zenjectContainer.Bind<ISubObjectOne>().To<SubObjectOne>().AsTransient();
                    zenjectContainer.Bind<ISubObjectTwo>().To<SubObjectTwo>().AsTransient();
                    zenjectContainer.Bind<ISubObjectThree>().To<SubObjectThree>().AsTransient();
                    
                    zenjectContainer.Resolve<IComplex1>();
                    zenjectContainer.Resolve<IComplex2>();
                    zenjectContainer.Resolve<IComplex3>();
                })
                .SampleGroup(new SampleGroup("Zenject", SampleUnit.Nanosecond))
                .GC()
                .MeasurementCount(MeasurementCount)
                .WarmupCount(WarmupCount)
                .Run();
        }
    }
}

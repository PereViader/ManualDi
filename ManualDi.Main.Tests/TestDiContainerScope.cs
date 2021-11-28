using NSubstitute;
using NUnit.Framework;

namespace ManualDi.Main.Tests
{
    public class TestDiContainerNestedResolutions
    {
        public interface IParent
        {
            void Inject(IChildInject childInject);
            void Init(IChildInit childInit);
        }

        public interface IChildInject
        {
            void Inject();
            void Init();
        }

        public interface IChildInit
        {
            void Inject();
            void Init();
        }

        [Test]
        public void TestCallOrder()
        {
            var parent = Substitute.For<IParent>();
            var childInject = Substitute.For<IChildInject>();
            var childInit = Substitute.For<IChildInit>();

            var container = new DiContainerBuilder().WithInstallDelegate(x =>
            {
                x.Bind<IParent>()
                    .FromInstance(parent)
                    .Inject((o, c) => o.Inject(c.Resolve<IChildInject>()))
                    .Initialize((o, c) => o.Init(c.Resolve<IChildInit>()));

                x.Bind<IChildInject>()
                    .FromInstance(childInject)
                    .Inject((o, c) => o.Inject())
                    .Initialize((o, c) => o.Init());

                x.Bind<IChildInit>()
                    .FromInstance(childInit)
                    .Inject((o, c) => o.Inject())
                    .Initialize((o, c) => o.Init());
            }).Build();

            _ = container.Resolve<IParent>();

            Received.InOrder(() =>
            {
                childInject.Inject();
                parent.Inject(Arg.Is(childInject));
                childInject.Init();
                childInit.Inject();
                childInit.Init();
                parent.Init(Arg.Is(childInit));
            });
        }
    }

    public class TestDiContainerScope
    {
        [Test]
        public void TestSingle()
        {
            var factoryMethodDelegate = Substitute.For<FactoryMethodDelegate<object>>();
            var injectionDelegate = Substitute.For<InjectionDelegate<object>>();

            factoryMethodDelegate.Invoke(Arg.Any<IDiContainer>()).Returns(c => new object());

            var container = new DiContainerBuilder().WithInstallDelegate(x =>
            {
                x.Bind<object>().FromMethod(factoryMethodDelegate).Inject(injectionDelegate).Single();
            }).Build();

            var resolution1 = container.Resolve<object>();
            var resolution2 = container.Resolve<object>();

            Assert.That(resolution1, Is.EqualTo(resolution2));

            injectionDelegate.Received(1).Invoke(Arg.Any<object>(), Arg.Any<IDiContainer>());
            factoryMethodDelegate.Received(1).Invoke(Arg.Any<IDiContainer>());
        }

        [Test]
        public void TestTransient()
        {
            var factoryMethodDelegate = Substitute.For<FactoryMethodDelegate<object>>();
            var injectionDelegate = Substitute.For<InjectionDelegate<object>>();

            factoryMethodDelegate.Invoke(Arg.Any<IDiContainer>()).Returns(c => new object());

            var container = new DiContainerBuilder().WithInstallDelegate(x =>
            {
                x.Bind<object>().FromMethod(factoryMethodDelegate).Inject(injectionDelegate).Transient();
            }).Build();

            var resolution1 = container.Resolve<object>();
            var resolution2 = container.Resolve<object>();

            Assert.That(resolution1, Is.Not.EqualTo(resolution2));

            injectionDelegate.Received(2).Invoke(Arg.Any<object>(), Arg.Any<IDiContainer>());
            factoryMethodDelegate.Received(2).Invoke(Arg.Any<IDiContainer>());
        }
    }
}

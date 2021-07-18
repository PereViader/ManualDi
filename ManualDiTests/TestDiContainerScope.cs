using NSubstitute;
using NUnit.Framework;

namespace ManualDi.Tests
{
    public class TestDiContainerScope
    {
        private IDiContainer container;

        [SetUp]
        public void SetUp()
        {
            container = new ContainerBuilder().Build();
        }

        [Test]
        public void TestSingle()
        {
            var factoryMethodDelegate = Substitute.For<FactoryMethodDelegate<object>>();
            var injectionDelegate = Substitute.For<InjectionDelegate<object>>();

            factoryMethodDelegate.Invoke(Arg.Any<IDiContainer>()).Returns(c => new object());

            container.BindAndFinish<object>(b => b.FromMethod(factoryMethodDelegate).Inject(injectionDelegate).Single());

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

            container.BindAndFinish<object>(b => b.FromMethod(factoryMethodDelegate).Inject(injectionDelegate).Transient());

            var resolution1 = container.Resolve<object>();
            var resolution2 = container.Resolve<object>();

            Assert.That(resolution1, Is.Not.EqualTo(resolution2));

            injectionDelegate.Received(2).Invoke(Arg.Any<object>(), Arg.Any<IDiContainer>());
            factoryMethodDelegate.Received(2).Invoke(Arg.Any<IDiContainer>());
        }
    }
}

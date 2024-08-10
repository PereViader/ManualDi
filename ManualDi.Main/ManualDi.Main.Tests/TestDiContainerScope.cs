using NSubstitute;
using NUnit.Framework;

namespace ManualDi.Main.Tests
{
    public class TestDiContainerScope
    {
        [Test]
        public void TestSingle()
        {
            var factoryMethodDelegate = Substitute.For<CreateDelegate<object>>();
            var injectionDelegate = Substitute.For<InjectionDelegate<object>>();

            factoryMethodDelegate.Invoke(Arg.Any<IDiContainer>()).Returns(c => new object());

            var container = new DiContainerBuilder().Install(x =>
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
            var factoryMethodDelegate = Substitute.For<CreateDelegate<object>>();
            var injectionDelegate = Substitute.For<InjectionDelegate<object>>();

            factoryMethodDelegate.Invoke(Arg.Any<IDiContainer>()).Returns(c => new object());

            var container = new DiContainerBuilder().Install(x =>
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

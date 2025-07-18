using NSubstitute;
using NUnit.Framework;

namespace ManualDi.Sync.Tests
{
    public class TestDiContainerScope
    {
        [Test]
        public void TestSingle()
        {
            var factoryMethodDelegate = Substitute.For<FromDelegate>();
            var injectionDelegate = Substitute.For<InstanceContainerDelegate>();

            factoryMethodDelegate.Invoke(Arg.Any<IDiContainer>()).Returns(c => new object());

            var container = new DiContainerBindings().Install(b =>
            {
                b.Bind<object>().FromMethod(factoryMethodDelegate).Inject(injectionDelegate);
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
            var factoryMethodDelegate = Substitute.For<FromDelegate>();
            var injectionDelegate = Substitute.For<InstanceContainerDelegate>();

            factoryMethodDelegate.Invoke(Arg.Any<IDiContainer>()).Returns(c => new object());

            var container = new DiContainerBindings().Install(b =>
            {
                b.Bind<object>().Transient().FromMethod(factoryMethodDelegate).Inject(injectionDelegate);
            }).Build();

            var resolution1 = container.Resolve<object>();
            var resolution2 = container.Resolve<object>();

            Assert.That(resolution1, Is.Not.EqualTo(resolution2));

            injectionDelegate.Received(2).Invoke(Arg.Any<object>(), Arg.Any<IDiContainer>());
            factoryMethodDelegate.Received(2).Invoke(Arg.Any<IDiContainer>());
        }
    }
}

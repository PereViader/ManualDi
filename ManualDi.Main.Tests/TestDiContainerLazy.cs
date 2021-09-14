using NSubstitute;
using NUnit.Framework;

namespace ManualDi.Main.Tests
{
    public class TestDiContainerLazy
    {
        private IDiContainer container;

        [SetUp]
        public void SetUp()
        {
            container = new ContainerBuilder().Build();
        }

        [Test]
        public void TestLazy()
        {
            var builderFunc = Substitute.For<FactoryMethodDelegate<object>>();

            container.Bind<object>().FromMethod(builderFunc).Lazy();

            container.FinishBinding();

            builderFunc.DidNotReceive().Invoke(Arg.Any<IDiContainer>());
        }

        [Test]
        public void TestNonLazy()
        {
            var builderFunc = Substitute.For<FactoryMethodDelegate<object>>();

            container.Bind<object>().FromMethod(builderFunc).NonLazy();

            container.FinishBinding();

            builderFunc.Received(1).Invoke(Arg.Any<IDiContainer>());
        }
    }
}

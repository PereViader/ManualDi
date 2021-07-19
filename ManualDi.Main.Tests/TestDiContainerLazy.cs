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

            container.BindAndFinish<object>(b => b.FromMethod(builderFunc).Lazy());

            builderFunc.DidNotReceive().Invoke(Arg.Any<IDiContainer>());
        }

        [Test]
        public void TestNonLazy()
        {
            var builderFunc = Substitute.For<FactoryMethodDelegate<object>>();

            container.BindAndFinish<object>(b => b.FromMethod(builderFunc).NonLazy());

            builderFunc.Received(1).Invoke(Arg.Any<IDiContainer>());
        }
    }
}

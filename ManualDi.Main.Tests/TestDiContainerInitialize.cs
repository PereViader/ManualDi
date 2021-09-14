using ManualDi.Main.Initialization;
using NSubstitute;
using NUnit.Framework;

namespace ManualDi.Main.Tests
{
    public class TestDiContainerInitialize
    {
        private IDiContainer container;

        [SetUp]
        public void SetUp()
        {
            container = new ContainerBuilder().Build();
        }

        [Test]
        public void TestInitialize()
        {
            var instance = new object();
            var initializationDelegate = Substitute.For<InitializationDelegate<object>>();
            container.Bind<object>()
                .FromInstance(instance)
                .Initialize(initializationDelegate);

            _ = container.FinishAndResolve<object>();
            initializationDelegate.Received(1).Invoke(Arg.Is(instance), Arg.Is(container));
        }
    }
}

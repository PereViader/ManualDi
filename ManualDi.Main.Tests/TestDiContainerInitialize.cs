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
            _ = container.BindFinishAndResolve<object>(b => b
                .FromInstance(instance)
                .Initialize(initializationDelegate)
                );
            initializationDelegate.Received(1).Invoke(Arg.Is(instance), Arg.Is(container));
        }
    }
}

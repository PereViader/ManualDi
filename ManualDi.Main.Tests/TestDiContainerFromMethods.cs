using NUnit.Framework;

namespace ManualDi.Main.Tests
{
    public class TestDiContainerFromMethods
    {
        private IDiContainer container;

        [SetUp]
        public void SetUp()
        {
            container = new ContainerBuilder().Build();
        }

        [Test]
        public void TestFromInstance()
        {
            var instance = new object();
            var resolved = container.BindFinishAndResolve<object>(b => b.FromInstance(instance));
            Assert.That(resolved, Is.EqualTo(instance));
        }

        [Test]
        public void TestFromMethod()
        {
            var instance = new object();
            var resolved = container.BindFinishAndResolve<object>(b => b.FromMethod(c => instance));
            Assert.That(resolved, Is.EqualTo(instance));
        }

        [Test]
        public void TestFromContainer()
        {
            int instance = 5;
            container.Bind<int>(b => b.FromInstance(instance));
            var resolved = container.BindFinishAndResolve<object>(b => b.FromContainer<int, object>());
            Assert.That(resolved, Is.EqualTo(instance));
        }
    }
}

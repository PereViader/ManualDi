using NUnit.Framework;
using System.Collections.Generic;

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
            var resolved = container.BindFinishAndResolve<object, int>(b => b.FromContainer());
            Assert.That(resolved, Is.EqualTo(instance));
        }

        [Test]
        public void TestFromContainerAll()
        {
            container.Bind<int>(b => b.FromInstance(1));
            container.Bind<int>(b => b.FromInstance(2));

            List<object> resolved = container.BindFinishAndResolve<List<object>, List<int>>(b => b.FromContainerAll());

            Assert.That(resolved, Is.EquivalentTo(new[] { 1, 2 }));
        }
    }
}

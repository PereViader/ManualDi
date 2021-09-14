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
            container.Bind<object>().FromInstance(instance);

            var resolved = container.FinishAndResolve<object>();
            Assert.That(resolved, Is.EqualTo(instance));
        }

        [Test]
        public void TestFromMethod()
        {
            var instance = new object();
            container.Bind<object>().FromMethod(c => instance);

            var resolved = container.FinishAndResolve<object>();
            Assert.That(resolved, Is.EqualTo(instance));
        }

        [Test]
        public void TestFromContainer()
        {
            int instance = 5;
            container.Bind<int>().FromInstance(instance);
            container.Bind<object, int>().FromContainer();

            var resolved = container.FinishAndResolve<object>();
            Assert.That(resolved, Is.EqualTo(instance));
        }

        [Test]
        public void TestFromContainerAll()
        {
            container.Bind<int>().FromInstance(1);
            container.Bind<int>().FromInstance(2);
            container.Bind<List<object>, List<int>>().FromContainerAll();

            var resolved = container.FinishAndResolve<List<object>>();
            Assert.That(resolved, Is.EquivalentTo(new[] { 1, 2 }));
        }
    }
}

using NUnit.Framework;

namespace ManualDi.Main.Tests
{
    public class TestDiContainerMetadata
    {
        private IDiContainer container;

        [SetUp]
        public void SetUp()
        {
            container = new ContainerBuilder().Build();
        }

        [Test]
        public void TestOnlyKeyMetadata()
        {
            var instance1 = new object();
            var instance2 = new object();

            container.Bind<object>(b => b.FromInstance(instance1).WithMetadata(nameof(instance1)));
            container.Bind<object>(b => b.FromInstance(instance2).WithMetadata(nameof(instance2)));

            var resolution1 = container.Resolve<object>(b => b.WhereMetadata(nameof(instance1)));
            var resolution2 = container.Resolve<object>(b => b.WhereMetadata(nameof(instance2)));

            Assert.That(instance1, Is.EqualTo(resolution1));
            Assert.That(instance2, Is.EqualTo(resolution2));
        }

        [Test]
        public void TestKeyValueMetadata()
        {
            var instance1 = new object();
            var instance2 = new object();

            container.Bind<object>(b => b.FromInstance(instance1).WithMetadata("Key", 5));
            container.Bind<object>(b => b.FromInstance(instance2).WithMetadata("Key", 10));

            var resolution1 = container.Resolve<object>(b => b.WhereMetadata("Key", 5));
            var resolution2 = container.Resolve<object>(b => b.WhereMetadata("Key", 10));

            Assert.That(instance1, Is.EqualTo(resolution1));
            Assert.That(instance2, Is.EqualTo(resolution2));
        }

        [Test]
        public void TestKeyValueExpressionMetadata()
        {
            var instance1 = new object();
            var instance2 = new object();

            container.Bind<object>(b => b.FromInstance(instance1).WithMetadata("Key", 5));
            container.Bind<object>(b => b.FromInstance(instance2).WithMetadata("Key", 10));

            var resolution1 = container.Resolve<object>(b => b.WhereMetadata(x => x.Get<int>("Key") < 6));
            var resolution2 = container.Resolve<object>(b => b.WhereMetadata(x => x.Get<int>("Key") > 6));

            Assert.That(instance1, Is.EqualTo(resolution1));
            Assert.That(instance2, Is.EqualTo(resolution2));
        }
    }
}

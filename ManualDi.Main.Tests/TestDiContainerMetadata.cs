using NUnit.Framework;

namespace ManualDi.Main.Tests
{
    public class TestDiContainerMetadata
    {
        [Test]
        public void TestOnlyKeyMetadata()
        {
            var instance1 = new object();
            var instance2 = new object();

            var container = new DiContainerBuilder().WithInstallDelegate(x =>
            {
                x.Bind<object>().FromInstance(instance1).WithMetadata(nameof(instance1));
                x.Bind<object>().FromInstance(instance2).WithMetadata(nameof(instance2));
            }).Build();

            var resolution1 = container.Resolve<object>(b => b.WhereMetadata(nameof(instance1)));
            var resolution2 = container.Resolve<object>(b => b.WhereMetadata(nameof(instance2)));

            Assert.That(resolution1, Is.EqualTo(instance1));
            Assert.That(resolution2, Is.EqualTo(instance2));
        }

        [Test]
        public void TestKeyValueMetadata()
        {
            var instance1 = new object();
            var instance2 = new object();

            var container = new DiContainerBuilder().WithInstallDelegate(x =>
            {
                x.Bind<object>().FromInstance(instance1).WithMetadata("Key", 5);
                x.Bind<object>().FromInstance(instance2).WithMetadata("Key", 10);
            }).Build();

            var resolution1 = container.Resolve<object>(b => b.WhereMetadata("Key", 5));
            var resolution2 = container.Resolve<object>(b => b.WhereMetadata("Key", 10));

            Assert.That(resolution1, Is.EqualTo(instance1));
            Assert.That(resolution2, Is.EqualTo(instance2));
        }

        [Test]
        public void TestKeyValueExpressionMetadata()
        {
            var instance1 = new object();
            var instance2 = new object();

            var container = new DiContainerBuilder().WithInstallDelegate(x =>
            {
                x.Bind<object>().FromInstance(instance1).WithMetadata("Key", 5);
                x.Bind<object>().FromInstance(instance2).WithMetadata("Key", 10);
            }).Build();

            var resolution1 = container.Resolve<object>(b => b.WhereMetadata(x => x.Get<int>("Key") < 6));
            var resolution2 = container.Resolve<object>(b => b.WhereMetadata(x => x.Get<int>("Key") > 6));

            Assert.That(resolution1, Is.EqualTo(instance1));
            Assert.That(resolution2, Is.EqualTo(instance2));
        }
    }
}

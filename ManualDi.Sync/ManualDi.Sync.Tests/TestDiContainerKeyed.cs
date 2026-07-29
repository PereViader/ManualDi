using NUnit.Framework;

namespace ManualDi.Sync.Tests
{
    public class TestDiContainerKeyed
    {
        public struct PrimaryKey { }
        public struct SecondaryKey { }

        public interface IStorage
        {
            string Name { get; }
        }

        [ManualDi]
        public class SqlStorage : IStorage
        {
            public string Name => "Sql";
        }

        [ManualDi]
        public class CloudStorage : IStorage
        {
            public string Name => "Cloud";
        }

        [Test]
        public void TestBindKeyed_ResolvesCorrectInstances()
        {
            using var container = new DiContainerBindings().Install(b =>
            {
                b.BindKeyed<IStorage, SqlStorage, PrimaryKey>().Default().FromConstructor();
                b.BindKeyed<IStorage, CloudStorage, SecondaryKey>().Default().FromConstructor();
            }).Build();

            var primary = container.ResolveKeyed<IStorage, PrimaryKey>();
            var secondary = container.ResolveKeyed<IStorage, SecondaryKey>();

            Assert.That(primary, Is.TypeOf<SqlStorage>());
            Assert.That(primary.Name, Is.EqualTo("Sql"));

            Assert.That(secondary, Is.TypeOf<CloudStorage>());
            Assert.That(secondary.Name, Is.EqualTo("Cloud"));
        }

        [Test]
        public void TestBindKeyed_SingleLifecycleShared()
        {
            using var container = new DiContainerBindings().Install(b =>
            {
                b.BindKeyed<IStorage, SqlStorage, PrimaryKey>().Default().FromConstructor();
            }).Build();

            var instance1 = container.ResolveKeyed<IStorage, PrimaryKey>();
            var instance2 = container.ResolveKeyed<IStorage, PrimaryKey>();
            var instanceDirect = container.Resolve<IStorage>();

            Assert.That(instance1, Is.SameAs(instance2));
            Assert.That(instance1, Is.SameAs(instanceDirect));
        }

        [Test]
        public void TestBindKeyed_TransientLifecycleCreatesNew()
        {
            using var container = new DiContainerBindings().Install(b =>
            {
                b.BindKeyed<IStorage, SqlStorage, PrimaryKey>().Transient().FromConstructor();
            }).Build();

            var instance1 = container.ResolveKeyed<IStorage, PrimaryKey>();
            var instance2 = container.ResolveKeyed<IStorage, PrimaryKey>();

            Assert.That(instance1, Is.Not.SameAs(instance2));
        }

        [Test]
        public void TestResolveKeyedNullable_ReturnsNullWhenNotBound()
        {
            using var container = new DiContainerBindings().Build();

            var result = container.ResolveKeyedNullable<IStorage, PrimaryKey>();

            Assert.That(result, Is.Null);
        }
    }
}

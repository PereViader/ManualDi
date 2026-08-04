using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ManualDi.Async.Tests
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
        public async Task TestBindKeyed_ResolvesCorrectInstances()
        {
            await using var container = await new DiContainerBindings().Install(b =>
            {
                b.BindKeyed<IStorage, SqlStorage, PrimaryKey>().Default().FromConstructor();
                b.BindKeyed<IStorage, CloudStorage, SecondaryKey>().Default().FromConstructor();
            }).Build(CancellationToken.None);

            var primary = container.ResolveKeyed<IStorage, PrimaryKey>();
            var secondary = container.ResolveKeyed<IStorage, SecondaryKey>();

            Assert.That(primary, Is.TypeOf<SqlStorage>());
            Assert.That(primary.Name, Is.EqualTo("Sql"));

            Assert.That(secondary, Is.TypeOf<CloudStorage>());
            Assert.That(secondary.Name, Is.EqualTo("Cloud"));
        }

        [Test]
        public async Task TestBindKeyed_SingleLifecycleShared()
        {
            await using var container = await new DiContainerBindings().Install(b =>
            {
                b.BindKeyed<IStorage, SqlStorage, PrimaryKey>().Default().FromConstructor();
            }).Build(CancellationToken.None);

            var instance1 = container.ResolveKeyed<IStorage, PrimaryKey>();
            var instance2 = container.ResolveKeyed<IStorage, PrimaryKey>();
            var instanceDirect = container.Resolve<IStorage>();

            Assert.That(instance1, Is.SameAs(instance2));
            Assert.That(instance1, Is.SameAs(instanceDirect));
        }

        [Test]
        public async Task TestResolveKeyedNullable_ReturnsNullWhenNotBound()
        {
            await using var container = await new DiContainerBindings().Build(CancellationToken.None);

            var result = container.ResolveKeyedNullable<IStorage, PrimaryKey>();

            Assert.That(result, Is.Null);
        }
    }
}

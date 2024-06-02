using NUnit.Framework;

namespace ManualDi.Main.Tests
{
    public class TestDiContianerTryResolve
    {
        [Test]
        public void TestTryResolveGenericSuccess()
        {
            var container = new DiContainerBuilder().Install(x =>
            {
                x.Bind<int>().FromInstance(1);
            }).Build();

            var success = container.TryResolve<int>(out int resolution);
            Assert.That(success, Is.True);
            Assert.That(resolution, Is.EqualTo(1));
        }

        [Test]
        public void TestTryResolveGenericFailure()
        {
            var container = new DiContainerBuilder().Build();

            var success = container.TryResolve<int>(out _);
            Assert.That(success, Is.False);
        }

        [Test]
        public void TestTryResolveNonGenericSuccess()
        {
            var container = new DiContainerBuilder().Install(x =>
            {
                x.Bind<int>().FromInstance(1);
            }).Build();

            var success = container.TryResolve(typeof(int), out object resolution);
            Assert.That(success, Is.True);
            Assert.That(resolution, Is.EqualTo(1));
        }

        [Test]
        public void TestTryResolveNonGenericFailure()
        {
            var container = new DiContainerBuilder().Build();

            var success = container.TryResolve(typeof(int), out _);
            Assert.That(success, Is.False);
        }
    }
}

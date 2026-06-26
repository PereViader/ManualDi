using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace ManualDi.Async.Tests
{
    public class TestDiContainerCancellationTokenSource
    {
        [Test]
        public async Task TestWithCancellationTokenSource_UsesProvidedInstance()
        {
            var cts = new CancellationTokenSource();
            
            var container = await new DiContainerBindings()
                .WithCancellationTokenSource(cts)
                .Build(CancellationToken.None);

            Assert.That(container.CancellationToken, Is.EqualTo(cts.Token));
            Assert.That(cts.IsCancellationRequested, Is.False);

            cts.Cancel();
            Assert.That(container.CancellationToken.IsCancellationRequested, Is.True);
        }

        [Test]
        public async Task TestWithCancellationTokenSource_IsCancelledAndDisposedOnContainerDispose()
        {
            var cts = new CancellationTokenSource();
            
            var container = await new DiContainerBindings()
                .WithCancellationTokenSource(cts)
                .Build(CancellationToken.None);

            await container.DisposeAsync();

            Assert.That(cts.IsCancellationRequested, Is.True);
            // After CancellationTokenSource is disposed, accessing Token might throw ObjectDisposedException
            Assert.Throws<System.ObjectDisposedException>(() => _ = cts.Token);
        }

        [Test]
        public async Task TestWithCancellationTokenSource_BuildCancellationTokenCancelsContainer()
        {
            var cts = new CancellationTokenSource();
            using var buildCts = new CancellationTokenSource();
            
            var container = await new DiContainerBindings()
                .WithCancellationTokenSource(cts)
                .Build(buildCts.Token);

            Assert.That(container.CancellationToken.IsCancellationRequested, Is.False);
            Assert.That(cts.IsCancellationRequested, Is.False);

            buildCts.Cancel();

            Assert.That(container.CancellationToken.IsCancellationRequested, Is.True);
            Assert.That(cts.IsCancellationRequested, Is.True);
        }

        [Test]
        public async Task TestDefault_BuildCancellationTokenCancelsContainer()
        {
            using var buildCts = new CancellationTokenSource();
            
            var container = await new DiContainerBindings()
                .Build(buildCts.Token);

            Assert.That(container.CancellationToken.IsCancellationRequested, Is.False);

            buildCts.Cancel();

            Assert.That(container.CancellationToken.IsCancellationRequested, Is.True);
        }
    }
}

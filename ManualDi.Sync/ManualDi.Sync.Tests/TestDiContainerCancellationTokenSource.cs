using NUnit.Framework;
using System.Threading;

namespace ManualDi.Sync.Tests
{
    public class TestDiContainerCancellationTokenSource
    {
        [Test]
        public void TestWithCancellationTokenSource_UsesProvidedInstance()
        {
            var cts = new CancellationTokenSource();
            
            var container = new DiContainerBindings()
                .WithCancellationTokenSource(cts)
                .Build();

            Assert.That(container.CancellationToken, Is.EqualTo(cts.Token));
            Assert.That(cts.IsCancellationRequested, Is.False);

            cts.Cancel();
            Assert.That(container.CancellationToken.IsCancellationRequested, Is.True);
        }

        [Test]
        public void TestWithCancellationTokenSource_IsCancelledAndDisposedOnContainerDispose()
        {
            var cts = new CancellationTokenSource();
            
            var container = new DiContainerBindings()
                .WithCancellationTokenSource(cts)
                .Build();

            container.Dispose();

            Assert.That(cts.IsCancellationRequested, Is.True);
        }
    }
}

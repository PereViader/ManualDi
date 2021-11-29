using NSubstitute;
using NUnit.Framework;
using System;

namespace ManualDi.Main.Tests
{
    public class TestDiContainerBindings
    {
        [Test]
        public void TestQueueDispose()
        {
            var action = Substitute.For<Action>();

            var container = new DiContainerBuilder()
                .WithInstallDelegate(x => x.QueueDispose(action))
                .Build();

            action.DidNotReceive().Invoke();

            container.Dispose();

            action.Received(1).Invoke();
        }
    }
}

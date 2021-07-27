using NSubstitute;
using NUnit.Framework;
using System;

namespace ManualDi.Main.Tests
{
    public class TestDiContainerDispose
    {
        [Test]
        public void TestDispose()
        {
            var container = new ContainerBuilder().Build();
            var instance = new object();
            var disposeAction = Substitute.For<Action>();

            _ = container.BindFinishAndResolve<object>(b => b.FromInstance(instance).RegisterDispose(o => disposeAction));

            disposeAction.DidNotReceive().Invoke();

            container.Dispose();

            disposeAction.Received(1).Invoke();
            disposeAction.ClearReceivedCalls();

            container.Dispose();

            disposeAction.DidNotReceive().Invoke();
        }
    }
}

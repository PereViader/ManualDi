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
            var instance = new object();
            var disposeAction = Substitute.For<Action>();

            IDiContainer container = new DiContainerBuilder().Install(x =>
            {
                x.Bind<object>()
                    .FromInstance(instance)
                    .Dispose((o, c) => disposeAction);
            }).Build();

            _ = container.Resolve<object>();

            disposeAction.DidNotReceive().Invoke();

            container.Dispose();

            disposeAction.Received(1).Invoke();
            disposeAction.ClearReceivedCalls();

            container.Dispose();

            disposeAction.DidNotReceive().Invoke();
        }
    }
}

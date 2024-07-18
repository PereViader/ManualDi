using NSubstitute;
using NUnit.Framework;
using System;

namespace ManualDi.Main.Tests
{
    public class TestDiContainerDispose
    {
        public interface IA : IDisposable { }
        public interface IB : IDisposable { }

        [Test]
        public void TestDisposeCalledByDefault()
        {
            IDiContainer container = new DiContainerBuilder().Install(x =>
            {
                x.Bind<IDisposable>().FromInstance(Substitute.For<IDisposable>());
            }).Build();

            var disposable = container.Resolve<IDisposable>();
            
            container.Dispose();
            
            disposable.Received(1).Dispose();
        }
        
        [Test]
        public void TestDontDisposePreventsDispose()
        {
            IDiContainer container = new DiContainerBuilder().Install(x =>
            {
                x.Bind<IDisposable>().FromInstance(Substitute.For<IDisposable>()).DontDispose();
            }).Build();

            var disposable = container.Resolve<IDisposable>();
            
            container.Dispose();
            
            disposable.DidNotReceive().Dispose();
        }
        
        [Test]
        public void TestDisposeOrder()
        {
            var disposable1 = Substitute.For<IA>();
            var disposable2 = Substitute.For<IB>();
            
            IDiContainer container = new DiContainerBuilder().Install(x =>
            {
                x.Bind<IA>().FromMethod(c =>
                    {
                        _ = c.Resolve<IB>();
                        return disposable1;
                    });

                x.Bind<IB>().FromInstance(disposable2);
            }).Build();

            _ = container.Resolve<IA>();
            
            container.Dispose();
            
            Received.InOrder(() => {
                disposable2.Dispose();
                disposable1.Dispose();
            });
        }
        
        [Test]
        public void TestDisposeCustom()
        {
            var instance = new object();
            var disposeAction = Substitute.For<Action>();

            IDiContainer container = new DiContainerBuilder().Install(x =>
            {
                x.Bind<object>()
                    .FromInstance(instance)
                    .Dispose((_, _) => disposeAction);
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

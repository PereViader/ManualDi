using NSubstitute;
using NUnit.Framework;

namespace ManualDi.Main.Tests
{
    public class TestDiContainerNestedResolutions
    {
        public interface IParent
        {
            void Inject(IChildInject childInject);
            void Init(IChildInit childInit);
        }

        public interface IChildInject
        {
            void Inject();
            void Init();
        }

        public interface IChildInit
        {
            void Inject();
            void Init();
        }

        [Test]
        public void TestCallOrder()
        {
            var parent = Substitute.For<IParent>();
            var childInject = Substitute.For<IChildInject>();
            var childInit = Substitute.For<IChildInit>();

            var container = new DiContainerBuilder().Install(x =>
            {
                x.Bind<IParent>()
                    .FromInstance(parent)
                    .Inject((o, c) => o.Inject(c.Resolve<IChildInject>()))
                    .Initialize((o, c) => o.Init(c.Resolve<IChildInit>()));

                x.Bind<IChildInject>()
                    .FromInstance(childInject)
                    .Inject((o, c) => o.Inject())
                    .Initialize((o, c) => o.Init());

                x.Bind<IChildInit>()
                    .FromInstance(childInit)
                    .Inject((o, c) => o.Inject())
                    .Initialize((o, c) => o.Init());
            }).Build();

            _ = container.Resolve<IParent>();

            Received.InOrder(() =>
            {
                childInject.Inject();
                parent.Inject(Arg.Is(childInject));
                childInject.Init();
                childInit.Inject();
                childInit.Init();
                parent.Init(Arg.Is(childInit));
            });
        }
    }
}

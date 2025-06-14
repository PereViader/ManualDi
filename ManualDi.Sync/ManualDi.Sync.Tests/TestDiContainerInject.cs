using NSubstitute;
using NUnit.Framework;

namespace ManualDi.Sync.Tests;

public class TestDiContainerInject
{
    [Test]
    public void TestInject()
    {
        var instance = new object();
        var injectMethod = Substitute.For<InstanceContainerDelegate>();

        var container = new DiContainerBindings().Install(b =>
        {
            b.Bind<object>()
                .FromInstance(instance)
                .Inject(injectMethod)
                .Inject(injectMethod);
        }).Build();

        _ = container.Resolve<object>();

        injectMethod.Received(2).Invoke(Arg.Is(instance), Arg.Is(container));
    }
}
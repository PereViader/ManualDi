using NSubstitute;
using NUnit.Framework;

namespace ManualDi.Main.Tests;

public class TestDiContainerInject
{
    [Test]
    public void TestInject()
    {
        var instance = new object();
        var injectMethod = Substitute.For<InjectionDelegate<object>>();

        var container = new DiContainerBindings().Install(x =>
        {
            x.Bind<object>()
                .FromInstance(instance)
                .Inject(injectMethod)
                .Inject(injectMethod);
        }).Build();

        _ = container.Resolve<object>();

        injectMethod.Received(2).Invoke(Arg.Is(instance), Arg.Is(container));
    }
}
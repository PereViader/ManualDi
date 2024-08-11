using NSubstitute;
using NUnit.Framework;

namespace ManualDi.Main.Tests;

public class TestDiContainerInitialize
{
    [Test]
    public void TestInitialize()
    {
        var instance = new object();
        var initializationDelegate = Substitute.For<InitializationDelegate<object>>();

        var container = new DiContainerBindings().Install(x =>
        {
            x.Bind<object>()
                .FromInstance(instance)
                .Initialize(initializationDelegate);
        }).Build();

        _ = container.Resolve<object>();

        initializationDelegate.Received(1).Invoke(Arg.Is(instance), Arg.Is(container));
    }
}
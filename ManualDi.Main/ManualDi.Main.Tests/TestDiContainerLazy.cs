using NSubstitute;
using NUnit.Framework;

namespace ManualDi.Main.Tests;

public class TestDiContainerLazy
{
    [Test]
    public void TestLazy()
    {
        var builderFunc = Substitute.For<CreateDelegate<object>>();

        var container = new DiContainerBindings().Install(b =>
        {
            b.Bind<object>().FromMethod(builderFunc).Lazy();
        }).Build();

        builderFunc.DidNotReceive().Invoke(Arg.Any<IDiContainer>());
    }

    [Test]
    public void TestNonLazy()
    {
        var builderFunc = Substitute.For<CreateDelegate<object>>();
        builderFunc.Invoke(Arg.Any<IDiContainer>()).Returns(new object());

        var container = new DiContainerBindings().Install(b =>
        {
            b.Bind<object>().FromMethod(builderFunc).NonLazy();
        }).Build();

        builderFunc.Received(1).Invoke(Arg.Any<IDiContainer>());
    }
}
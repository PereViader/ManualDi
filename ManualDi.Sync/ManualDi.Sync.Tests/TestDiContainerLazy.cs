using NSubstitute;
using NUnit.Framework;

namespace ManualDi.Sync.Tests;

public class TestDiContainerLazy
{
    [Test]
    public void TestNonLazy()
    {
        var builderFunc = Substitute.For<FromDelegate>();
        builderFunc.Invoke(Arg.Any<IDiContainer>()).Returns(new object());

        using var _ = new DiContainerBindings().Install(b =>
        {
            b.Bind<object>().FromMethod(builderFunc);
        }).Build();

        builderFunc.Received(1).Invoke(Arg.Any<IDiContainer>());
    }
}
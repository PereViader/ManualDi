using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;

namespace ManualDi.Main.Tests
{
    public class TestDiContainerScope
    {
        [Test]
        public async Task TestMultipleResolutionsReturnsSame()
        {
            var factoryMethodDelegate = Substitute.For<FromDelegate<object>>();
            var injectionDelegate = Substitute.For<InjectDelegate<object>>();

            factoryMethodDelegate.Invoke(Arg.Any<IDiContainer>()).Returns(c => new object());

            var container = await new DiContainerBindings().Install(b =>
            {
                b.Bind<object>().FromMethod(factoryMethodDelegate, []).Inject(injectionDelegate);
            }).Build(CancellationToken.None);

            var resolution1 = container.Resolve<object>();
            var resolution2 = container.Resolve<object>();

            Assert.That(resolution1, Is.EqualTo(resolution2));

            injectionDelegate.Received(1).Invoke(Arg.Any<object>(), Arg.Any<IDiContainer>());
            factoryMethodDelegate.Received(1).Invoke(Arg.Any<IDiContainer>());
        }
    }
}

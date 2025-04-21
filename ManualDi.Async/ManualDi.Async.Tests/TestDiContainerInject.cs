using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;

namespace ManualDi.Async.Tests;

public class TestDiContainerInject
{
    [Test]
    public async Task TestInject()
    {
        var instance = new object();
        var injectMethod = Substitute.For<InjectDelegate>();

        await using var container = await new DiContainerBindings().Install(b =>
        {
            b.Bind<object>()
                .FromInstance(instance)
                .Inject(injectMethod)
                .Inject(injectMethod);
        }).Build(CancellationToken.None);

        _ = container.Resolve<object>();

        injectMethod.Received(2).Invoke(Arg.Is(instance), Arg.Is(container));
    }
}
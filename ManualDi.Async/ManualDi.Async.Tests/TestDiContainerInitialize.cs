using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;

namespace ManualDi.Async.Tests;

public class TestDiContainerInitialize
{
    [Test]
    public async Task TestInitialize()
    {
        var instance = new object();
        var initializationDelegate = Substitute.For<InitializeDelegate>();

        await using var container = await new DiContainerBindings().Install(b =>
        {
            b.Bind<object>()
                .FromInstance(instance)
                .Initialize(initializationDelegate);
        }).Build(CancellationToken.None);
        
        initializationDelegate.Received(1).Invoke(Arg.Is(instance));
    }
}
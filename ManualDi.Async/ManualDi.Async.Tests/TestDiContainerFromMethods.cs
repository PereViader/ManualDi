using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;

namespace ManualDi.Async.Tests;

public class TestDiContainerFromMethods
{
    [Test]
    public async Task TestFromInstance()
    {
        var instance = new object();

        await using var container = await new DiContainerBindings().Install(b =>
        {
            b.Bind<object>().FromInstance(instance);
        }).Build(CancellationToken.None);

        var resolved = container.Resolve<object>();
        Assert.That(resolved, Is.EqualTo(instance));
    }

    [Test]
    public async Task TestFromMethod()
    {
        var instance = new object();
        var fromDelegate = Substitute.For<FromDelegate>();
        fromDelegate.Invoke(Arg.Any<IDiContainer>()).Returns(instance);
        
        await using var container = await new DiContainerBindings().Install(b =>
        {
            b.Bind<object>().FromMethod(fromDelegate);
        }).Build(CancellationToken.None);

        var resolved = container.Resolve<object>();
        Assert.That(resolved, Is.EqualTo(instance));
        fromDelegate.Received(1).Invoke(Arg.Any<IDiContainer>());
    }

    [Test]
    public async Task TestFromContainer()
    {
        int instance = 5;
        await using var container = await new DiContainerBindings().Install(b =>
        {
            b.Bind<int>().FromInstance(instance);
            b.Bind<object, int>().FromContainerResolve();
        }).Build(CancellationToken.None);

        var resolved = container.Resolve<object>();
        Assert.That(resolved, Is.EqualTo(instance));
    }

    public class A;

    public class B(A a);
    
    [Test]
    public async Task TestFromSubContainerResolve()
    {
        await using var container = await new DiContainerBindings().Install(b =>
        {
            b.Bind<B>().FromSubContainerResolve(b =>
            {
                b.Bind<B>().Default().FromConstructor();
            },
            d =>
            {
                d.ConstructorDependency<A>();
            });
            
            b.Bind<A>().Default().FromConstructor();
        }).Build(CancellationToken.None);
    }
}
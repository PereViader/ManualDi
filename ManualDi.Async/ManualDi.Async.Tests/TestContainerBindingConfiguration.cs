using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ManualDi.Async.Tests;

public class TestContainerBindingConfiguration
{
    public class TestConfig;
    
    [Test]
    public async Task ResolveInstance_ProperlyResolveAfterInstanceIsAvailable()
    {
        var instance = new TestConfig();

        await using var container = await new DiContainerBindings()
            .Install(b =>
            {
                //Configs can't be resolved before they have been bound to the container
                var notFound = b.ResolveInstanceNullable<TestConfig>();
                var isNotFound = b.TryResolveInstance<TestConfig>(out _);
                Assert.That(notFound, Is.Null);
                Assert.That(isNotFound, Is.False);
                
                //Bind the config to the container
                b.Bind<TestConfig>().Default().FromInstance(instance);

                //Configs can be resolved at any time after they have been bound to the container
                var found = b.ResolveInstance<TestConfig>();
                Assert.That(found, Is.SameAs(instance));
            }).Build(CancellationToken.None);

        var resolvedInstance = container.Resolve<TestConfig>();
        Assert.That(resolvedInstance, Is.SameAs(instance));
    }
    
    [Test]
    public async Task ResolveInstance_UsingWithParentContainer_CanResolveInstanceOnSubContainer()
    {
        var instance = new TestConfig();

        await using var container = await new DiContainerBindings()
            .Install(b =>
            {
                //Bind the config to the container
                b.Bind<TestConfig>().Default().FromInstance(instance);
            }).Build(CancellationToken.None);

        await using var subContainer = await new DiContainerBindings()
            .WithParentContainer(container)
            .Install(b =>
            {
                //Sub container can resolve the instance from the parent container during installation
                var found = b.ResolveInstance<TestConfig>();
                Assert.That(found, Is.SameAs(instance));
            }).Build(CancellationToken.None);
    }
    
    [Test]
    public async Task ResolveInstance_UsingFromSubContainer_CanResolveInstanceOnSubContainer()
    {
        var instance = new TestConfig();

        await using var container = await new DiContainerBindings()
            .Install(b =>
            {
                //Bind the config to the container
                b.Bind<TestConfig>().Default().FromInstance(instance);

                //We are testing a sub container 
                b.BindSubContainer<int>(b =>
                {
                    b.Bind<int>().FromInstance(3);
                    
                    //Assert that we can resolve the instance from the parent container
                    var resolved = b.ResolveInstanceNullable<TestConfig>();
                    Assert.That(resolved, Is.SameAs(instance));
                });
            }).Build(CancellationToken.None);
    }
    
    [Test]
    public async Task ResolveInstance_OnASubSubContainer_CanResolveInstance()
    {
        var instance = new TestConfig();

        await using var container = await new DiContainerBindings()
            .Install(b =>
            {
                //Bind the config to the container
                b.Bind<TestConfig>().Default().FromInstance(instance);
            }).Build(CancellationToken.None);
        
        await using var subContainer = await new DiContainerBindings()
            .WithParentContainer(container)
            .Install(b =>
            {
                //We are testing a sub sub container 
                b.BindSubContainer<int>(b =>
                {
                    b.Bind<int>().FromInstance(3);
                    
                    //Assert that we can resolve the instance from the parent container
                    var resolved = b.ResolveInstanceNullable<TestConfig>();
                    Assert.That(resolved, Is.SameAs(instance));
                });
            }).Build(CancellationToken.None);
    }
}
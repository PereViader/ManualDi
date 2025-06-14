using NUnit.Framework;

namespace ManualDi.Sync.Tests;

public class TestContainerBindingConfiguration
{
    public class TestConfig;
    
    [Test]
    public void ResolveInstance_ProperlyResolveAfterInstanceIsAvailable()
    {
        var instance = new TestConfig();
        
        using var container = new DiContainerBindings()
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
            }).Build();
        
        var resolvedInstance = container.Resolve<TestConfig>();
        Assert.That(resolvedInstance, Is.SameAs(instance));
    }
}
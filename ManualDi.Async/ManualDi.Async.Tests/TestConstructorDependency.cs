using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ManualDi.Async.Tests;

public class TestConstructorDependency
{
    private class ClassWithDependency
    {
        public ClassWithDependency(UnregisteredClass dependency) {}
    }

    private class UnregisteredClass {}

    [Test]
    public void TestMissingConstructorDependencyThrowsException()
    {
        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await new DiContainerBindings().Install(b =>
            {
                b.Bind<ClassWithDependency>()
                    .FromMethod(c => new ClassWithDependency(c.Resolve<UnregisteredClass>()))
                    .DependsOn(d => d.ConstructorDependency<UnregisteredClass>());
            }).Build(CancellationToken.None);
        });

        Assert.That(exception!.Message, Does.Contain("is not registered"));
    }

    [Test]
    public void TestMissingConstructorDependencyWithFilterThrowsException()
    {
        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await new DiContainerBindings().Install(b =>
            {
                b.Bind<ClassWithDependency>()
                    .FromMethod(c => new ClassWithDependency(c.Resolve<UnregisteredClass>()))
                    .DependsOn(d => d.ConstructorDependency<UnregisteredClass>(c => true));
            }).Build(CancellationToken.None);
        });

        Assert.That(exception!.Message, Does.Contain("with some filter is not registered"));
    }
}

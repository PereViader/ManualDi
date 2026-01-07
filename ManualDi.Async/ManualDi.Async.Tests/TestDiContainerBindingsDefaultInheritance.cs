using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace ManualDi.Async.Tests;

public class TestDiContainerBindingsDefaultInheritance
{
    [ManualDi]
    public abstract class Base
    {
        public readonly List<string> Order = new();

        public void Inject(object instance)
        {
            Order.Add("Base.Inject");
        }

        public void Initialize()
        {
            Order.Add("Base.Initialize");
        }
    }

    [ManualDi]
    public class Child : Base
    {
        public void Inject(int instance)
        {
            Order.Add("Child.Inject");
        }

        public new void Initialize()
        {
            Order.Add("Child.Initialize");
        }
    }

    [Test]
    public async Task TestDefaultInheritance()
    {
        await using var diContainer = await new DiContainerBindings().Install(b =>
        {
            b.Bind<object>().FromInstance(new object());
            b.Bind<int>().FromInstance(42);
            b.Bind<Child>().Default().FromConstructor();
        }).Build(CancellationToken.None);

        var instance = diContainer.Resolve<Child>();
        Assert.That(instance.Order, Is.EquivalentTo(new[]
        {
            "Base.Inject",
            "Child.Inject",
            "Base.Initialize",
            "Child.Initialize"
        }));
    }
}
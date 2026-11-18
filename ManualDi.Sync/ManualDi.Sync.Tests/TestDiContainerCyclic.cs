using System;
using NUnit.Framework;

namespace ManualDi.Sync.Tests;

public class TestDiContainerCyclic
{
    private sealed class ClassA
    {
        public ClassB? B { get; set; }

        public ClassA() { }

        public ClassA(ClassB b)
        {
            B = b;
        }
    }

    private sealed class ClassB
    {
        public ClassA? A { get; }
        public ClassC? C { get; }

        public ClassB(ClassA a)
        {
            A = a;
        }

        public ClassB(ClassC c)
        {
            C = c;
        }
    }

    private sealed class ClassC
    {
        public ClassA A { get; }

        public ClassC(ClassA a)
        {
            A = a;
        }
    }

    private sealed class SelfDependent
    {
        public SelfDependent(SelfDependent self) { }
    }

    [Test]
    public void TestDirectCycleThrowsInvalidOperationException()
    {
        var bindings = new DiContainerBindings().Install(b =>
        {
            b.Bind<ClassA>().FromMethod(c => new ClassA(c.Resolve<ClassB>()));
            b.Bind<ClassB>().FromMethod(c => new ClassB(c.Resolve<ClassA>()));
        });

        var ex = Assert.Throws<InvalidOperationException>(() => bindings.Build());
        Assert.That(ex!.Message, Is.EqualTo(
            $"Circular dependency detected when resolving type {typeof(ClassA).FullName} from type {typeof(ClassB).FullName}"));
    }

    [Test]
    public void TestTransitiveCycleThrowsInvalidOperationException()
    {
        var bindings = new DiContainerBindings().Install(b =>
        {
            b.Bind<ClassA>().FromMethod(c => new ClassA(c.Resolve<ClassB>()));
            b.Bind<ClassB>().FromMethod(c => new ClassB(c.Resolve<ClassC>()));
            b.Bind<ClassC>().FromMethod(c => new ClassC(c.Resolve<ClassA>()));
        });

        var ex = Assert.Throws<InvalidOperationException>(() => bindings.Build());
        Assert.That(ex!.Message, Is.EqualTo(
            $"Circular dependency detected when resolving type {typeof(ClassA).FullName} from type {typeof(ClassC).FullName}"));
    }

    [Test]
    public void TestSelfDependencyThrowsInvalidOperationException()
    {
        var bindings = new DiContainerBindings().Install(b =>
        {
            b.Bind<SelfDependent>().FromMethod(c => new SelfDependent(c.Resolve<SelfDependent>()));
        });

        var ex = Assert.Throws<InvalidOperationException>(() => bindings.Build());
        Assert.That(ex!.Message, Is.EqualTo(
            $"Circular dependency detected when resolving type {typeof(SelfDependent).FullName} from type {typeof(SelfDependent).FullName}"));
    }

    [Test]
    public void TestCycleBrokenViaInjectDoesNotThrow()
    {
        var container = new DiContainerBindings().Install(b =>
        {
            b.Bind<ClassA>()
                .FromMethod(_ => new ClassA())
                .Inject((o, c) => ((ClassA)o).B = c.Resolve<ClassB>());
            b.Bind<ClassB>()
                .FromMethod(c => new ClassB(c.Resolve<ClassA>()));
        }).Build();

        var a = container.Resolve<ClassA>();
        var b = container.Resolve<ClassB>();

        Assert.That(a, Is.Not.Null);
        Assert.That(b, Is.Not.Null);
        Assert.That(a.B, Is.SameAs(b));
        Assert.That(b.A, Is.SameAs(a));
    }
}

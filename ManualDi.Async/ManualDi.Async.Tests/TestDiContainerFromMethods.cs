using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

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

#pragma warning disable CS9113 // Parameter is unread.
    [ManualDi]
    public class ChildTestInject;
    [ManualDi]
    public class ParentPureTestInject(ChildTestInject childTestInjecta);
    [ManualDi]
    public class ParentMonoBehaviourTestInject
    {
        public void Inject(ChildTestInject childTestInject, [CyclicDependency] ParentPureTestInject parentPureTestInject) { }
    }
#pragma warning restore CS9113 // Parameter is unread.

    [Test]
    public async Task TestSubContainerCreatesInReverseOrder()
    {
        await using var container = await new DiContainerBindings().Install(b =>
        {
            //Added in the reverse order they will need to be created in
            b.BindSubContainer<ParentMonoBehaviourTestInject>(b =>
            {
                b.Bind<ParentMonoBehaviourTestInject>().Default().FromInstance(new());
            });

            b.BindSubContainer<ParentPureTestInject>(b =>
            {
                b.Bind<ParentPureTestInject>().Default().FromConstructor();
            });

            b.Bind<ChildTestInject>().Default().FromConstructor();
        }).Build(CancellationToken.None);
    }

    [ManualDi]
    public class ParentConstructorDependency;
    [ManualDi]
    public class ParentConstructorFilterDependency;
    [ManualDi]
    public class ParentNullableConstructorFilterDependency;
    [ManualDi]
    public class ParentNullableConstructorDependency;

    [ManualDi]
    public class ParentInjectionDependency;
    [ManualDi]
    public class ParentInjectionFilterDependency;
    [ManualDi]
    public class ParentNullableInjectionFilterDependency;
    [ManualDi]
    public class ParentNullableInjectionDependency;

    [ManualDi]
    public class ChildConstructorDependency;
    [ManualDi]
    public class ChildConstructorFilterDependency;
    [ManualDi]
    public class ChildNullableConstructorFilterDependency;
    [ManualDi]
    public class ChildNullableConstructorDependency;


    [ManualDi]
    public class ChildInjectionDependency;
    [ManualDi]
    public class ChildInjectionFilterDependency;
    [ManualDi]
    public class ChildNullableInjectionFilterDependency;
    [ManualDi]
    public class ChildNullableInjectionDependency;

    [ManualDi]
    public class ChildResolveAll
    {
        public ChildResolveAll(
            ParentConstructorDependency parentConstructorDependency,
            [Id("id")] ParentConstructorFilterDependency parentConstructorFilterDependency,
            [Id("id")] ParentNullableConstructorFilterDependency? parentNullableConstructorFilterDependency,
            ParentNullableConstructorDependency? parentNullableConstructorDependency,
            ChildConstructorDependency childConstructorDependency,
            [Id("id")] ChildConstructorFilterDependency childConstructorFilterDependency,
            [Id("id")] ChildNullableConstructorFilterDependency? childNullableConstructorFilterDependency,
            ChildNullableConstructorDependency? childNullableConstructorDependency)
        {
        }

        public void Inject(
            [CyclicDependency] ParentInjectionDependency parentInjectionDependency,
            [CyclicDependency, Id("id")] ParentInjectionFilterDependency parentInjectionFilterDependency,
            [CyclicDependency, Id("id")] ParentNullableInjectionFilterDependency? parentNullableInjectionFilterDependency,
            [CyclicDependency] ParentNullableInjectionDependency? parentNullableInjectionDependency,
            [CyclicDependency] ChildInjectionDependency childInjectionDependency,
            [CyclicDependency, Id("id")] ChildInjectionFilterDependency childInjectionFilterDependency,
            [CyclicDependency, Id("id")] ChildNullableInjectionFilterDependency? childNullableInjectionFilterDependency,
            [CyclicDependency] ChildNullableInjectionDependency? childNullableInjectionDependency
        )
        {
        }
    }

    [Test]
    public void TestFromSubContainerResolveDependencies()
    {
        var dependencies = new DiContainerBindings()
            .Install(InstallSubContainerDependencies)
            .GatherDependencies();

        var dependencyResolver = Substitute.For<IDependencyResolver>();
        dependencies.Invoke(dependencyResolver);

        Received.InOrder(() =>
        {
            // Injection first then constructor because that's how the Default then FromConstructor will add them in

            dependencyResolver.InjectionDependency<ParentInjectionDependency>();
            dependencyResolver.InjectionDependency<ParentInjectionFilterDependency>(Arg.Any<FilterBindingDelegate>());
            dependencyResolver.NullableInjectionDependency<ParentNullableInjectionFilterDependency>(Arg.Any<FilterBindingDelegate>());
            dependencyResolver.NullableInjectionDependency<ParentNullableInjectionDependency>();

            dependencyResolver.ConstructorDependency<ParentConstructorDependency>();
            dependencyResolver.ConstructorDependency<ParentConstructorFilterDependency>(Arg.Any<FilterBindingDelegate>());
            dependencyResolver.NullableConstructorDependency<ParentNullableConstructorFilterDependency>(Arg.Any<FilterBindingDelegate>());
            dependencyResolver.NullableConstructorDependency<ParentNullableConstructorDependency>();
        });
    }


    [Test]
    public async Task TestFromSubContainerResolve()
    {
        await using var container = await new DiContainerBindings().Install(b =>
        {
            InstallSubContainerDependencies(b);

            b.Bind<ParentConstructorDependency>().Default().FromConstructor();
            b.Bind<ParentConstructorFilterDependency>().Default().FromConstructor().WithId("id");
            b.Bind<ParentNullableConstructorFilterDependency>().Default().FromConstructor().WithId("id");
            b.Bind<ParentNullableConstructorDependency>().Default().FromConstructor();
            b.Bind<ParentInjectionDependency>().Default().FromConstructor();
            b.Bind<ParentInjectionFilterDependency>().Default().FromConstructor().WithId("id");
            b.Bind<ParentNullableInjectionFilterDependency>().Default().FromConstructor().WithId("id");
            b.Bind<ParentNullableInjectionDependency>().Default().FromConstructor();
        }).Build(CancellationToken.None);
    }

    private static void InstallSubContainerDependencies(DiContainerBindings b)
    {
        b.BindSubContainer<ChildResolveAll>(b =>
        {
            b.Bind<ChildConstructorDependency>().Default().FromConstructor();
            b.Bind<ChildConstructorFilterDependency>().Default().FromConstructor().WithId("id");
            b.Bind<ChildNullableConstructorFilterDependency>().Default().FromConstructor().WithId("id");
            b.Bind<ChildNullableConstructorDependency>().Default().FromConstructor();
            b.Bind<ChildInjectionDependency>().Default().FromConstructor();
            b.Bind<ChildInjectionFilterDependency>().Default().FromConstructor().WithId("id");
            b.Bind<ChildNullableInjectionFilterDependency>().Default().FromConstructor().WithId("id");
            b.Bind<ChildNullableInjectionDependency>().Default().FromConstructor();

            b.Bind<ChildResolveAll>().Default().FromConstructor(); // This will trigger the resolution of everything else
        });
    }
}
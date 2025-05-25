[![Test and publish](https://github.com/PereViader/ManualDi/actions/workflows/TestAndPublish.yml/badge.svg)](https://github.com/PereViader/ManualDi/actions/workflows/TestAndPublish.yml) [![Unity version 2022.3.29](https://img.shields.io/badge/Unity-2022.3.29-57b9d3.svg?style=flat&logo=unity)](https://github.com/PereViader/ManualDi.Unity3d) [![OpenUPM ManualDi.Sync](https://img.shields.io/npm/v/com.pereviader.manualdi.sync.unity3d?label=openupm%20sync&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.pereviader.manualdi.sync.unity3d/) [![OpenUPM ManualDi.Async](https://img.shields.io/npm/v/com.pereviader.manualdi.async.unity3d?label=openupm%20async&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.pereviader.manualdi.async.unity3d/) [![NuGet ManualDi.Sync](https://img.shields.io/nuget/v/ManualDi.Sync?label=nuget%20sync)](https://www.nuget.org/packages/ManualDi.Sync) [![NuGet ManualDi.Async](https://img.shields.io/nuget/v/ManualDi.Async?label=nuget%20async)](https://www.nuget.org/packages/ManualDi.Async) [![Release](https://img.shields.io/github/release/PereViader/ManualDi.svg?label=github%20release)](https://github.com/PereViader/ManualDi/releases/latest)

Welcome to ManualDi – a fast and extensible C# dependency injection library.
- Unified API to create, inject, initialize and startup the application
- Synchronous and asynchronous library variants with similar APIs.
- Supercharge the container with tailored extensions for your application
- Source generation, no reflection - Faster and more memory efficient than most other dependency injection containers.
- Seamless Unity3D game engine integration


# Benchmark

BenchmarkDotNet [Sync](https://github.com/PereViader/ManualDi/blob/main/ManualDi.Sync/ManualDi.Sync.Benchmark/Benchmark.cs) and [Async](https://github.com/PereViader/ManualDi/blob/main/ManualDi.Async/ManualDi.Async.Benchmark/Benchmark.cs) benchmarks between Microsoft and ManualDi

```
| Method           | Mean      | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|----------------- |----------:|----------:|----------:|-------:|-------:|----------:|
| ManualDi.Sync    | 4.244 us  | 0.0823 us | 0.0948 us | 0.2747 | 0.0076 |  13.73 KB |
| ManualDi.Async   | 6.858 us  | 0.1267 us | 0.1185 us | 0.3128 | 0.0153 |  15.51 KB |
| MicrosoftDi      | 39.258 us | 0.2415 us | 0.2259 us | 2.5024 | 0.6714 | 122.87 KB |
```

Unity3d [Sync](https://github.com/PereViader/ManualDi/blob/main/ManualDi.Sync.Unity3d/Assets/ManualDi.Sync.Unity3d/Tests/Benchmark.cs) and [Async](https://github.com/PereViader/ManualDi/blob/main/ManualDi.Async.Unity3d/Assets/ManualDi.Async.Unity3d/Tests/Benchmark.cs) benchmarks between [Zenject](https://github.com/modesttree/Zenject), [VContainer](https://github.com/hadashiA/VContainer), [Reflex](https://github.com/gustavopsantos/Reflex) and ManualDi

> TODO update this graphic blow. ManualDi.Sync is now a bit faster and is missing ManualDi.Async which is a bit slower than the Sync version

![Unity3d-Container-Benchmark](https://github.com/user-attachments/assets/67065b14-dea1-494a-b53e-469ebaf50101)

# Installation

- Plain C#: Install it using nuget (netstandard2.1) 
  - Sync: https://www.nuget.org/packages/ManualDi.Sync/
  - Async: https://www.nuget.org/packages/ManualDi.Async/

- Unity3d [2022.3.29](https://github.com/PereViader/ManualDi/issues/25) or later 
    - (Recommended) OpenUPM [(instructions)](https://openupm.com/docs/getting-started.html#google_vignette)
        - Sync: https://openupm.com/packages/com.pereviader.manualdi.sync.unity3d/
        - Async: https://openupm.com/packages/com.pereviader.manualdi.async.unity3d/
    - Directly from git [(instructions)](https://docs.unity3d.com/6000.1/Documentation/Manual/upm-ui-giturl.html)
        - Git URL: https://github.com/PereViader/ManualDi.Unity3d.git

Note: \* [.Net Compact Framework](https://es.wikipedia.org/wiki/.NET_Compact_Framework) is [not compatible](https://learn.microsoft.com/en-us/dotnet/api/system.type.typehandle?view=net-8.0#system-type-typehandle) because of an [optimization](https://github.com/PereViader/ManualDi/commit/d7965d1b77b905084bb1fdf8fdad7c4f53f63fb5)

Note: Source generation will only happen in csproj that are linked both with the source generator and the library.
- In a regular C# project, this requires referencing the library on the csproj as a nuget package
- In a Unity3d project, this requires adding the library to the project through the Package Manager and then referencing ManualDi on each assembly definition where you want to use it

Note: Source generator will never run on 3rd party libraries and System classes because they won't reference the generator.

Note: A limitation of the source generator is that it does not run for partial classes defined across multiple declarations. It will only operate on partial classes that are declared once.

# Container Lifecycle

- Binding Phase: Container binding configuration is defined 
- Building Phase: Binding configurastion is used to create the object graph
- Startup Phase: Startup callbacks are run.
- Alive Phase: Container is returned to the user and it can be kept until it is no longer necessary. 
- Disposal Phase: The container and its resources are released.

In the section below we will add two examples, check the `ApplicationEntryPoint` to see an example of the lifecycle

- Creating the builder and installing the bindings is where the binding phase happens
- Within the execution of the `Build` method both the Building and Startup phase will happen
    - The container and object graph is be created
    - Startup callbacks of the application are invoked

## ManualDi.Sync

```csharp
public void ApplicationEntryPoint
{
    private DiContainer _diContainer;
 
    public void StartApplication()
    {
        _diContainer = new DiContainerBindings() 
            .Install(b => {
                b.Bind<SomeClass>().Default().FromConstructor();
                b.Bind<IOtherClass, OtherClass>().Default().FromConstructor();
                b.Bind<Startup>().Default().FromConstructor();
                b.WithStartup<Startup>(static startup => startup.Execute());
            })
            .Build();
    }

    public void StopApplication()
    {
        if(_diContainer == null) return;

        _diContainer.Dispose();
        _diContainer = null;
    }
}
    
//Once it reaches this line, both classes will have been created, initialized in the proper order and Startup.Execute will have been run

public class SomeClass
{
    public SomeClass(IOtherClass otherClass)
    {
        // All required classes are instantiated in an initial pass.
        // The order of instantiation and subsequent method execution is determined dynamically at runtime
        // based on dependency analysis, ensuring automatic adaptation to changes.
    }

    public void Inject(IOtherClass otherClass)
    {
        //Called after Construct
        //Should only be used when to break cyclic dependencies between dependencies
        //When you need to use it, it usually means there is a problem in the design of your object graph
        //Dependencies taken from Inject will always sort those dependencies at the end.  
    }

    public void Initialize() { 
        // Called after Inject
        // Should be used to implement actual initialization logic.
        // Other methods should have NO logic, only store references and wire the object graph
        // By having logic ONLY on Intialize methods, the library guarantees that there won't be issues in initialization
        // All methods other than the constructor are called in order of dependencies
        // In this example SomeClass.Initialize runs after OtherClass.Initialize (because SomeClass depends on IOtherClass)
    }
}

public interface IOtherClass { }

public class OtherClass : IOtherClass
{
    public void Initialize() { 
        // Some initialization here
    }
}

public class Startup(SomeClass someClass, IOtherClass otherClass) // Constructor injected
{
    public void Execute()
    {
        //This runs after SomeClass.Initialize and OtherClass.Initialize
    }
}
```

## ManualDi.Async

```csharp
public void ApplicationEntryPoint
{
    private DiContainer _diContainer;
 
    public void StartApplication()
    {
        await using DiContainer diContainer = await new DiContainerBindings()
            .Install(b => {
                b.Bind<SomeClass>().Default().FromConstructor();
                b.Bind<IOtherClass, OtherClass>().Default().FromConstructor();
                b.Bind<Startup>().Default().FromConstructor();
                b.WithStartup<Startup>(static async (startup, ct) => startup.Execute(ct));
            })
            .Build(CancellationToken.None);
    }

    public ValueTask StopApplication()
    {
        if(_diContainer == null) return;

        var task = _diContainer.DisposeAsync();
        _diContainer = null;
        return task;
    }
}

    
//Once it reaches this line, both classes will have been created, initialized in the proper order and Startup.Execute will have been run

public class SomeClass
{
    public SomeClass(IOtherClass otherClass)
    {
        // All required classes are instantiated in an initial pass.
        // The order of instantiation and subsequent method execution is determined dynamically at runtime
        // based on dependency analysis, ensuring automatic adaptation to changes.
    }

    public void Inject(IOtherClass otherClass)
    {
        //Called after Construct
        //Should only be used when to break cyclic dependencies between dependencies
        //When you need to use it, it usually means there is a problem in the design of your object graph
        //Dependencies taken from Inject will always sort those dependencies at the end.  
    }

    public void Initialize() { 
        // Called after Inject
        // Should be used to implement actual initialization logic.
        // Other methods should have NO logic, only store references and wire the object graph
        // By having logic ONLY on Intialize methods, the library guarantees that there won't be issues in initialization
        // All methods other than the constructor are called in order of dependencies
        // In this example SomeClass.Initialize runs after OtherClass.InitializeAsync (because SomeClass depends on IOtherClass)
    }
}

public interface IOtherClass { }

public class OtherClass : IOtherClass
{
    public void InitializeAsync() { 
        // Some initialization here
    }
}

public class Startup(SomeClass someClass, IOtherClass otherClass) // Constructor injected
{
    public async Task Execute(CancellationToken ct)
    {
        //This runs after SomeClass.Initialize and OtherClass.InitializeAsync
    }
}
```

# Quick concepts

Let's briefly discuss some concepts from the library

- The container is created using a builder `DiContainerBindings`
- Container instance creation are configured through `Bind` method overloads.
- The type exposed will depend on the `Bind<TConcrete>()`/`Bind<TApparent, TConcrete>()` method used 
- Configuration of the binding is performed on the `Binding` object returned by the `Bind` method.
- The library achieves speed via source generated, reflection-free, `Default` and `FromConstructor` methods
- Using the source-generated methods is optional but the container's recommended pattern
- The `FromConstructor` method configures the Binding to use `TConcrete`'s constructor, resolving parameters from the container
- There are many other construction methods other than `FromConstructor` available.
- Instances are created in inverse dependency order.
- Method injection is accomplished by adding an `Inject` method to your instances.
- Instance initialization is accomplished by adding an `Initialize` or `InitializeAsync`** method.
- The `Default` method configures the `Binding` to use `TConcrete`'s `Inject`, `Initialize` and `InitializeAsync`** methods
- Instances are injected and initialized in the same order they are created
- Once the object graph is created and initialized, `WithStartup` callbacks are invoked

# Binding

The container configuration is done through a fluent API available on `DiContainerBindings`.
This process can only be done during the initial phase and should not be altered afterwards. 

The fluent binding API begins by calling `Binding<TApparent, TConcrete>`.
- TApparent: It's the type that can be used when resolving the container.
- TConcrete: It's type of the actual instance behind the scenes.

Let's see an example
```csharp
interface IInterface { }
class Implementation : IInterface {}

b.Bind<IInterface, Implementation>()...
...
c.Resolve<IInterface>() // Succeeds
c.Resolve<Implementation>() // Runtime error
```

Use the returned value of the bind method, to configure the binding and tell the container how it should create, inject, initialize and dispose the instance.
The specific binding configuration is done through 9 categories of methods that, by convention, should be done in the order specified below.

```csharp
// * means source generated
// ** means ManualDi.Async only

// TApparent is optional and will be equal to TConcrete when undefined
Bind<(TApparent,)? TConcrete>() 
    .Default*
    .From[Constructor*|Instance|Method|MethodAsync**|ContainerResolve|SubContainerResolve|...]  //There are many more
    .DependsOn**
    .Inject
    .Initialize
    .Dispose
    .WithId
    .When([InjectedIntoId|InjectedIntoType])
    .[Any other custom extension method your project implements]
```

Keep in mind that you can bind multiple implementations to the same `TApparent` type.
When resolving, if multiple bindings match, the first one that satisfies the resolution rules will be returned.

```csharp
b.Bind<SomeClass>().Default().FromConstructor(); // When calling c.Resolve<SomeClass>() this one is returned
b.Bind<SomeClass>().Default().FromConstructor();
```


## Default

This source generated method is where most of the "magic" of the library happens.
The source generator will inspect the `TConcrete` type and generate code to register the `Inject`, `Initialize` and `InitializeAsync`** methods when available.
It will also inspect the type for `IDisposable` or `IAsyncDisposable`** and disable the automatic disposal behaviour when it does not implement it.

Think of this system as "duck typing" via source generation: if a method matches the expected name and signature, it will be invoked, regardless of interface inheritance.

For detailed behavior of each of the registered methods, refer to the sections below.

When there are multiple candidates for a given method:
- public methods are preferred over internal ones.
- in case multiple methods with the same visibility exist, the first one found top-to-bottom is selected.

```csharp
public class A { }
public class B {
    public void Inject(A a) { } //Sample only, prefer using the constructor
}
public class C {
    public void Initialize() { }
}
public class D {
    public D(A a) { }
    public void Inject(C c, A a) { } //Sample only, prefer using the constructor
    public Task InitializeAsync(CancellationToken ct) { ... }
}

b.Bind<A>().Default().FromConstructor(); // Default does not call anything
b.Bind<B>().Default().FromConstructor(); // Default calls Inject
b.Bind<C>().Default().FromConstructor(); // Default calls Initialize
b.Bind<D>().Default().FromConstructor(); // Default calls Inject and InitializeAsync
```

Using `Default` is optional, but it is the recommended pattern in this library as it accelerates development.
By using it, developers only need to implement standardized DI boilerplate once.
Any subsequent changes to the types are automatically handled by the source generator.
For this reason, it’s best to always include it—even if the type doesn’t initially define any of the methods.

## From

The `From` methods define the instantiation strategy the container should use for each binding.

### Constructor

This method is source generated ONLY when there is a single `public`/`internal` accessible constructor.

The container creates the instance using the constructor of the `TConcrete` type. 
Dependencies necessary on the constructor will get resolved from the container.

```csharp
b.Bind<T>().Default().FromConstructor();
```

### Instance

The container does not create the instance—it is provided externally and used as-is.

```csharp
b.Bind<T>().Default().FromInstance(new T())
```

### Method

The instance is created using the provided delegate. The container is passed in as a parameter, allowing it to resolve any required dependencies.
Note: All synchronous `From` configuration methods end up calling this one

```csharp
b.Bind<T>().Default().FromMethod(c => new T(c.Resolve<SomeService>()))
```

### MethodAsync 
(**ManualDi.Async only)

The instance is created using the provided async delegate. The container and a cancellation token is passed in as a parameter, allowing it to resolve any required dependencies and handle cancellation.
Note: All asynchronous `From` configuration methods end up calling this one

```csharp
b.Bind<T>().Default().FromMethodAsync(async (c, ct) => {
    // Work can be delayed for any reason
    // For example: HttpRequests, Loading from disk, etc
    await Task.Delay(300, ct); 
    return new T(c.Resolve<SomeService>())
    });
```

### ContainerResolve

You will rarely need to call this one.
Used for apparent type remapping. Or in other words, used to reexpose some binding as another binding.
Don't call `Default` when using this, otherwise you get multiple calls on `Inject`, `Initialize` and `InitializeAsync`** more than once

In the example below, there is `SomeClass` that is bound individually and then two more bindings expose the `SomeClass`
```csharp
interface IFirst { }
interface ISecond { }
public class SomeClass : IFirst, ISecond { }
b.Bind<SomeClass>().Default().FromConstructor();
b.Bind<IFirst, SomeClass>().FromContainerResolve();
b.Bind<ISecond, SomeClass>().FromContainerResolve();
```

However the recommended pattern when implementing this is to use the `Bind` overload with multiple `TApparent`

```csharp
b.Bind<IFirst, ISecond, SomeClass>().Default().FromConstructor();
```

### SubContainerResolve

The instance is created using a sub-container built via the provided installer.
This is useful for encapsulating parts of the object graph into isolated sub-containers.
The sub-container inherits from the main container, allowing its bindings to depend on types registered in the parent.
When using this approach, do not call Default on the main binding—Default should be invoked within the sub-container’s installation instead.

Question: When would I do this?
Answer: For instance, think of a Unity3d game that has many enemies on a scene and you want to bind all enemies to the container so that their dependencies are setup and it is properly initialized.

```csharp
class Enemy : MonoBehaviour
{ 
    public void Inject(ParentDependency parentDependency, SubDependency subDependency) { }
    public void Initialize() { }
}
class ParentDependency { }
class SubDependency {}

b.Bind<ParentDependency>().Default().FromConstructor();
foreach(var enemy in enemiesInScene) //enemiesInScene 
{
    b.Bind<Enemy>().FromSubContainerResolve(sub => {
        sub.Bind<Enemy>().Default().FromInstance(enemy);
        sub.Bind<SubDependency>().Default().FromConstructor();
    });
}
```

### IsolatedSubContainerResolve

Works just like `SubContainerResolve` but the subcontainer will not inherit from the main container.
Thus nothing from the main container will be resolvable. 

```csharp
class Enemy : MonoBehaviour
{ 
    public void Inject(SubDependency subDependency) { }
}
class SubDependency {}

foreach(var enemy in enemiesInScene)
{
    b.Bind<Enemy>().FromSubContainerResolve(sub => {
        sub.Bind<Enemy>().Default().FromInstance(enemy);
        sub.Bind<SubDependency>().Default().FromConstructor();
    });
}
```


## Inject

The Inject method allows for post-construction injection of types.
The injection will be run only once.
The injection will be done in reverse dependency order. Injected objects will already be injected themselves.
The injection can be used to hook into the object creation lifecycle and run custom code.
Any amount of injection callbacks can be registered

As stated on the `Default` section, calling the source generated method will handle the registration of the`Inject` method on `TConcrete` automatically.

The inject method has two usecases
1. Inject dependencies when the constructor can't be used (this happens for instance in unity3d)
2. Workaround Cyclic dependencies that prevent the object graph from being wired
Warning: Cyclic dependencies usually highlight a problem in the design of the code. If you find such a problem in your codebase, consider redesigning the code before applying the following proposal.

This next example will fail

```csharp
public class A(B b);
public class B(A a);
```

In order to fix this, the chain must be broken with `Inject`.
When using `ManualDi.Async` adding the `CyclicDependency`** attribute is also necessary in order to break down cyclic dependencies. Without the attribute it might work, but just due to chance. Using it, will update the creation order of dependencies to avoid issues.

```csharp
public class A(B b);
public class B
{
    public void Inject(A a) { } //ManualDi.Sync this will work
    public void Inject([CyclicDependency] A a) { } //ManualDi.Async
}
```

Note: When dealing with cyclic dependencies, the usual method execution order may not hold.
Normally, Initialize is called on a type’s dependencies before being called on the type itself.
However, with cyclic dependencies, this order is not guaranteed, and additional synchronization logic may be required to ensure correct behavior.
That said, cyclic dependencies are often a sign of flawed design. If you encounter them, it’s usually better to refactor the architecture rather than patch around the issue.

## Initialize

The Initialize method allows for post-injection initialization of types.
The initialization will NOT happen immediately after the object injection. It will be queued and run later.
The initialization will be done in reverse resolution order. In other words, dependent objects will already be initialized themselves.
The initialization will not happen more than once for any instance.
The initialization can also be used to hook into the object creation lifecycle and run other custom user code.
Any amount of initialization callback can be registered

As stated on the `Default` section, calling the source generated method will handle the Initialize method automatically.


```csharp
b.Bind<object>()
    .FromInstance(new object())
    .Initialize((o, c) => Console.WriteLine("1"))  // When object is resolved this is called first
    .Initialize((o, c) => Console.WriteLine("2")); // And then this is called
```

```csharp
public class A
{
    public void Initialize() { }
}

public class B
{
    public B(A a) { }
    
    public void Initialize() { }
}

//This is the manual implementation without Default
b.Bind<A>().FromConstructor().Initialize((o, c) => o.Initialize()));
b.Bind<B>().FromConstructor().Initialize((o, c) => o.Initialize());

//And this is the equivalent and simpler implementation with Default
b.Bind<A>().Default().FromConstructor();
b.Bind<B>().Default().FromConstructor();
```

Creating custom extension methods that call Initialize is the recommended way to supercharge the container.

### Example: Interconnect some features

```csharp
class SomeFeature : IFeature { }

b.Bind<SomeFeature>()
    .Default()
    .FromConstructor()
    .LinkFeature();
```

Imagine you have an `IFeature` interface in your project and you want to add some shared initialization code to the ones that have it. You can add this code in an "Link" extension method. Internally this extension method should just call the `Initialize` method and add whatever extra logic the feature requires.

You may find further Link examples already present in the library [here](https://github.com/PereViader/ManualDi/blob/540cb3d3155d81dc8925d9ab5769d2a18e61e81b/ManualDi.Unity3d/Assets/ManualDi.Unity3d/Runtime/Extensions/TypeBindingLinkExtensions.cs#L8)

## Dispose

Objects may implement the IDisposable interface or require custom teardown logic. 
The Dispose extension method allows defining behavior that will run when the object is disposed. 
The container will dispose of the objects when itself is disposed.
The objects will be disposed in reverse resolution order.

If an object implements the `IDisposable` or `IAsyncDisposable`** interface, it doesn't need a manual `Dispose` call in the binding; it will be disposed of automatically.

Using the `Default` source-generated method provides a slight optimization by skipping a runtime check for IDisposable and IAsyncDisposable.

```csharp
class A : IDisposable 
{ 
    public void Dispose() { }    
}

class B 
{
    public B(A a) { }
    public void DoCleanup() { }
}

b.Bind<A>().Default().FromConstructor(); // No need to call Dispose because the object is IDisposable
b.Bind<B>().Default().FromConstructor().Dispose((o,c) => o.DoCleanup());

// ...

B b = c.Resolve<B>();

c.Dispose(); // A is the first object disposed, then B
```

### SkipDisposable

When this extension method is used, the container skips the runtime check for `IDisposable` and `IAsyncDisposable`** during the disposal phase, and consequently, does not dispose the instances.

Delegates registered using the `Dispose` method will still be invoke.


## WithId

These extension methods allow defining an id, enabling the filtering of elements during resolution.

```csharp
b.Bind<int>().FromInstance(1).WithId("Potato");
b.Bind<int>().FromInstance(5).WithId("Banana");

// ...

c.Resolve<int>(x => x.Id("Potato")); // returns 1
c.Resolve<int>(x => x.Id("Banana")); // returns 5
```

Note: This feature can be nice to use, but I usually prefer using it sparingly. This is because it introduces the need for the provider and consumer to share two points of information (Type and Id) instead of just one (Type)

An alternative to it is to register delegates instead. Delegates used this way encode the two concepts into one.

```
delegate int GetPotatoInt();

b.Bind<GetPotatoInt>().FromInstance(() => 1);

int value = c.Resolve<GetPotatoInt>()();
```

### Source generator

The id functionality can be used on method and property dependencies by using the Inject attribute and providing a string id to it

```csharp
class A
{
    public void Inject(int a, [Id("Other")] object b) { ... }
}

//Will resolve the object with the requested Id
o.Inject(
    c.Resolve<int>(),
    c.Resolve<object>(x => x.Id("Other"))
)
```

## When

The `When` extension method allows defining filtering conditions as part of the bindings.

### InjectedIntoType

Allows filtering bindings using the injected Concrete type

```csharp
class SomeValue(int Value) { }
class OtherValue(int Value) { }
class FailValue(int Value) { }

b.Bind<int>().FromInstance(1).When(x => x.InjectedIntoType<SomeValue>())
b.Bind<int>().FromInstance(2).When(x => x.InjectedIntoType<OtherValue>())

b.Bind<SomeValue>().Default().FromConstructor(); // will be provided 1
b.Bind<OtherValue>().Default().FromConstructor(); // will be provided 2
b.Bind<FailValue>().Default().FromConstructor(); // will fail at runtime when resolved
```

### InjectedIntoId

Allows filtering bindings using the injected Concrete type

```csharp
class SomeValue(int Value) { }

b.Bind<int>().FromInstance(1).When(x => x.InjectedIntoId("1"));
b.Bind<int>().FromInstance(2).When(x => x.InjectedIntoId("2"));

b.Bind<SomeValue>().Default().FromConstructor().WithId("1"); // will be provided 1
b.Bind<SomeValue>().Default().FromConstructor().WithId("2"); // will be provided 2
b.Bind<FailValue>().Default().FromConstructor(); // will fail at runtime when resolved
```

# Installers

In order to group features in a sensible way, bindings will usually be grouped on Installer classes.
These classes may be implemented either as object instances that implement `IInstaller` or as extension methods.
Unless you explicitly need to use actual instances, this library recommends to prefer extension methods.

```csharp
class A { }
class B { }
interface IC { }
class C : IC { }

//Extension method (recommended)
static class SomeFeatureInstaller
{
    public static DiContainerBindings InstallSomeFunctionality(this DiContainerBindings b)
    {
        b.Bind<A>().Default().FromInstance(new A());
        b.Bind<B>().Default().FromConstructor();
        b.Bind<IC, C>().Default().FromConstructor();
        return b;
    }
}

//Or

class SomeFeatureInstaller : IInstaller
{
    public static DiContainerBindings Install(DiContainerBindings b)
    {
        b.Bind<A>().Default().FromInstance(new A());
        b.Bind<B>().Default().FromConstructor();
        b.Bind<IC, C>().Default().FromConstructor();
        return b;
    }
}
```

# Extra Source Generator features

## Nullable

The source generator will take into account the nullability of the dependencies.
If a dependency is nullable, the resolution will not fail if it is missing.
If a dependency is not nullable, the resolution will fail if it is missing.

```csharp
public class A
{
    //object MUST be registered on the container
    //int may or may not be registered on the container
    public A(object obj, int? nullableValue) { }
}

b.Bind<A>().Default().FromConstructor();
```

## Collection

The source generator will inject all bound instances of a type if the dependency declared is one of the following types `List<T>` `IList<T>` `IReadOnlyList<T>` `IEnumerable<T>`
The resolved dependencies will be resolved using `ResolveAll<T>`

If the collection itself is declared nullable (e.g., `List<T>?`), it will be `null` if no matching bindings are found. Otherwise (e.g. `List<T>`), an empty list will be injected if no matching bindings are found. In both cases, when no matching bindings exist, the list will contain those instances found.
If the generic type argument `T` is nullable (e.g., `List<T?>`), the source generator will accommodate this. This is generally not recommended, as bindings are always expected to return non-null instances.



```csharp
public class A
{
    public 
    [Inject] List<object> ListObj {get; set;}
    [Inject] IList<int> IListInt {get; set;}
    [Inject] IReadOnlyList<obj> IReadOnlyListObj {get; set;}
    [Inject] IEnumerable<int> IEnumerableInt {get; set;}
    [Inject] List<object>? NullableList {get; set;} //Either null or Count > 0
    [Inject] List<object?> NullableGenericList {get; set;} //Valid but NOT recommended
}

b.Bind<A>().Default().FromConstructor();
```

# Resolving

Notice that resolution can only be done on apparent types, not concrete types. Concrete types are there so the container can provide a type safe fluent API.

If you use the source generated methods, you will usually not interact with the Resolution methods.

Resolutions can be done in the following ways:

## Resolve

Resolve an instance from the container. An exception is thrown if it can't be resolved.

```csharp
SomeService service = container.Resolve<SomeService>();
```

## ResolveNullable

Resolve a reference type instance from the container. Returns null if it can't be resolved.

```csharp
SomeService? service = container.ResolveNullable<SomeService>();
```

## ResolveNullableValue

Resolve a value type instance from the container. Returns null if it can't be resolved.

```csharp
int? service = container.ResolveNullableValue<int>();
```

## TryResolve

Resolve an instance from the container. Returns true if found and false if not.

```csharp
bool found = container.TryResolve<SomeService>(out SomeService someService);
```

## ResolveAll

Resolve all the registered instance from the container. If no instances are available the list is empty.

```csharp
List<SomeService> services = container.ResolveAll<SomeService>();
```

# Startups

The container provides functionality that queues work to be done once the container is built and ready.
By using this you can define the entry points of your application declaratively during the installation of the container.

```csharp
class Startup
{
    public Startup(...) { ... }
    public void Start() { ... }
}

class SomeService
{
    public void Initialize() { ... }
}

b.Bind<SomeService>().Default().FromConstructor();
b.Bind<Startup>().Default().FromConstructor();
b.WithStartup<Startup>(o => o.Start());
```

In the snippet above, the following will happen when the container is built:
- `SomeService` is created
- `Startup` is created
- `SomeService`'s `Initialize` method is called
- `Startup`'s `Start` method is called

# Unity3d

When using the container in unity, do not rely on Awake / Start. Instead, rely on Inject / Initialize / InitializeAsync.
You may still use Awake / Start if the classes involved are not injected through the container.

By relying on the container and not on native Unity3d callbacks you can be certain that the dependencies of your classes are Injected and Initialized in the proper order.

## Installers

The container provides two specialized Installers 
- `MonoBehaviourInstaller` 
- `ScriptableObjectInstaller`

This is the idiomatic Unity way to have both the configuration and engine object references in the same place.
These classes just implement the `IInstaller` interface, there is no requirement for these classes to be used, so feel free to use IInstaller directly if you want.

```csharp
public class SomeFeatureInstaller : MonoBehaviourInstaller
{
    public Image Image;
    public Toggle Toggle;
    public Transform Transform;

    public override void Install(DiContainerBindings b)
    {
        b.Bind<Image>().FromInstance(Image);
        b.Bind<Toggle>().FromInstance(Toggle);
        b.Bind<Transform>().FromInstance(Transform);
    }
}
```

## Binding

When using the container in the Unity3d game engine the library provides specialized extensions for object construction

- `FromGameObjectGetComponentInParent`: Retrieves a component from the parent of a GameObject.
- `FromGameObjectGetComponentsInParent`: Retrieves all components of a specific type from the parent of a GameObject.
- `FromGameObjectGetComponentInChildren`: Retrieves a component from the children of a GameObject.
- `FromGameObjectGetComponentsInChildren`: Retrieves all components of a specific type from the children of a GameObject.
- `FromGameObjectGetComponent`: Retrieves a component from the current GameObject.
- `FromGameObjectAddComponent`: Adds a component to the current GameObject.
- `FromGameObjectGetComponents`: Retrieves all components of a specific type from the current GameObject.
- `FromInstantiateComponent`: Instantiates a component and optionally sets a parent.
- `FromInstantiateGameObjectGetComponent`: Instantiates a GameObject and retrieves a specific component from it.
- `FromInstantiateGameObjectGetComponentInChildren`: Instantiates a GameObject and retrieves a component from its children.
- `FromInstantiateGameObjectGetComponents`: Instantiates a GameObject and retrieves all components of a specific type.
- `FromInstantiateGameObjectGetComponentsInChildren`: Instantiates a GameObject and retrieves all components from its children.
- `FromInstantiateGameObjectAddComponent`: Instantiates a GameObject and adds a component to it.
- `FromObjectResource`: Loads an object from a Unity resource file by its path.
- `FromInstantiateGameObjectResourceGetComponent`: Instantiates a GameObject from a resource file and retrieves a component from it.
- `FromInstantiateGameObjectResourceGetComponentInChildren`: Instantiates a GameObject from a resource and retrieves a component from its children.
- `FromInstantiateGameObjectResourceGetComponents`: Instantiates a GameObject from a resource and retrieves all components of a specific type.
- `FromInstantiateGameObjectResourceGetComponentsInChildren`: Instantiates a GameObject from a resource and retrieves all components from its children.
- `FromInstantiateGameObjectResourceAddComponent`: Instantiates a GameObject from a resource and adds a component to it.

Use them like this.

```csharp
public class SomeFeatureInstaller : MonoBehaviourInstaller
{
    public Transform canvasTransform;
    public string ResourcePath;
    public Toggle TogglePrefab;
    public GameObject SomeGameObject;

    public override Install(DiContainerBindings b)
    {
        b.Bind<Toggle>().FromInstantiateComponent(TogglePrefab, canvasTransform);
        b.Bind<Image>().FromInstantiateGameObjectResourceGetComponent(ResourcePath);
        b.Bind<SomeFeature>().FromGameObjectGetComponent(SomeGameObject);
    }
}
```

As seen in the snippet above, my recommendation is that whatever dependencies may be necessary on an installer are provided as public members.
This way Unity3d will serialize them so they can be linked through the inspector. Private `[SerializeField]` members may be used, but they add extra unnecesary boilerplate compared to public fields. 

Most methods have several optional parameters.

Special attention to `Transform? parent = null`. This one will define the parent transform used when instantiating new instances.

Special attention to the `bool destroyOnDispose = true` parameter. This one will be available on creation strategies that create new instances.
If the parameter is left as `true`, when the container is disposed, it will first destroy the instance.
This is the necessary default behaviour due to the game likely needing those resources cleaned up for example from shared Additive scenes and wanting the default behaviour to be the safest.
If the scene the resource is created on will then be deleted, there is no need to destroy it during the disposal of the container, so feel free to set the parameter as `false` for a faster disposal.

## EntryPoints

An entry point is a place where some context of your application is meant to start.
In the case of ManualDi, it is where the object graph is configured and then the container is started.

The last binding of an entry point will usually make use of WithStartup, to run any logic necessary after the container is created.

### RootEntryPoint

Root entry points will not depend on any other container.
Root entry points may be started either manually or on the Unity Start callback. This is configured through the inspector.

Use the appropriate type depending on how you want to structure your application:
- `MonoBehaviourRootEntryPoint`
- `ScriptableObjectRootEntryPoint`

```csharp
public class Startup
{
    public Startup(Dependency1 d1, Dependency2 d2) { ... }
    public void Start() { ... }
}

class InitialSceneEntryPoint : MonoBehaviourRootEntryPoint
{
    public Dependency1 dependency1;
    public Dependency2 dependency2;

    public override void Install(DiContainerBindings b)
    {
        b.Bind<Dependency1>().Default().FromInstance(dependency1);
        b.Bind<Dependency2>().Default().FromInstance(dependency2);
        b.Bind<Startup>().Default().FromConstructor();
        b.WithStartup<Startup>(o => o.Start());
    }
}
```

### SubordinateEntryPoint

Subordinate entry points will depend on other container or require other data.
This means that these entry points cannot be started by themselves. They need to be started by some other part of your application.

If the data provided to these entry points, implements `IInstaller`, then the data will also be installed to the container.
Otherwise, it will just be available through the `Data` property of the EntryPoint.
If the subordinate entry point requires access to the parent container's dependencies, it is recommended to set the parent container on the EntryPointData object.

```csharp
public class EntryPointData : IInstaller
{
    public IDiContainer ParentDiContainer { get; set; }

    public void Install(DiContainerBindings b)
    {
        b.WithParentContainer(ParentDiContainer);
    }
}
```

These entry points may also optionally return a `TContext` object resolved from the container.

Doing such a thing is useful to provide a facade to the systems created.

Use the appropriate type depending on how you want to structure your application:
- `MonoBehaviourSubordinateEntryPoint<TData>`
- `MonoBehaviourSubordinateEntryPoint<TData, TContext>`
- `ScriptableObjectSubordinateEntryPoint<TData>`
- `ScriptableObjectSubordinateEntryPoint<TData, TContext>`

```csharp
public class Startup
{
    public Startup(Dependency1? d1, Dependency2 d2) { ... }
    public void Start() { ... }
}

public class EntryPointData : IInstaller
{
    public Dependency1? Dependency1 { get; set; }

    public void Install(DiContainerBindings b)
    {
        if(Dependency1 is not null)
        {
            b.Bind<Dependency1>().FromInstance(Dependency1).DontDispose();
        }
    }
}

public class Facade : MonoBehaviour
{
    [Inject] public Dependency1? Dependency1 { get; set; }
    [Inject] public Dependency2 Dependency2 { get; set; }

    public void DoSomething1()
    {
        Dependency1?.DoSomething1();
    }

    public void DoSomething2()
    {
        Dependency2.DoSomething2();
    }
}

class InitialSceneEntryPoint : MonoBehaviourSubordinateEntryPoint<EntryPointData, Facade>
{
    public Dependency2 dependency2;
    public Facade Facade;

    public override void Install(DiContainerBindings b)
    {
        b.Bind<Dependency2>().Default().FromInstance(dependency2);
        b.Bind<Facade>().Default().FromInstance(Facade);
        b.Bind<Startup>().Default().FromConstructor();
        b.WithStartup<Startup>(o => o.Start());
    }
}
```

And this is an example of how a subordinate entry point on a scene or as a prefab could be initiated

```csharp
public class Data
{
    public string Name { get; set; }
}

public class SceneFacade
{
    [Inject] Data Data { get; set; }

    public void DoSomething() 
    {  
        Console.WriteLine(Data.Name);
    }
}

public class SceneEntryPoint : MonoBehaviourSubordinateEntryPoint<Data, SceneFacade>
{
    public override void Install(DiContainerBindings b)
    {
        b.Bind<Data>().Default().FromInstance(Data);
        b.Bind<SceneFacade>().Default().FromConstructor();
    }
}

class Example
{
    IEnumerator Run()
    {
        yield return SceneManager.LoadSceneAsync("TheScene", LoadSceneMode.Additive);
        
        var entryPoint = Object.FindObjectOfType<SceneEntryPoint>();
        var data = new Data() { Name = "Charles" };
        var facade = entryPoint.Initiate(data)
        
        facade.DoSomething();
    }
}
```

and this is an example of how you could initiate a subordinate entry point that is part of a prefab


```csharp
class Example : MonoBehaviour
{
    public SceneEntryPoint EntryPoint;

    void Start()
    {        
        var data = new Data() { Name = "Charles" };
        var facade = entryPoint.Initiate(data)
        
        facade.DoSomething();
    }
}
```

The container provides you with the puzzle pieces necessary. The actual composition of these pieces is up to you to decide.
Feel free to ignore the container classes and implement your custom entry points if you have any special need.

## Link

Link methods are a great way to interconnect different features right from the container.
The library provides a few, but adding your own custom ones for your use cases is a great way to speed up development.

- `LinkDontDestroyOnLoad`: The GameObject associated with the bound component will have `DontDestroyOnLoad` called on it when the container is bound. Behaviour can be customized with the optional parameters

```csharp
class Installer : MonoBehaviourInstaller
{
    public SomeService SomeService;

    public override void Install(DiContainerBindings b)
    {
        b.Bind<SomeService>()
            .Default()
            .FromInstance(SomeService)
            .LinkDontDestroyOnLoad();
    }
}
```

Note: There is a sample in the package that provides a Tickable system and a LinkTickable extension. This system allows for having Update like behaviour on any class.
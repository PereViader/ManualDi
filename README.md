[![Test and publish](https://github.com/PereViader/ManualDi/actions/workflows/TestAndPublish.yml/badge.svg)](https://github.com/PereViader/ManualDi/actions/workflows/TestAndPublish.yml) [![NuGet Version](https://img.shields.io/nuget/v/ManualDi.Main)](https://www.nuget.org/packages/ManualDi.Main) [![Release](https://img.shields.io/github/release/PereViader/ManualDi.svg)](https://github.com/PereViader/ManualDi/releases/latest) [![Unity version 2022.3.29](https://img.shields.io/badge/Unity-2022.3.29-57b9d3.svg?style=flat&logo=unity)](https://github.com/PereViader/ManualDi.Unity3d) [![openupm](https://img.shields.io/npm/v/com.pereviader.manualdi.unity3d?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.pereviader.manualdi.unity3d/)

Welcome to ManualDi – a fast and extensible C# dependency injection library.
- Source generation, no reflection - Faster and more memory efficient than most other dependency injection containers.
- Unified API to create, inject, initialize and startup the application
- Compatible with almost all client and server platforms, including IL2CPP and WASM*
- Supercharge the container with your own custom extensions
- Seamless Unity3D game engine integration
- Synchronous and asynchronous library variants with similar APIs.

\* [.Net Compact Framework](https://es.wikipedia.org/wiki/.NET_Compact_Framework) is [not compatible](https://learn.microsoft.com/en-us/dotnet/api/system.type.typehandle?view=net-8.0#system-type-typehandle) because of an [optimization](https://github.com/PereViader/ManualDi/commit/d7965d1b77b905084bb1fdf8fdad7c4f53f63fb5)


# Benchmark
[Benchmark](https://github.com/PereViader/ManualDi/blob/main/ManualDi.Main/ManualDi.Main.Benchmark/Benchmark.cs) against Microsoft's container
```
| Method           | Mean      | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|----------------- |----------:|----------:|----------:|-------:|-------:|----------:|
| ManualDi.Sync    | 4.244 us  | 0.0823 us | 0.0948 us | 0.2747 | 0.0076 |  13.73 KB |
| ManualDi.Async   | 6.858 us  | 0.1267 us | 0.1185 us | 0.3128 | 0.0153 |  15.51 KB |
| MicrosoftDi      | 39.258 us | 0.2415 us | 0.2259 us | 2.5024 | 0.6714 | 122.87 KB |
```

[Benchmark](https://github.com/PereViader/ManualDi/blob/main/ManualDi.Unity3d/Assets/ManualDi.Unity3d/Tests/Benchmark.cs) against Unity3d compatible containers.

> TODO update this graphic blow

![Unity3d-Container-Benchmark](https://github.com/user-attachments/assets/67065b14-dea1-494a-b53e-469ebaf50101)

# Installation

- Plain C#: Install it using [Nuget](https://www.nuget.org/packages/ManualDi.Async/)
- Unity3d [2022.3.29](https://github.com/PereViader/ManualDi/issues/25) or later [Unity Package Manager](https://docs.unity3d.com/Manual/upm-ui-giturl.html) 
  - OpenUPM: https://openupm.com/packages/com.pereviader.manualdi.async.unity3d/
  - Git URL: https://github.com/PereViader/ManualDi.Unity3d.git

Note: Source generation will only happen in csproj that depend both on the source generator and is linked with the ManualDi library.
- In a regular C# project, this requires referencing the library on the csproj as a nuget package
- In a Unity3d project, this requires adding the library to the project through the Package Manager and then referencing ManualDi on each assembly definition where you want to use it

# Examples

Let's compare a small usecase for both the Sync and Async variants

## ManualDi.Sync

```csharp
using DiContainer diContainer = new DiContainerBindings()
    .Install(b => {
        b.Bind<SomeClass>().Default().FromConstructor();
        b.Bind<ISomeService, SomeService>().Default().FromConstructor();
        b.Bind<Startup>().Default().FromConstructor();
        b.WithStartup<Startup>(static startup => startup.Execute());
    })
    .Build();
    
//Once it reaches this line, both classes will have been created, initialized in the proper order and Startup.Execute will have been run

public class SomeClass
{
    public SomeClass(ISomeService someService)
    {
        // All required classes are instantiated in an initial pass.
        // The order of instantiation and subsequent method execution is determined dynamically at runtime
        // based on dependency analysis, ensuring automatic adaptation to changes.
    }

    public void Inject(ISomeService someService)
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
        // In this example SomeClass.Initialize runs after SomeService.Initialize (because SomeClass depends on ISomeService)
    }
}

public interface ISomeService { }

public class SomeService : ISomeService
{
    public void Initialize() { 
        // Some initialization here
    }
}

public class Startup(SomeClass someClass, ISomeService someService) // Constructor injected
{
    public void Execute()
    {
        //This runs after SomeClass.Initialize and SomeService.Initialize
    }
}
```

# ManualDi.Async

```csharp
await using DiContainer diContainer = await new DiContainerBindings()
    .Install(b => {
        b.Bind<SomeClass>().Default().FromConstructor();
        b.Bind<ISomeService, SomeService>().Default().FromConstructor();
        b.Bind<Startup>().Default().FromConstructor();
        b.WithStartup<Startup>(static async (startup, ct) => startup.Execute(ct));
    })
    .Build(CancellationToken.None);
    
//Once it reaches this line, both classes will have been created, initialized in the proper order and Startup.Execute will have been run

public class SomeClass
{
    public SomeClass(ISomeService someService)
    {
        // All required classes are instantiated in an initial pass.
        // The order of instantiation and subsequent method execution is determined dynamically at runtime
        // based on dependency analysis, ensuring automatic adaptation to changes.
    }

    public void Inject(ISomeService someService)
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
        // In this example SomeClass.Initialize runs after SomeService.InitializeAsync (because SomeClass depends on ISomeService)
    }
}

public interface ISomeService { }

public class SomeService : ISomeService
{
    public void InitializeAsync() { 
        // Some initialization here
    }
}

public class Startup(SomeClass someClass, ISomeService someService) // Constructor injected
{
    public async Task Execute(CancellationToken ct)
    {
        //This runs after SomeClass.Initialize and SomeService.InitializeAsync
    }
}
```


# Quick concepts

Let's brief some concepts from the library

- The container is created using a builder `DiContainerBindings`
- When building the container, `Bind` extensions are provided to bind types into the container
- When binding types, exposed directly with `Bind<TConcrete>()` or through some other apparent type `Bind<TApparent, TConcrete>()`
- The resolution of the bindings can happen quickly thanks to the source generated `Default` and `FromConstructor` methods that avoid all need for reflection.
- The source generated `FromConstructor` will provide all the dependencies requested on the constructor by resolving them from the container
- The source generated `Default` method inspects and registers public/internal `Construct`, `Inject`, `Initialize` and `InitializeAsync` methods to the container building pipeline
- All Duck typed source generated methods `Constructor`, `Construct`, `Inject`, `Initialize`, `InitializeAsync` are optional.
- There are other construction methods other than `FromConstructor` available.
- The object graph is created taking into account the dependency order defined implicitly in the parameters requested
- The object graph is initialized in the same order it is created in
- Once the object graph is created and initialized, Startup registrations are called


# Container Lifecycle

- Binding Phase: Container binding configuration is defined 
- Building Phase: Object graph is created, instances are created and initialized.
- Startup Phase: Startup startup callbacks are run.
- Usage Phase: Container is returned to the user and it can be kept until it is no longer necessary. 
- Disposal Phase: The container and its resources are released.

# Binding

The configuration of the container is done through Binding extension methods available on `DiContainerBindings` and can only be set during the installation phase. 
Any alteration by custom means after the container's creation may result in undefined behaviour.

Calling the Bind method provides a fluent interface through `Binding<TApparent, TConcrete>`.
- Apparent: It's the type that can be used when resolving the container.
- Concrete: It's type of the actual instance behind the scenes.

By convention, method calls should be done in the following order.

```csharp
Bind<(TApparent,)? TConcrete>() // TApparent is optional and will be equal to TConcrete if undefined
    .Default   // Source generated
    .From[Constructor|Instance|Method|MethodAsync|ContainerResolve|SubContainerResolve|...]  //Constructor is source generated
    .DependsOn // **ManualDi.Async only
    .Inject
    .Initialize
    .Dispose
    .WithId
    .When([InjectedIntoId|InjectedIntoType])
    .[Any other custom extension method your project implements]
```

Sample extension method installer:
```csharp
class A { }
class B { }
interface IC { }
class C : IC { }

static class Installer
{
    public static DiContainerBindings InstallSomeFunctionality(this DiContainerBindings b)
    {
        b.Bind<A>().Default().FromInstance(new A());
        b.Bind<B>().Default().FromConstructor();
        b.Bind<IC, C>().Default().FromConstructor();
        return b;
    }
}
```

## Binding

## Default

This source generated method is a shorthand for calling `Construct`, `Inject`, `Initialize` and `InitializeAsync` (ManualDi.Async only) that classes may have.
`Construct`, `Inject` methods may have 0 or N parameters and the parameters will be supplied by resolving them from the container. 
`Initialize` methods must have 0 parameters.
`InitializeAsync` methods must have 1 paramter being `System.Threading.CancellationToken` and must return `System.Threading.Task`
Think of it as a "duck typed" source generated approach.
It will only consider `public` or `ìnternal` methods.

In case there is more than one of a single meathod in a type: 
- It will prefer `public` methods over `internal` ones
- It will take the first one vertically in case there is more than one

When injecting dependencies to the class, it is preferred that it is done on the Constructor of the type. In case the constructor is not possible, then the `Construct` method is provided (in Unity3d, `MonoBehaviour` derived types are unable to use the consturctor and thus injection is fine to happen separetly).
Only in the case that there is some circular dependency the `Inject` method should be used

For more concrete details on the Construct, Inject and Initialize functionality read the sections on the topics below.

```csharp
public class A { }
public class B { 
    public void Inject(A a) { } //Sample only, prefer constructor
}
public class C { 
    public void Initialize() { }
}
public class D {
    public D(A a) { }
    public void Inject(C c, A a) { } //Sample only, prefer constructor
    public Task InitializeAsync(CancellationToken ct) { ... }
}


b.Bind<A>().Default().FromConstructor(); // Default does not call anything
b.Bind<B>().Default().FromConstructor(); // Default calls Construct
b.Bind<C>().Default().FromConstructor(); // Default calls Initialize
b.Bind<D>().Default().FromConstructor(); // Default calls Inject and InitializeAsync
```

Using default is not required, but it helps speed up development because the developer are only required to implement standardized DI boilerplate once and then then the actual DI resolution code will be updated automatically. 
For this reason, it is recommended to always add it even if the type does not currently have any of the methods.

Note: As stated on the installation section, the default method will only be generated for classes that live in assamblies that reference both ManualDi and the source generator.
In other words, 3rd party libraries and System classes will not have any generated code on them.

Note: A limitation of the source generator is that it does not run for partial classes defined across multiple declarations. It will only operate on partial classes that are declared once.

## From

These methods define the instance creation strategy

### Constructor

This method is source generated ONLY if there is a single public/internal accessible constructor.

The instance is created using the constructor of the concrete type. 
The necessary dependencies of the constructor are resolved using the container.

Like the on the Default method the following rules for constructor selection will apply:
- It will prefer `public` methods over `internal` ones
- It will take the first one vertically in case there is more than one

```csharp
b.Bind<T>().Default().FromConstructor() 
```

### Instance

No new instances will be created, the instance provided during the binding stage is used directly.
Note: If used with `Transient` scope, the container will still return the same instance.

```csharp
b.Bind<T>().Default().FromInstance(new T())
```

### Method

The instance is created using the delegate provided. The container is provided as a parameter thus allowing it to Resolve required dependencies.

```csharp
b.Bind<T>().Default().FromMethod(c => new T(c.Resolve<SomeService>()))
```

### MethodAsync 
(**ManualDi.Async only)

The instance is created using async delegate provided. The container and a cancellation token are provided as parameters thus allowing it to Resolve required dependencies asynchronously

```csharp
b.Bind<T>().Default().FromMethodAsync(async (c, ct) => {
    // Work can be delayed for any reason
    // For example: HttpRequests, Loading from disk, etc
    await Task.Delay(300, ct); 
    return new T(c.Resolve<SomeService>())
    });
```

### ContainerResolve

Used for apparent type remapping.
Commonly used to bind multiple relevant interfaces of some type to the container.
Notice that when redirecting a binding, the Default method should not be called. Otherwise the internal methods of the class would be called more than once.

```csharp
interface IFirst { }
interface ISecond { }
public class SomeClass : IFirst, ISecond { }
b.Bind<SomeClass>().Default().FromConstructor();
b.Bind<IFirst, SomeClass>().FromContainerResolve();
b.Bind<ISecond, SomeClass>().FromContainerResolve();
```

However to make it clear that you are redirecting some binding and also in order to avoid the issue mentioned above the extension below should be used instead

```csharp
interface IFirst { }
interface ISecond { }
public class SomeClass : IFirst, ISecond { }
b.Bind<IFirst, ISecond, SomeClass>().Default().FromConstructor();
```

### SubContainerResolve

The instance is created using a sub container built using the installer parameter.
Useful for encapsulating parts of object graph definition into sub-containers.
The subcontainer will have the main container as the parent. This means that types on the subcontainer can depend on things on the main container
Don't call Default when using this, the Default should be called within the subcontainer installation

```csharp
class SomeService(Dependency dependency) { }
class Dependency { }

b.Bind<SomeService>().FromSubContainerResolve(sub => {
    sub.Bind<SomeService>().Default().FromConstructor();
    sub.Bind<Dependency>().Default().FromConstructor();
})
```

### IsolatedSubContainerResolve

The instance is created using a sub container built using the installer parameter.
Useful for encapsulating parts of object graph definition into sub-containers.
The subcontainer will NOT have the main container as the parent. This means that types on the subcontainer can NOT depend on things on the main container
Don't call Default when using this, the Default should be called within the subcontainer installation

```csharp
class SomeService(Dependency dependency) { }
class Dependency { }

b.Bind<SomeService>().FromIsolatedSubContainerResolve(sub => {
    sub.Bind<SomeService>().Default().FromConstructor();
    sub.Bind<Dependency>().Default().FromConstructor();
})
```

## Construct

The Construct method allows for post-constructor injection of types.
This method should be used when constructor injection of types is not available. This can happen for example on Unity3d where `UnityEngine.Object` derived types can't use constructors.
The construct injection will happen immediately after the object creation.
The injection will be done in reverse resolution order. In other words, injected objects will already be injected themselves.
The injection will not happen more than once for any instance.
The injection can also be used to run other custom user code during the object creation lifecycle.
Any amount of injection callbacks can be registered

As stated on the `Default` section, calling the source generated method will handle the Construct method automatically.



## Inject

The Inject method allows for post-construct injection of types.
The Inject method should ONLY be used to Inject types and those types create circular dependencies. In that case, one of the types in the circular chain should be injected with Inject.


The injection will happen after the object construction.
The injection will be done in reverse resolution order. In other words, injected objects will already be injected themselves.
The injection will not happen more than once for any instance.
The injection can also be used to run other custom user code during the object creation lifecycle.
Any amount of injection callbacks can be registered

As stated on the `Default` section, calling the source generated method will handle the Inject method automatically.

Warning: Cyclic dependencies usually highlight a problem in the design of the code. If you find such a problem in your codebase, consider redesigning the code before applying the following proposal.

This will fail because it will run out of stack

```csharp
b.Bind<A>().FromMethod(c => new A(c.Resolve<B>()));
b.Bind<B>().FromMethod(c => new B(c.Resolve<A>()));
```

This will work because A has broken the cyclic dependency chain.
```csharp
b.Bind<A>().FromMethod(c => new A())).Inject((o,c) => o.B = c.Resolve<B>());
b.Bind<B>().FromMethod(c => new B(c.Resolve<A>()));
```

## Initialize

The Initialize method allows for post-injection initialization and injection of types.
The initialization will NOT happen immediately after the object injection. It will be queued and run later.
The initialization will be done in reverse resolution order. In other words, injected objects will already be initialized themselves.
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

Imagine you have some `IFeature` interface in your project and you want to some shared initialization code to the ones that have it. You can add this code in an "Link" extension method. Internally this extension method should just call the `Initialize` method and add whatever extra logic the feature requires.

You may find further Link examples already present in the library [here](https://github.com/PereViader/ManualDi/blob/540cb3d3155d81dc8925d9ab5769d2a18e61e81b/ManualDi.Unity3d/Assets/ManualDi.Unity3d/Runtime/Extensions/TypeBindingLinkExtensions.cs#L8)

## Dispose

Objects may implement the IDisposable interface or require custom teardown logic. 
The Dispose extension method allows defining behavior that will run when the object is disposed. 
The container will dispose of the objects when itself is disposed.
The objects will be disposed in reverse resolution order.

If an object implements the IDisposable interface, it doesn't need to call Dispose, it will be Disposed automatically.

The default source generated method will apply a very slight optimization by precalculating types implementing IDisposable. However calling Default is not required.

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

### DontDispose

If this extension method is called, the method will not call the `IDisposable.Dispose` method.
Any delegate registered to the Dispose method will still be called.


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
    [Inject("Potato")] public B B { get; set; }

    public void Inject(int a, [Inject("Other")] object b) { ... }
}

//Will resolve the property doing
c.Resolve<B>(x => x.Id("Potato"));

//and call the inject method doing
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

## Extra Source Generator features

### Nullable

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

### Collection

The source generator will inject all bound instances of a type if the dependency declared is one of the following types `List<T>` `IList<T>` `IReadOnlyList<T>` `IEnumerable<T>`
The resolved dependencies will be resolved using `ResolveAll<T>`
If the whole collection is nullable, the provided collection will always have `Count > 0` otherwise null.
If the underlying `T` is nullable, the resulting list will be converted from `T` to `T?`. This is not recommended, the nullable values will never be null.

The differences between `TCollection<T?>` and `TCollection<T>?`:
- `TCollection<T?>` does a single resolution but allocates a list every time
- `TCollection<T>?` does a dry resolution at build time and thus may not allocate the list

```csharp
public class A
{
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

The container provides functionality that queues work to be done once the continer is built and ready.
By using this you can define the entry points of your application declarativly during the installation of the container.

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

When using the container in unity, do not rely on Awake / Start. Instead, rely on Construct / Inject / Initialize / InitializeAsync.
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
This way Unity3d will serialize them so they can be linked through the inspector. Private serialize field members may be used but they add extra unnecesary boilerplate. 

Most methods have several optional parameters.

Special attention to `Transform? parent = null`. This one will define the parent transform used when instantiating new instances.

Special attention to `bool destroyOnDispose = true` one. This one will be available on creation strategies that create new instances.
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
If the subordinate container requires all the dependencies of the parent container, it is recommended to set the parent container on the EntryPointData object.

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

And this is how a subordinate entry point on a scene could be started

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

and this is an example of how you could use a subordinate prefab


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

- `LinkDontDestroyOnLoad`: The object will have don't destroy on load called on it when the container is bound. Behaviour can be customized with the optional parameters

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
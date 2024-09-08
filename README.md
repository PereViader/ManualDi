[![Test and publish](https://github.com/PereViader/ManualDi/actions/workflows/TestAndPublish.yml/badge.svg)](https://github.com/PereViader/ManualDi/actions/workflows/TestAndPublish.yml) [![NuGet Version](https://img.shields.io/nuget/v/ManualDi.Main)](https://www.nuget.org/packages/ManualDi.Main) [![Release](https://img.shields.io/github/release/PereViader/ManualDi.svg)](https://github.com/PereViader/ManualDi/releases/latest) [![Unity version 2022.3.29](https://img.shields.io/badge/Unity-2022.3.29-57b9d3.svg?style=flat&logo=unity)](https://github.com/PereViader/ManualDi.Unity3d)


Welcome to ManualDi. A manual dependency injection library for both Unity3d and plain C# solutions.

The project is grounded on the following five fundamentals:
 - Fast: Operations must be quick and efficient.
 - Flexible: The container's pipeline should support custom functionality.
 - Familiar: Simple, known concepts should be prioritized.
 - Fathomable: Behaviour should be easy to read and understand.
 - Functional: Achieving desired behaviors should require minimal boilerplate.

# Benchmark

## Plain C#
Let's compare this container with Microsoft's one given it is the standard most projects use today. [(Benchmark)](https://github.com/PereViader/ManualDi/blob/main/ManualDi.Main/ManualDi.Main.Benchmark/SimpleBenchmark.cs)

 - Combined GC consumption for setup and resolution is reduced by 7.33 times.
 - Object graph resolution is 7.9 times faster.
 - Disposal speed is 17 times faster.
 - Setup is 1.2 times faster.
 - Repeated access performance remains equivalent.

```
| Method                           | Mean          | Error       | StdDev       | Median          | Gen0   | Gen1   | Allocated |
|--------------------------------- |--------------:|------------:|-------------:|----------------:|-------:|-------:|----------:|
| ManualDi_Setup                   |   4,646.66 ns |    56.60 ns |     44.19 ns |   4,636.3789 ns | 0.3204 | 0.0305 |   16264 B |
| MicrosoftDi_Setup                |   5,730.04 ns |   110.85 ns |    172.57 ns |   5,696.2318 ns | 0.5951 | 0.5951 |   30232 B |
| ManualDi_Dispose                 |      47.00 ns |    17.68 ns |     52.14 ns |       0.0000 ns |      - |      - |     640 B |
| MicrosoftDi_Dispose              |     829.00 ns |    42.93 ns |    126.57 ns |     800.0000 ns |      - |      - |     640 B |
| ManualDi_Resolve_Service         |  19,096.94 ns |   759.32 ns |  2,214.97 ns |  20,000.0000 ns |      - |      - |    6296 B |
| MicrosoftDi_Resolve_Service      | 151,230.93 ns | 6,041.54 ns | 17,527.61 ns | 141,500.0000 ns |      - |      - |  135136 B |
| ManualDi_Resolve_ServiceTwice    |     946.94 ns |    40.95 ns |    119.45 ns |     900.0000 ns |      - |      - |     640 B |
| MicrosoftDi_Resolve_ServiceTwice |     987.50 ns |    21.58 ns |     33.60 ns |   1,000.0000 ns |      - |      - |     640 B |
```

## Unity3d

Let's compare the container against some other common Unity3d compatible containers. [(Benchmark)](https://github.com/PereViader/ManualDi/blob/main/ManualDi.Unity3d/Assets/ManualDi.Unity3d/Tests/Benchmark.cs)

![Unity3d-Build-Benchmark](https://github.com/user-attachments/assets/fe5c13ad-82c8-4c45-a8f8-5a818c6d97a7) 
![Unity3d-Resolve-Benchmark](https://github.com/user-attachments/assets/15589e49-5106-4855-bc20-41e0dd50bfcd)

# Installation

## Nuget

Install it using [Nuget](https://www.nuget.org/packages/ManualDi.Main/)

## Unity3d

Install it using the [Unity Package Manager](https://docs.unity3d.com/Manual/upm-ui-giturl.html) with the following git url: https://github.com/PereViader/ManualDi.Unity3d.git

Compatible with [Unity 2022.3.29](https://github.com/PereViader/ManualDi/issues/25) or later

# Examples

If you want to quickly jump to the code, you can find examples [here](https://github.com/PereViader/ManualDi/tree/main/ManualDi.Main/ManualDi.Main.Tests) and [here](https://github.com/PereViader/ManualDi/tree/main/ManualDi.Unity3d/Assets/ManualDi.Unity3d/Tests)

# Creation

The container is created using a fluent Builder

```csharp
IDiContainer container = new DiContainerBindings()  // Declare the fluent builder
    .InstallSomeFunctionality() // Configure with an extension method implemented in your project
    .Install(new SomeOtherInstaller()) // Configure with an instance of `IInstaller` implemented your project
    .Build(); // Build the container
```

Creation of the container is a synchronous process, if you need to do any kind of asynchronous work you can do the following:
- Simple but with delayed construction: Load the asynchronous data before creating the container, then provide it synchronously.
```csharp
SomeConfig someConfig = await GetSomeConfig(); 

IDiContainer container = new DiContainerBindings()
    .InstallSomeFunctionality(someConfig)
    .Build();
```

- Avoids delayed construction but complex: Handle asynchronous loading during runtime, designing the object graph to work even if some dependencies aren't immediately available.
```csharp
IDiContainer container = new DiContainerBindings()
    .InstallSomeFunctionality()
    .Build();

var initializer = container.Resolve<Initializer>();

await initializer.StartApplication();
```

# Container Lifecycle

- Installation Phase: The container's configuration is defined and set up.
- Building Phase: The container is created and ingests the configuration. Non lazy Bindings are resolved.
- Startup Phase: The Startup callbacks addded during the Building Phase are run before the container is returned.
- Resolution Phase: The container can be used to resolve services as needed.
- Disposal Phase: The container and its resources are properly cleaned up and released.

# Binding

The configuration of the container may only set during the installation phase. 
Any alteration by custom means after the container's cration may result in undefined behaviour.

The previous section displays how to create the container, but the configuration of the container is done on some opaque installer extension method or object. 

Configuration of the container is done through Binding extension methods available on `DiContainerBindings`. 
These binding methods require the `Concrete` and `Apparent` types being bound:
- Concrete: It's type of the actual instance behind the scenes
- Aparent: It's the type that can be used when resolving the container.

This is a sample implementation of an installer extension method

```csharp
class A {}
class B {}
interface IC {}
class C : IC{}

static class Installer
{
    public static DiContainerBindings InstallSomeFunctionality(this DiContainerBindings b)
    {
        //Adding more than one bind method 
        b.Bind<A>()...      // Aparent and concrete type is A
        b.Bind<B, B>()...   // Aparent and concrete type is B, if it was specified once it would be more readable
        b.Bind<IC, C>()...  // Aparent type is IC and concrete type is C
    }
}
```
The result of calling the Bind with generic arguments is a `TypeBinding<T, Y>`. This type provides several extension used to define the exact behaviour of the binding.
By convention, you should call methods in the following order.

```csharp
Bind<T>()
    .Default   // Source generated
    .[Single|Transient]
    .From[Constructor|Instance|Method|Container|ContainerAll|...]  //Constructor is source generated
    .Inject   //Empty overload is source generated
    .Initialize  //Empty overload is source generated
    .Dispose
    .WithMetadata
    .[Lazy|NonLazy]
    .[Any other custom extension method your project implements]
```

The bindings may also be done with a non type safe interface. This variant should only be used when implementing programatic driven configuration. Use the type safe variant when all the types involved are known.

```csharp
List<Type> someTypes = GetSomeTypesWithReflexion();
foreach(var type in someTypes)
{
    b.Bind(type)....
}
```

Using reflexion to do such bindings will slow down your application due to the runtime Type metadata analysis necessary.

If the reduced performance is not desired, [source generation](https://github.com/PereViader/ManualDi/tree/develop/ManualDi.Main/ManualDi.Main.Generators) of equivalent code that avoids reflexion can be done to do the analysis at build time.

Some platforms may not even be compatible with reflexion. If you target any such platform, using source generators is the only approach.

```csharp
b.InstallSomeTypes(); // This could be source generated to do the same but faster
```

## Binding

## Scope

The scope of a binding defines the rules of creation of instances of the binding.

### Single

The container will generate a single instance of the type and cache it.
Further calls with for the same resolution will return the cached instance.

### Transient

The container will generate a new instance every time the type is resolved.

## From

The creation strategy for the binding

### Constructor

This method is source generated ONLY if there is a single public/internal accessible constructor.
When the instance of the type is resolved, it will be created using the constructor of the concrete type. The required dependencies of the constructor will be resolved using the container.

```csharp
b.Bind<T>().FromConstructor()
```

### Instance

When the instance of the type is resolved, it will return the instance supplied during the binding stage.
Note: If used with `Transient` scope, the container will still return the same instance.

```csharp
b.Bind<T>().FromInstance(new T())
```

### Method

When the instance of the type is resolved, it will be created using the delegate provided. The delegate provides the container as a parameter, enabling it Resolve any other dependency from it.

Note: Given this is the most common use case, the container optimizes for this use case in terms of GC 

```csharp
b.Bind<T>().FromMethod(c => new T(c.Resolve<SomeService>()))
```

### ContainerResolve

Useful mapping one type of the container as another

```csharp
b.Bind<int>().FromInstance(1);
b.Bind<object, int>().FromContainerResolve();

// ...

System.Console.WriteLine(c.Resolve<object>()); // Outputs "1"
```

Using FromContainerResolve is a shorthand for:

```csharp
b.Bind<object, int>().FromMethod(c => c.Resolve<int>());
```

### SubContainerResolve

This method allows resolving instances from a sub-container, enabling more modular and flexible container setups. A sub-container is created using the provided InstallDelegate, with the option to inherit the parent container (isContainerParent), giving precise control over lifetimes and resolution scopes.

```
class SomeService(Dependency dependency) { }
class Dependency { }

b.Bind<SomeService>().FromSubContainerResolve(sub => {
    sub.Bind<SomeService>().Default().FromConstructor();
    sub.Bind<Dependency>().Default().FromConstructor();
})
```


## Inject

The Inject method allows for post-construction injection of types.
The injection will happen immediately after the object creation.
The injection will be done in reverse resolution order. In other words, injected objects will already be injected themselves.
The injection will not happen more than once for any instance.
The injection can also be used to run other custom user code during the object creation lifecycle.

### Source Generator

An empty overload of the Inject method will be generated if:
- The type has a single public/internal accessible Inject method. The method may have 0 or more dependencies.
- The type has any amount of public/internal accessible properties that use the Inject attribute

The generated method will first do property injection on the properties and then call the inject method.

```csharp
public class A
{
    [Inject] public object Object { get; set; }
    [Inject] public int Value { get; set; }

    public void Inject(B b, C c) { }
}

public class B
{
    [Inject] public object Object { get; set; }
}

public class C
{
    public void Inject() { }
}

b.Bind<object>().FromInstance(new object());
b.Bind<int>().FromInstance(3);
b.Bind<A>().FromConstructor().Inject();
b.Bind<B>().FromConstructor().Inject();
b.Bind<C>().FromConstructor().Inject();
```

### Example1: Cannot change the constructor

In the unity engine, for example, types that that derive from `UnityEngine.Object` cannot make use of the constructor.
For this reason, derived types will usually resort to this kind of injection

```csharp
public class SomethingGameRelated : MonoBehaviour  // this class derives from UnityEngine.Object
{
    [Inject] public SomeGameService SomeGameService { get; set; }

    public void Inject()
    {
        //Do something
    }
}
```

#### Example2: Cyclic dependencies

Warning: Cyclic dependencies usually highlight a problem in the design of the code. If you find such a problem in your codebase, consider redesigning the code before applying the following proposal.

This will throw a stack trace exception when any of the types involved in the cyclic chain is resolved.

```csharp
b.Bind<A>().FromMethod(c => new A(c.Resolve<B>()));
b.Bind<B>().FromMethod(c => new B(c.Resolve<A>()));
```

This will work.
As long as a single object in the cyclic chain breaks the chain, the resolution will be able to complete successfully.

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

```csharp
b.Bind<A>().FromInstance(new A()).Initialize((o,c) => o.Init());

c.Resolve<A>()
```

### Source Generation

An empty overload of the Initialize method will be generated if:
- The type has a single public/internal accessible Initialize method. The method may have 0 or more dependencies.

```csharp
public class A
{
    public void Initialize() { }
}

public class B
{
    public void Initialize(A a) { }
}

b.Bind<A>().FromConstructor().Initialize();
b.Bind<B>().FromConstructor().Initialize();
```

## Dispose

Objects may implement the IDisposable interface or require custom teardown logic. 
The Dispose extension method allows defining behavior that will run when the object is disposed. 
The container will dispose of the objects when itself is disposed.
The objects will be disposed in reverse resolution order.

If an object implements the IDisposable interface, it doesn't need to call Dispose, it will be Disposed automatically.

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

b.Bind<A>().FromConstructor(); // No need to call Dispose because the object is IDisposable
b.Bind<B>().FromConstructor().Dispose((o,c) => o.DoCleanup());

// ...

B b = c.Resolve<B>();

c.Dispose(); // A is the first object disposed, then B
```

### DontDispose

If this extension method is called, the method will not call the `IDisposable.Dispose` method.
Any delegate registered to the Dispose method will still be called.


## With Metadata

These extension methods allow registering keys or key/value pairs, enabling the filtering of elements during resolution.

```csharp
b.Bind<int>().FromInstance(1).WithMetadata("Potato");
b.Bind<int>().FromInstance(5).WithMetadata("Banana");

// ...

c.Resolve<int>(b => b.WhereMetadata("Potato")); // returns 1
c.Resolve<int>(b => b.WhereMetadata("Banana")); // returns 5
```


## Laziness

### Lazy

The FromMethod delegate will not be called until the object is actually resolved.
By default bindings are Lazy so calling this method is usually not necessary.

### NonLazy

The object will be built simultaneously with the container.

## Default

This source generated method is a shorthand for calling Inject and Initialize when they are available without needing to manually update the container bindings.

This means that

```csharp
public class A { }
public class B { 
    void Inject(A a) { }
}
public class C { 
    void Initialize(A a) { }
}
public class D {
    void Inject(A a) { }
    void Initialize(A a) { }
}


b.Bind<A>().Default().FromConstructor(); // Default does not call anything
b.Bind<B>().Default().FromConstructor(); // Default calls Inject
b.Bind<C>().Default().FromConstructor(); // Default calls Initialize
b.Bind<D>().Default().FromConstructor(); // Default calls Inject and Initialize
```

Using default is not mandatory, but it helps development by taking the responsability of updating the bindings away from the developer any time a new Inject / Initialize method is introduced.

Thus it is recommended to always add it even if the type does not currently have any of the methods.

## Extra Source Generator features

### Nullable

The source generator will take into account the nullability of the dependencies.
If a dependency is nullable, the resolution will not fail if it is missing.
If a dependency is not nullable, the resolution will fail if it is missing.

```
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
The underlying `T` type may NOT be nullable.
The whole collection type may NOT be nullable

```
public class A
{
    [Inject] List<object> ListObj {get; set;}
    [Inject] IList<int> IListInt {get; set;}
    [Inject] IReadOnlyList<obj> IReadOnlyListObj {get; set;}
    [Inject] IEnumerable<int> IEnumerableInt {get; set;}

    //[Inject] List<object?> NullableList1 {get; set;} //DON'T do this
    //[Inject] List<object>? NullableList2 {get; set;} //DON'T do this
}

b.Bind<A>().Default().FromConstructor();
```

### Lazy<T>

The source generator will lazily inject dependencies if the dependency is lazy itself.
Lazy dependencies may have nullable contents.
Lazy dependencies may NOT be nullable themselves
Lazy dependencies are also compatible with collection .
Lazy Collection dependencies are also supported

```
public class A
{
    [Inject] Lazy<object> Obj {get; set;}
    [Inject] Lazy<object?> NullableObj {get; set;}
    [Inject] Lazy<int> Value {get; set;}
    [Inject] Lazy<int?> NullableValue {get; set;}
    [Inject] Lazy<List<object>> LazyObjectList {get; set;}

    //[Inject] Lazy<object>? NullableObj {get; set;} //DON'T do this
    //[Inject] Lazy<object?>? NullableObj {get; set;} //DON'T do this
}

b.Bind<A>().Default().FromConstructor();
```

# Resolving

Notice that resolution can only be done on aparent types, not concrete types. Concrete types are there so the continer can provide a type safe fluent API.

If you use the source generated methods, you will usually not interact with the Resolution methods.

Resolutions can be done in the following ways:

## Resolve

Resolve a single registered instance from the container. An exception is thrown if it can't be resolved.

```csharp
SomeService service = container.Resolve<SomeService>();
```

## ResolveNullable

Resolve a single reference type registered instance from the container. Returns null if it can't be resolved.

```csharp
SomeService? service = container.ResolveNullable<SomeService>();
```

## ResolveNullableValue

Resolve a single value type registered instance from the container. Returns null if it can't be resolved.

```csharp
int? service = container.ResolveNullableValue<int>();
```

## TryResolve

Resolve a single instance from the container. Returns true if found and false if not.

```csharp
bool found = container.TryResolve<SomeService>(out SomeService someService);
```

## ResolveAll

Resolve all the registered instance from the container. If no instances are available the list is empty.

```csharp
List<SomeService> services = container.ResolveAll<SomeService>();
```

# Startups

The container also provides an extension method so it can queue some delegate to be run during the Startups lifecycle of the container.
This step is useful to make sure that some object's method is run after all the NonLazy objects have been created and initialized.

In the following example the following will happen once the container is built:
- `SomeNonLazy` is created 
- `SomeNonLazy`'s `Initialize` method is be called
- `Startup` is created
- `Startup`'s `Start` method is called

```csharp
class Startup
{
    public Startup(...) { ... }
    public void Start() { ... }
}

class SomeNonLazy
{
    public void Initialize() { ... }
}

b.Bind<SomeNonLazy>().Default().FromConstructor().NonLazy()
b.Bind<Startup>().Default().FromConstructor();
b.WithStartup<Startup>(o => o.Start());
```

# ManualDi.Unity3d

When using the container in unity, do not rely on Awake / Start. Instead rely on Inject / Initialize.
You can still use Awake / Start if the classes involved are not injected through the container.

## Installers

The container provides two specialized Installers 
- `MonoBehaviourInstaller` 
- `ScriptableObjectInstaller`

This is the idiomatic Unity way to have both the configuration and engine object references in the same place.
These classes just implement the `IInstaller` interface, there is no requriement for these classes to be used, so feel free to use IInstaller directly if you want.

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
- `FromInstantiateGameObjectGetComponentInParent`: Instantiates a GameObject and retrieves a component from its parent.
- `FromInstantiateGameObjectGetComponentInChildren`: Instantiates a GameObject and retrieves a component from its children.
- `FromInstantiateGameObjectGetComponents`: Instantiates a GameObject and retrieves all components of a specific type.
- `FromInstantiateGameObjectGetComponentsInParent`: Instantiates a GameObject and retrieves all components from its parent.
- `FromInstantiateGameObjectGetComponentsInChildren`: Instantiates a GameObject and retrieves all components from its children.
- `FromInstantiateGameObjectAddComponent`: Instantiates a GameObject and adds a component to it.
- `FromObjectResource`: Loads an object from a Unity resource file by its path.
- `FromInstantiateGameObjectResourceGetComponent`: Instantiates a GameObject from a resource file and retrieves a component from it.
- `FromInstantiateGameObjectResourceGetComponentInParent`: Instantiates a GameObject from a resource and retrieves a component from its parent.
- `FromInstantiateGameObjectResourceGetComponentInChildren`: Instantiates a GameObject from a resource and retrieves a component from its children.
- `FromInstantiateGameObjectResourceGetComponents`: Instantiates a GameObject from a resource and retrieves all components of a specific type.
- `FromInstantiateGameObjectResourceGetComponentsInChildren`: Instantiates a GameObject from a resource and retrieves all components from its children.
- `FromInstantiateGameObjectResourceAddComponent`: Instantiates a GameObject from a resource and adds a component to it.

Use them like this

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

Most methods have several optional parameters.

Also special attention to `Transform? parent = null`. This one will define the parent transform used when instantiating new instances.


Special attention to `bool destroyOnDispose = true` one. This one will be available on creation strategies that create new instances.
If the parameter is left as `true`, when the container is disposed, it will first destroy the instance.
This is the necessary default behaviour due to the game likely needing those resources cleaned up for example from shared Additive scenes and wanting the default behaviour to be the safest.
If the scene the resource is created on will then be deleted, there is no need to destroy it during the disposal of the container, so feel free to set the parameter as `false`.

## EntryPoints

An entry point is a place where some context of your application is meant to start.
In the case of ManualDi, it is where the object graph is configured and then the container is started.

The last binding of an entry point will usually make use of WithStartable to run any logic necessary after the container is created.

### RootEntryPoint

Root entry points will not depend on any other container.
This means that all dependencies will be registered in the main container itself.

Use the appropiate type depending on how you want to structure your application:
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

Subodinate entry points will depend on other container or require other data.
This means that these entry points cannot be started by themselves. They need to be started by some other part of your application.

If the data provided to these entry points, implements `IInstaller`, then the data will also be installed to the container.
Otherwise it will just be available throught the `Data` property of the EntryPoint.
If the subordinate container requires all the dependencies of the parent container, it is recommended to set the parent container on the EntryPointData object.

```
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

Use the appropiate type depending on how you want to structure your application:
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

When binding things to the container, further actions can be done on the bindings

- `LinkDontDestroyOnLoad`: The object will have don't destroy on load called on it when the contianer is bound. Behaviour can be customized with the optional parameters

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

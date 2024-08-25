[![Test and publish](https://github.com/PereViader/ManualDi/actions/workflows/TestAndPublish.yml/badge.svg)](https://github.com/PereViader/ManualDi/actions/workflows/TestAndPublish.yml)

Welcome to ManualDi. A C# dependency injection library for both Unity3d and pure C# solutions. 

The principles this project is based on are:
 - Understandable: A new developer should be able to quickly understand how to use it
 - Familiar: Concepts used by the container should be the same as already as existing containers
 - Fast: The container should be able to resolve the object graph quickly and efficiently
 - Responsible: New features added to the container should not degrade performance
 - Natural: We should invest our time on providing value and not on getting things to work
 - Easy: Adding functionality should not require a high mental load
 - Pluggable:  Easily customize the container and provide extra functionality by hooking into its pipeline
 - Default: The container should do as much work as possible so the user has to do the least work as possible

# Benchmark

Let compare this container with Microsoft's one given it is the standard most projects use today.

When looking at them, keep in mind that
- CPU time depends on the specs of the pc running it
- CPU time and memory allocations are correlated with the amount of services bound to the container
- The benchmark numbers are only valid when comparing the same program implemented with either ManualDi or Microsoft's container.

## Simple Chain

For this case, we have a chain of 100 services where each depend on the previous one, all of them are transient but the first one that is a singleton and thus will be cached. Seconds resolution will happen on the single instance and thus it should reuse the previously built object graph.

- Combined GC consumption for the setup and resolution is 7,33 times lower 
- Aggregate performance is 
- Object graph resolution is 7,9 times faster
- Disposal is 17 times faster
- Setup is 1,2 times faster
- Repeated access is equivalent

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

# Installation

## Nuget

Install it using [Nuget](https://www.nuget.org/packages/ManualDi.Main/)

## Unity3d

Install it using Unity Package Manager with the following git url: https://github.com/PereViader/ManualDi.Unity3d.git

# Examples

If you want to quickly jump to the code, you can find examples [here](https://github.com/PereViader/ManualDi/tree/main/ManualDi.Main/ManualDi.Main.Tests) and [here](https://github.com/PereViader/ManualDi/tree/main/ManualDi.Unity3d/Assets/ManualDi.Unity3d/Tests)

# Creation

The container is created using a fluent Builder

```csharp
IDiContainer container = new DiContainerBindings()
    .InstallSomeFunctionality()
    .Install(new SomeOtherInstaller());
    .Build();
```

Let's analize this snippet:

- Declare the container variable of type `IDiContainer` called container.
- Create the `DiContainerBindings` builder
- Call the user created static extension method `InstallSomeFunctionality` which will bind services to the container
- Call `Install` with an `IInstaller` create by the user named `SomeOtherInstaller`. This does the same as the extension method, but with an object instead of a static function call
- Call `Build` which will freeze the bindings and return a container

Creation of the container is a synchronous process, if you need to do any kind of asynchronous work you can do the following:
- Simple but delays construction: Load the asyncronous data before creating the container and provide it syncronously
```csharp
SomeConfig someConfig = await GetSomeConfig(); 

IDiContainer container = new DiContainerBindings()
    .InstallSomeFunctionality(someConfig)
    .Build();
```

- Complex but does not delay construction of the object graph: Have the asynchronous loading as part of the runtime code and take into account that the necessary dependency will not be there as part of the runtime flow. Create an object graph that can support this fact, this can take many shapes depending on the usecase.


# Binding

First of all, it is important to understand that the configuration of the container may only be done during it's creation. Once built, the container's configuration is completely readonly. Changing the configuration after that will result in undefined behaviour.

The previous section displays how to create the container, but the configuration of the container happens on some opaque installers. 

In order to configure the container, Binding extension methods are provided on `DiContainerBindings`. Those binding methods require the `Concrete` and `Apparent` types being bound:
- Concrete: It's type of the actual instance behind the scenes
- Aparent: It's the type that can be used when resolving the container.

Let's see how is one of those installers implemented

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

There are several extension methods for the `TypeBinding<T, Y>` that will allow you to actually make the binding do something.
Although there is nothing preventing you from calling these methods in another order, the convention this library recommends is to call them in this order.

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
```

We will now go over each one of them

## Scope

The scope of a binding defines the rules of creation of instances of the binding.

### Single

The container will generate a single instance of the type and always return the same when asked to resolve it. Similar to the dreaded `Singleton` but not globally accessible. 

### Transient

The container will generate a new instance of the type when requested to resolve it.


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
Note: When used with a `Transient` scope, the container will still return the same instance.

```csharp
b.Bind<T>().FromInstance(new T())
```

### Method

When the instance of the type is resolved, it will be created using the delegate provided. The delegate provides the container as a parameter, enabling it Resolve any other dependency from it.

Note: Given this is the most common use case, the container optimizes for this use case in terms of GC 

```csharp
b.Bind<T>().FromMethod(c => new T(c.Resolve<SomeService>()))
```

### Container

Useful exposing a type on the container as another

```csharp
b.Bind<int>().FromInstance(1);
b.Bind<object, int>().FromContainer();

// ...

System.Console.WriteLine(c.Resolve<object>()); // Outputs "1"
```

In this snippet, an integer with a value of 1 is registered. Then an object is bound to the container by redirecting the integer binding to it. As a result, when the object is requested, the container resolves the integer and returns 1.

Using FromContainer is the same :

```csharp
b.Bind<object, int>().FromMethod(c => c.Resolve<int>());
```

### Container All

Just like `FromContainer` binds all the instances to the container

```csharp
b.Bind<int>().FromInstance(1);
b.Bind<int>().FromInstance(5);
b.Bind<List<object>, List<int>>().FromContainerAll();

// ...

foreach(var value in container.Resolve<List<object>>())
{
    System.Console.WriteLine(value); // Outputs "1" and then "5"
}
```

As seen before this is just a shorthand for ResolveAll

```csharp
b.Bind<List<object>, List<int>>().FromMethod(c => c.ResolveAll<int>().Cast<object>().ToList());
```


## Inject

### Theory

In some situations, it might not be possible to inject all dependencies for a type at the time of its creation due to various reasons. When this occurs, the Inject method allows for post-construction injection.

Instead of injecting the object immediately after construction, it is added to a queue to be injected later. This ensures that all dependent services are created and injected in the correct order.

The Inject method will be called once for every new type, depending on the scope.

Example:


Service A: Depends on B
Service B: Depends on A


Naive solution that won't work

```csharp
b.Bind<A>().FromMethod(c => new A(c.Resolve<B>()));
b.Bind<B>().FromMethod(c => new B(c.Resolve<A>()));

// ...

c.Resolve<A>();
```

- Start by getting service A
- The from method of A defines we call the constructor, but first we have to get an instance of B
- The from method of B defines we call the constructor, but first we have to get an instance of A
- The from method of A defines we call the constructor, but first we have to get an instance of B
- The from method of B defines we call the constructor, but first we have to get an instance of A
- The from method of A defines we call the constructor, but first we have to get an instance of B
- The from method of B defines we call the constructor, but first we have to get an instance of A
- The from method of A defines we call the constructor, but first we have to get an instance of B
- The from method of B defines we call the constructor, but first we have to get an instance of A
... this never ends and will fail when the stack is full

Actual solution that works

```csharp
b.Bind<A>().FromMethod(c => new A())).Inject((o,c) => o.B = c.Resolve<B>());
b.Bind<B>().FromMethod(c => new B(c.Resolve<A>()));

// ...

c.Resolve<A>();
```

- Start by getting service A
- The from method of A defines we call the constructor
- The inject method is called, which requests and instance of B
- The from method of B defines we call the constructor, but first we have to get an instance of A
- We get the instance of A already constructed and return it
- The constructor for B resolves
- A's inject method finishes
- A finishes being resolved

### Source Generator

An empty overload of the Inject method will be generated if the type has a public/internal accessible Inject method. The generated method will call the inject method with all the dependencies that are requested as parameters (it may have no parameters).

```csharp
public class A
{
    public void Inject(B b, C c) { }
}

b.Bind<A>().Inject();
```


## Initialize

### Theory

Similar to the Inject method, the Initialize method defines a delegate to be called for newly constructed instances before they are resolved.

Like the Inject method, newly created instances are added to a queue for initialization.

The Initialize method will be called once for every new type, depending on the scope.

```csharp
b.Bind<A>().FromInstance(new A()).Initialize((o,c) => o.Init());

c.Resolve<A>()
```

- Start by getting service A
- The from method of A defines we return the instance provided
- The initialize method calls the Init method on A

### Source Generation

An empty overload of the Initialize method will be generated if the type has a public/internal accessible Initialize method. The generated method will call the Initialize method with all the dependencies that are requested as parameters (it may have no parameters). 

```csharp
public class A
{
    public void Initialize() { }
}

b.Bind<A>().Initialize();
```

## Dispose

Objects may implement the IDisposable interface or require custom teardown logic. The Dispose extension method allows defining behavior that will run when the object is disposed. The container will handle disposal when itself is disposed.

If an object implements the IDisposable interface, it doesn't need to call Dispose, it will be Disposed automatically.

```csharp
// Given A implements IDisposable
// Given B does not implement IDisposable

b.Bind<A>().FromInstance(new A());
b.Bind<B>().FromInstance(new B()).Dispose((o,c) => o.DoCleanup);

container.Dispose(); // A and B disposed if they were created
```


## With Metadata

These extension methods allow registering keys or key/value pairs, enabling the filtering of elements during resolution.

```csharp
Given A does not implement IDisposable
and B implements IDisposable

b.Bind<int>().FromInstance(1).WithMetadata("Potato");
b.Bind<int>().FromInstance(5).WithMetadata("Banana");

// ...

c.Resolve<int>(b => b.WhereMetadata("Potato")); // returns 1
c.Resolve<int>(b => b.WhereMetadata("Banana")); // returns 5
```


## Laziness

### Lazy

The FromMethod delegate will not be called until the object is actually resolved.

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

Using default is not mandatory in any way, but it is a way to speed up development because the source generator will update bindings as the type is changed.


## Unity3d

When using the container in the Unity3d game engine the library provides specialized extensions

Section under construction, for now see the code https://github.com/PereViader/ManualDi/tree/main/ManualDi.Unity3d/Assets/ManualDi.Unity3d/Runtime


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
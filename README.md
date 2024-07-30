[![Run Tests](https://github.com/PereViader/ManualDi.Main/actions/workflows/run-tests.yml/badge.svg)](https://github.com/PereViader/ManualDi.Main/actions/workflows/run-tests.yml) [![Publish Nuget Package](https://github.com/PereViader/ManualDi.Main/actions/workflows/publish-nuget-package.yml/badge.svg)](https://github.com/PereViader/ManualDi.Main/actions/workflows/publish-nuget-package.yml) [![Publish Unity3d Package](https://github.com/PereViader/ManualDi.Main/actions/workflows/publish-unity3d-package.yml/badge.svg)](https://github.com/PereViader/ManualDi.Main/actions/workflows/publish-unity3d-package.yml)

# Introduction

Welcome to the ManualDi.Main project. A C# dependency injection library.

This project was initially conceived with the idea to be used as a dependency injection library to be used for Unity3d. Although it was built with Unity3d in mind, the project completely separates the game engine specific functionality on another [repository](https://github.com/PereViader/ManualDi.Unity3d) in order to be able to use this project in any C# project.

The principles this project is based on are:
 - Understandable: A new developer should be able to quickly understand how to use it
 - Familiar: Concepts used by the container should be the same as already as existing containers
 - Fast: The container should be able to resolve the object graph quickly and efficiently
 - Natural: We should invest our time on providing value and not on getting things to work
 - Easy: Adding functionality should not require a high mental load
 - Pluggable:  Users of the container should be able to customize the container from the outside as they wish
 - Generic: The container should not assume the needs of the user and should be usable on any project

# Installation

## Nuget
Install the package from [nuget](https://www.nuget.org/packages/ManualDi.Main/)

## Unity3d
See instructions on https://github.com/PereViader/ManualDi.Unity3d

# Source Generator

The project aims to provide a great user experience by encapsulating common tasks in a source generator.
The source generator helps by generating methods that require a lot of boilerplate that always looks the same.
The source generator is the way we can have a nice API while at the same time not using reflexion.
Methods that are source generated are:
- FromConstructor
- Initialize
- Inject 
- Default

# API

## Examples

To get to the action, please visit the automated tests of the project found on https://github.com/PereViader/ManualDi.Unity3d/tree/main/Assets/ManualDi/Tests

## Container creation

In order to create the container, the project offers a fluent Builder `ContainerBuilder`. Let's see how that would look like:

```csharp
    IDiContainer container = new DiContainerBuilder()
        .Install(b => 
        {
            b.InstallSomeFunctionality();
        })
        .Install(new SomeOtherInstaller());
        .Build();
```

Let's analize this snippet:

- Declare the container variable of type `IDiContainer` called container.
- Create the `DiContainerBuilder`
- Use the `Install` function to start binding data to the container
- Call the static extension method `InstallSomeFunctionality` which will bind services to the container
- Use the `Install` function to bind an installer of type `SomeOtherInstaller`. This does the same as the extension method, but with an object instead of a static function call
- Actually build the container with the contents we gave to the Builder 


## Binding services

### Binding

Let's understand now how to bind anything to the container. 
For startes, you may only bind to the container during it's creation.
This is a syncronous process.

Binding data to the container is performed on the type `DiContainerBindings`. It offers its functionality also through extension methods.

In order to bind to the container, there are 3 `Bind` methods. Let's concentrate on the two most common ones.

```csharp
ITypeBinding<TConcrete, TConcrete> Bind<TConcrete>(this IDiContainerBindings diContainerBindings)
ITypeBinding<TInterface, TConcrete> Bind<TInterface, TConcrete>(this IDiContainerBindings diContainerBindings)
```

The first, is meant to be used to bind an instance of some type T and expose the same type T as the interface type when resolving
The second is meant to be used to bind an instance of some type Y but expose a different type T as the interface when resolving


Examples:

Binding the instance and exposed interface as the same one

```csharp
b.Bind<SomeType>()
c.Resolve<SomeType>() // Success
```

Binding the instance and the exposed interface as different types

```csharp
b.Bind<ISomeType, SomeType>()
c.Resolve<SomeType>() // Runtime error
c.Resolve<ISomeType>() // Success
```


### Resolving

Before we define how to actually add the data to the container, it will be useful to understand how we will get the data from the container.

We can get data from the container in two ways

#### Resolve

We get a single registered instance from the container

```csharp
SomeService service = container.Resolve<SomeService>();
```

#### ResolveAll

If we registered multiple instances of the same type to the container this method will return all of them.

```csharp
List<SomeService> services = container.ResolveAll<SomeService>();
```


### Populating the bindings

We have seen on the previous section how to start binding, but we've not actually bound anything.
There are several extension methods for the `TypeBinding<T, Y>` that will allow you to actually make the binding do something.
Although there is nothing preventing you from calling these methods in another order, the convention this library recommends is to call them in this order.

```csharp
Bind<T>()
    .Default   // Source generated
    .[Single|Transient]
    .From[Constructor|Instance|Method|Container|ContainerAll]  //Constructor is source generated
    .Inject   //Empty overload is source generated
    .Initialize  //Empty overload is source generated
    .Dispose
    .WithMetadata
    .[Lazy|NonLazy]
```

We will now go over each one of them

### Scope

The scope of a binding defines the rules of creation of instances of the binding.

#### Single

The container will generate a single instance of the type and always return the same when asked to resolve it. Similar to the dreaded `Singleton` but not globally accessible. 

#### Transient

The container will generate a new instance of the type when requested to resolve it.


### From

The creation strategy for the binding

#### Constructor

Will be source generated ONLY if there is a single public/internal accessible constructor.
It will call that constructor of the type and resolve all the dependencies it requires from the container.

```csharp
b.Bind<T>().FromConstructor()
```

#### Instance

An instance of the type is directly supplied to the container, which will subsequently return it.
Note: When used with the transient scope, the container will still return the same instance instead of different ones as one may expect, due to the creation strategy always providing the same instance.

Example:

```csharp
b.Bind<T>().FromInstance(new T())
```

#### Method

A delegate to create instances of the type is provided to the container. This delegate accepts the fully resolved container as a parameter, enabling it to inject services into the type's constructor.

Note: Given this is the most common use case, the container optimizes for this use case in terms of GC 

```csharp
b.Bind<T>().FromMethod((c) => new T(c.Resolve<SomeService>()))
```

#### Container

Useful for exposing other interfaces of types on the container

```csharp
b.Bind<int>().FromInstance(1);
b.Bind<object, int>().FromContainer();

// ...

System.Console.WriteLine(c.Resolve<object>()); // Outputs "1"
```

In this snippet, an integer with a value of 1 is registered. Then an object is bound to the container by redirecting the integer binding to it. As a result, when the object is requested, the container resolves the integer and returns 1.

This is a shorthand for calling Resolve on the container for the type:

```csharp
b.Bind<object, int>().FromMethod(c => c.Resolve<int>());
```

#### Container All

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


### Inject

#### Theory

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

#### Source Generator

An empty overload of the Inject method will be generated if the type has a public/internal accessible Inject method. The generated method will call the inject method with all the dependencies that are requested as parameters (it may have no parameters).

```csharp
public class A
{
    public void Inject(B b, C c) { }
}

b.Bind<A>().Inject();
```


### Initialize

#### Theory

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

#### Source Generation

An empty overload of the Initialize method will be generated if the type has a public/internal accessible Initialize method. The generated method will call the Initialize method with all the dependencies that are requested as parameters (it may have no parameters). 

```csharp
public class A
{
    public void Initialize() { }
}

b.Bind<A>().Initialize();
```

### Dispose

Objects may implement the IDisposable interface or require custom teardown logic. The Dispose extension method allows defining behavior that will run when the object is disposed. The container will handle disposal when itself is disposed.

If an object implements the IDisposable interface, it doesn't need to call Dispose, it will be Disposed automatically.

```csharp
// Given A implements IDisposable
// Given B does not implement IDisposable

b.Bind<A>().FromInstance(new A());
b.Bind<B>().FromInstance(new B()).Dispose((o,c) => o.DoCleanup);

container.Dispose(); // A and B disposed if they were created
```


### With Metadata

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


### Laziness

#### Lazy

The FromMethod delegate will not be called until the object is actually resolved.

#### NonLazy

The object will be built simultaneously with the container.

### Default

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
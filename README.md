[![Run Tests](https://github.com/PereViader/ManualDi.Main/actions/workflows/run-tests.yml/badge.svg)](https://github.com/PereViader/ManualDi.Main/actions/workflows/run-tests.yml) [![Publish Nuget Package](https://github.com/PereViader/ManualDi.Main/actions/workflows/publish-nuget-package.yml/badge.svg)](https://github.com/PereViader/ManualDi.Main/actions/workflows/publish-nuget-package.yml)

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

# API

## Examples

To get to the action, please visit the automated tests of the project found on https://github.com/PereViader/ManualDi.Unity3d/tree/main/Assets/ManualDi/Tests

## Container creation

In order to create the container, the project offers a fluent Builder `ContainerBuilder`. Let's see how that would look like:

```
    IDiContainer container = new ContainerBuilder()
        .WithInstallDelegate(b => 
        {
            b.InstallSomeFunctionality();
        })
        .WithInstaller(new SomeOtherInstaller());
        .Build();
```

Let's analize this snippet:

- Declare the container variable of type `IDiContainer` called container.
- Create the `ContainerBuilder`
- Use the `WithInstallDelegate` function to start binding data to the container
- Call the static extension method `InstallSomeFunctionality` which will bind services to the container
- Use the `WithInstaller` function to bind an installer of type `SomeOtherInstaller`. This does the same as the extension method, but with an object instead of a static function call
- Actually build the container with the contents we gave to the Builder 


## Binding services

### Binding

Let's understand now how to bind anything to the container. 
For startes, you may only bind to the container during it's creation.
This is a syncronous process.

Binding data to the container is performed on the type `IDiContainerBindings`. This type exposes a dictionary with all the bindings performed on it and a method to add a new binding to it.
```
public interface IDiContainerBindings
{
    Dictionary<Type, List<ITypeBinding>> TypeBindings { get; }

    void AddBinding<T>(ITypeBinding<T> typeBinding);
}
```

Although it only has these methods, the type is ment to be used through extension methods instead. Please only use these if you understand how the container works.

In order to bind to the container, we have 3 `Bind` methods. We will concentrate on only 2 as they will be the most used ones.

```
ITypeBinding<T, T> Bind<T>(this IDiContainerBindings diContainerBindings)
ITypeBinding<T, Y> Bind<T, Y>(this IDiContainerBindings diContainerBindings)
```

The first one is meant to be used to bind an instance of some type T and expose the same type T as the interface type when resolving
The second one is meant to be used to bind an instance of some type Y but expose a different type T as the interface when resolving


Examples:

Binding the instance and exposed interface as the same one
```
b.Bind<SomeType>()
c.Resolve<SomeType>() // Success
```

Binding the instance and the exposed interface as different types

```
b.Bind<ISomeType, SomeType>()
c.Resolve<SomeType>() // Runtime error
c.Resolve<ISomeType>() // Success
```


### Resolving

Before we define how to actually add the data to the container, it will be useful to understand how we will get the data from the container.

We can get data from the container in two ways

#### Resolve

We get a single registered instance from the container

```
SomeService service = container.Resolve<SomeService>();
```

#### ResolveAll

If we registered multiple instances of the same type to the container this method will return all of them.

```
List<SomeService> services = container.ResolveAll<SomeService>();
```


### Populating the bindings

We have seen on the previous section how to start binding, but we've not actually bound anything.
There are several extension methods for the `ITypeBinding<T, Y>` that will allow you to actually make the binding do something.
Although there is nothing preventing you from calling these methods in another order, the convention this library recommends is to call them in this order.

```
Bind<T>()
    .[Single|Transient]
    .From[Instance|Method|Container|ContainerAll]
    .Inject
    .Initialize
    .RegisterDispose
    .WithMetadata
    .[Lazy|NonLazy]
```

We will now go over each one of them

### Scope

The scope of a binding defines under what circumstances a new instance defined by the binding is returned.
The container currently supports 2 of the 3 possible (usual) scope types.

#### Single

Similar to the dreaded `Singleton` but not globally accessible. The container will generate a single instance of the type and always return the same when asked to resolve it

#### Transient

The container will generate and return a new instance of the type (as defined by the creation strategy explained in the next section) when asked to resolve it 


### From

The creation strategy for the binding

#### Instance

An instance of the type is provided to the container as is. The container will then return it.
If this is used in conjunction with the transient scope, even though one would expect to get different instances, the container will return the same instance because the creation strategy is to always return the same instance.

Example:

```
b.Bind<T>().FromInstance(new T())
```

#### Method

A delegate to creates instances of the type is provided to the container.
The delegate provides the fully resolved container as a parameter in order to be able to inject services to the type on the constructor.

```
b.Bind<T>().FromMethod((c) => new T(
    c.Resolve<SomeService>()
    ))
```

#### Container

Useful for exposing other interfaces of types on the container

```
b.Bind<int>().FromInstance(1);
b.Bind<object, int>().FromContainer();

...

System.Console.WriteLine(c.Resolve<object>()); // Outputs "1"
```

In this snippet you can see that you register an integer with a value of 1 and then bind an object with the contents of the container for the integer.
Because we did this, when we request the object, the container will end up resolving the contents of the integer and return a 1.

This is just a shorthand for calling Resolve on the container for the type. 

```
b.Bind<object, int>().FromMethod(c => c.Resolve<int>());
```

#### Container All

Just like `FromContainer` binds all the instances to the container

```
b.Bind<int>().FromInstance(1);
b.Bind<int>().FromInstance(5);
b.Bind<List<object>, List<int>>().FromContainerAll();

...

foreach(var value in container.Resolve<List<object>>())
{
    System.Console.WriteLine(value); // Outputs "1" and then "5"
}
```

As seen before this is just a shorthand for ResolveAll

```
b.Bind<List<object>, List<int>>().FromMethod(c => c.ResolveAll<int>());
```


### Inject

There might be situations where it is not possible to inject everything for some type at the time of its creation. This might be due to many reasons.
When this happens, the Inject method allows injecting after the object has been constructed.
The object is not injected right after constructing it however, but it is added to a queue for it to be injected later.
This is done like this in order to allow all the dependant services to be also created and injected in the correct order.
The inject method will be called once for every new type, depending on the scope.

Example:


Service A: Depends on B
Service B: Depends on A


Naive solution that won't work

```
b.Bind<A>().FromMethod(c => new A(c.Resolve<B>()));
b.Bind<B>().FromMethod(c => new B(c.Resolve<A>()));

...

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

```
b.Bind<A>().FromMethod(c => new A())).Inject((o,c) => o.B = c.Resolve<B>());
b.Bind<B>().FromMethod(c => new B(c.Resolve<A>()));

...

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


#### Initialize

Similar to the Inject method, the initialize method defines a delegate to be called for newly constructed instances before they are resolved.
Also similarly to the Inject method, newly created instances are added to a queue and initialized.
The Initialize method will be called once for every new type depending on the scope.

```
b.Bind<A>().FromInstance(new A()).Initialize((o,c) => o.Init());

c.Resolve<A>()
```

- Start by getting service A
- The from method of A defines we return the instance provided
- The initialize method calls the Init method on A


#### RegisterDispose

Objects may implement the IDisposable interface or may need some other custom teardown logic. The RegisterDispose extension method allows defining some behaviour that will be run when the object is disposed of by the container.
The container will dispose when itself is disposed.


```
Given A does not implement IDisposable
and B implements IDisposable

b.Bind<A>().FromInstance(new A()).RegisterDispose((o,c) => o.DoCleanup);
b.Bind<B>().FromInstance(new B()).RegisterDispose();

c.Dispose(); // A and B disposed if they were created
```


#### With Metadata

These extension methods allow registering some key or key/values  that allow to filter elements when resolving them


```
Given A does not implement IDisposable
and B implements IDisposable

b.Bind<int>().FromInstance(1).WithMetadata("Potato");
b.Bind<int>().FromInstance(5).WithMetadata("Banana");

...

c.Resolve<int>(b => b.WhereMetadata("Potato")); // returns 1
c.Resolve<int>(b => b.WhereMetadata("Banana")); // returns 5
```


#### Laziness

##### Lazy

The object from method will not be called until the object is actually resolved

##### NonLazy

The object will be build at the same time with the container.
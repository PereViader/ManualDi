# ManualDi.Unity3d

Note: this extension to ManualDi.Main has still not been used in any production environment.

# Introduction

Welcome to the ManualDi.Unity3d package. This package contains all the functionality that the ManualDi project offers for the Unity3d game engine.
For information about the main the ManualDi functionality, please visit [here](https://github.com/PereViader/ManualDi.Main)

The unity 3d game engine is a great engine to create games with, but we believe it moves away developers from the land of code driven development and more into the land of mouse driven development.

With this project, we aim to decrease the complexity of your Unity3d project, by providing adapters and helpers for Unity3d functionality so they can be used with ease with the ManualDi.Main project.

The priciples this project is based on are:
 - Understandable: A new developer should be able to quickly understand how to use it
 - Familiar: Concepts used by the container should be the same as already as existing containers
 - Fast: The container should be able to resolve the object graph quickly and efficiently
 - Natural: As developers we should be creating value and not fighting Unity3d
 - Easy: Adding functionality should not require a high mental load
 - Pluggable:  Users of the container should be able to customize the container from the outside as they wish
 - Generic: The container should not assume the needs of the user and should allow for any strategy

 Unity follows a component based architecture. It also allows to serialize data on the components so it can be easily changed on the editor. This allows great flexibility to designers, but is not a very code centric approach.

(Will exapend this with more reasons why in the future)

Having this in mind, we want to take the good parts of Unity: designer firendly ui, graphics rendering, audio, physics, ... and improve it with the code centric approach that is a dependency injection container.

# Benefits of ManualDi in Unity3d

- Load Scenes with data and initialize them: When you load a scene you have to go through error prone steps to give the data it needs and initialize it appropriately. ManualDi provides a type safe interface to do it consistently.
- Load GameObjects with data and initialize them: When you instantiate a GameObject, you have to go through error prone steps to give the data it needs and initialize it appropriately. ManualDi provides a type safe interface to do it consistently.
- No more MonoBehaviour: Unity component pattern promotes coding using MonoBehavoiurs for everything. Some inversion of control can be accomplished, but the fact that everything has to be a Unity.Object makes everything unnecesarily harder.
- Testable: Once you don't code with MonoBehaviours for everything and follow appropiate dependency injection patterns, the code will become much more testable.
- Scalable: When you use dependency injection appropiately, you will be able to scale your game much easier. 

# Examples

To get to the action clone the project and see the [examples](Assets/ManualDi/Examples) and [automated tests](Assets/ManualDi/Tests).

# Installation

In order to use this package in your project you will have to: 

- Add the ManualDi.Main package on the UnityPackage manager using the following Git Url: `https://github.com/PereViader/ManualDi.Main#upm` 
- Add the ManualDi.Unity3d package on the UnityPackage manager using the following Git Url: `https://github.com/PereViader/ManualDi.Unity3d.git#upm`

See [documentation](https://docs.unity3d.com/Manual/upm-ui-giturl.html) for any doubts.

# Instantiate a GameObject

You can see this in action directly without going through all the steps [here in this example](Assets/ManualDi/Examples/Example1)

If you, for any reason, need to make some pure c# functionality, you can still use ManualDi.Main directly, see the main repo for more details on that. 
Otherwise we want to take the best parts of Unity's powerful and designer friendly interface and ManualDi's powerful and engineering friendly dependency injection container.
When we take both, and follow proper dependency injection patterns, we get powerful, scalable and designer friendly systems on the Unity3d game engine interface.  

The architecture can be thought of several systems that depend on other ones. If we didn't think of them like this, we would have to get the whole Context of the game inside our heads which would be difficult to accomplish to say the least. 

Imagine you want to instantiate a GameObject. Unity provides an easy way to accomplish this task.

```csharp
class GameObjectContext : MonoBehaviour
{
    void Speak() 
    {
        Debug.Log("Hi");
    }
}

class Example
{
    public GameObject gameObjectPrefab;

    void Run()
    {
        GameObject gameObjectInstance = GameObject.Instantiate(gameObjectPrefab);
        GameObjectContext contextInstance = gameObjectInstance.GetComponent<GameObjectContext>();
        contextInstance.Speak();
    }
}
```

Here we can see we have a serialized GameObject and when we run the example, a GameObject instance is created and we get it on a varaible.
However this does not tell us anything about the GameObject, we can only assume what the contents of this GameObject are.

We could try to the components of the gameObject `GetComponent`, but we would be assuming maybe too much.

This however is not a problem, because we can do this.

```csharp
class GameObjectContext : MonoBehaviour
{
    void Speak() 
    {
        Debug.Log("Hi");
    }
}

class Example
{
    public GameObjectContext contextPrefab;

    void Run()
    {
        GameObjectContext contextInstance = GameObject.Instantiate(contextPrefab);
        contextInstance.Speak();
    }
}
```

This can be considered an improvement over the previous solution because we know that the prefab we are serializing is defenetly of the type we want.

But what would happen if we needed to provide this GameObject with some data.

```csharp
class Data
{
    string Name { get; set; }
}

class GameObjectContext : MonoBehaviour
{
    Data data;

    void Inject(Data data)
    {
        this.data = data;
    }

    void Speak() 
    { 
        Debug.Log("Hi " + data.Name);
    }
}

class Example
{
    public GameObjectContext contextPrefab;

    void Run()
    {
        var data = new Data() { Name = "Charles" };
        GameObjectContext contextInstance = GameObject.Instantiate(contextPrefab);
        contextInstance.Inject(data);
        contextInstance.Speak();
    }
}
```

This solution may work, but we will have to remember to call Inject every before using the System.

This is where ManualDi and this Unity3d integration will start to help (but there is even more, maybe don't use it for just this next step).

ManualDi.Unity3d provides you with a MonoBehaviour base class `BaseContextEntryPoint<TData, TContext>`  you can use to define the context for GameObject system.

```csharp
class Data
{
    string Name { get; set; }
}

class GameObjectContextEntryPoint : BaseContextEntryPoint<Data, GameObjectContext>
{
    public GameObjectContext context;

    public override void Install(IDiContainerBindings bindings)
    {
        bindings.Bind<GameObjectContext>()
            .FromInstance(context)
            .Inject((o,c) => o.Inject(Data));
    }
}

class GameObjectContext : MonoBehaviour
{
    Data data;

    void Inject(Data data)
    {
        this.data = data;
    }

    void Speak() 
    { 
        Debug.Log("Hi " + data.Name);
    }
}

class Example
{
    public GameObjectContextEntryPoint entryPointPrefab;

    void Run()
    {
        var data = new Data() { Name = "Charles" };
        GameObjectContext contextInstance = GameObjectManualDi.Instantiate(entryPointPrefab, data, RootContextInitiator.Instance);
        contextInstance.Speak();
    }
}
```

Let's go step by step, because there is a lot to unpack here:

- We have added a new `MonoBehaviour` class to the GameObject we are instantiating `SystemEntryPoint`. This class has a base class of `BaseContextEntryPoint<Data, GameObjectContext>` this means that in order to instantiate this prefab, you have to provide it with some data of type `Data` and that the game object will be instantiated beeing represented by `System`.

- `GameObjectContextEntryPoint` has a `void Install(IDiContainerBindings bindings)` method. This is how the Container is configured with the object graph. This method MUST install `GameObjectContext` this is why the prefab serializes System as a public variable and binds it from an instance. Notice how we also configure the container to Inject the `Data` property to the `GameObjectContext` instance doing this `Inject((o,c) => o.Inject(Data));`

- When we want to instantiate a gameobject system using ManualDi we have `GameObjectManualDi` static class which will do the necessary instantiations. Notice the parameters: the system entry point, the data we have to provide it with and `RootContextInitiator.Instance` which means that system does not depend on anything else (we will give more details on this later).

After doing this, we can see that there is no way to instantiate the System without providing it with the data it needs and that once the `GameObjectManualDi.Instantiate` method returns, the system is 100% ready to work.


# Load a Scene

You can see this in action directly without going through all the steps [here in this example](Assets/ManualDi/Examples/Example2)

The api for this is very similar to the one for GameObjects so we will not go with as much detail as with the GameObject example. See the other one for the full rationale.

Imagine you want to load a Scene on unity. You might load them directly through Unity SceneManagement functionality directly or through some indirect means that might help you manage your scenes better.

We can't make a generic solution to the custom possible Scene loading solution you use, so we will provide with a generic Unity API solution you can integrate into your possible custom project.

Just like with GameObjects, scenes will represent a part of your system. Each scene probably representing a context in your game.

Let's first see how is a scene loaded in unity and an example of how to provide some data to it.

```csharp
class Data
{
    string Name { get; set; }
}

class SceneContext : MonoBehaviour
{
    Data data;

    void Inject(Data data)
    {
        this.data = data;
    }

    void Speak()
    {
        Debug.Log("Hi " + data.Name);
    }
}

class Example
{
    IEnumerator Run()
    {
        var data = new Data() { Name = "Charles" };
        
        yield return SceneManager.LoadSceneAsync("TheScene", LoadSceneMode.Additive);

        Scene scene = SceneManager.GetSceneByName("Example2Context");

        SceneContext sceneContext = scene.GetRootGameObjects().Find<SceneContext>()

        sceneContext.Inject(data);

        sceneContext.Speak();
    }
}

```

Let's break this down:

- We have a coroutine `Run` on the `Example` class
- This coroutine creates some data object we will provide later
- Loads a scene using the Unity SceneManagement API
- Gets a reference to that scene
- Finds the `SceneContext` class
- Injects it with the data it needs
- Calls some method with the functionality we desire


This can be problematic however:

- We have to find the `SceneContext` manually
- We have to inject the data to the `SceneContext` manually

Having to get the scene manually after loading it is not ideal, but that is how the Unity API is made. Take care also because you can load the same scene with `LoadSceneMode.Additive` so you would then need to take good care when getting the reference to it otherwise you would be getting a reference to the wrong `SceneContext`.

How does the ManualDi integration improve this.

```csharp
class Data
{
    string Name { get; set; }
}

class SceneContextEntryPoint : BaseContextEntryPoint<Data, SceneContext>
{
    public SceneContext sceneContext;

    public override void Install(IDiContainerBindings bindings)
    {
        bindings.Bind<SceneContext>()
            .FromInstance(sceneContext)
            .Inject((o,c) => o.Inject(Data));
    }
}

class SceneContext : MonoBehaviour
{
    Data data;

    void Inject(Data data)
    {
        this.data = data;
    }

    void Speak()
    {
        Debug.Log("Hi " + data.Name);
    }
}

class Example
{
    IEnumerator Run()
    {
        var data = new Data() { Name = "Charles" };
        
        yield return SceneManager.LoadSceneAsync("TheScene", LoadSceneMode.Additive);

        Scene scene = SceneManager.GetSceneByName("TheScene");

        SceneContext context = SceneManualDi.Initiate<SceneContextEntryPoint, Data, SceneContext>(
                scene,
                data,
                RootContextInitiator.Instance
                );

        sceneContext.Speak();
    }
}

```

Very similar to how do it with GameObject instantiation, we have a `MonoBehaviour` entry point `SceneContextEntryPoint` based on `BaseContextEntryPoint<Data, SceneContext>` which when we set it up on the Install method, we bind the `SceneContext` to the instance we have serialized in it.

Using the `SceneManualDi.Initiate` method, we are then able to take the scene we have just loaded and gotten a reference to and pointing to the appropiate entry point type `SceneContextEntryPoint`, which we have to implicitly type as there is no way to do it statically, we then can provide the data it needs and just like the gameobject case we are saying this is going to be a root context (we will expand this further in another section).

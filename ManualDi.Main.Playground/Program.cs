﻿using ManualDi.Main;

Console.WriteLine("ManualDi Playground");

//Missing feature: a nullable argument should use TryResolve instead of Resolve

var container = new DiContainerBuilder().Install(b =>
{
    b.Bind<Public>().Default();
    b.Bind<Internal>().Default();
    b.Bind<Internal2>().Default();
    b.Bind<Static.PublicNested>().Default();
    b.Bind<Static.InternalNested>().Default();
    b.Bind<Public>().Default(new Public());
#pragma warning disable CS0612 // Type or member is obsolete
    b.Bind<Obsolete>().Default();
#pragma warning restore CS0612 // Type or member is obsolete
}).Build();

public class Public { }
internal class Internal { }
class Internal2 { }
class Generic<T> { }

public class InternalInitialize
{
    internal void Initialize() { }
}

public class PublicInitialize
{
    public void Initialize() { }
    internal void Inject() { }
}

class StaticInitialize
{
    public static void Initialize() { }
}

class InternalInject
{
    internal void Inject() { }
}

class PublicInject
{
    public void Inject() { }
}

class StaticInject
{
    public static void Inject() { }
}

class ConstructorWithGenericArgument
{
    public ConstructorWithGenericArgument(Func<int> func) { }
}

class InjectPropertyAndMethod
{
    [Inject] public object Object { get; set; } = default!;

    public void Inject(object obj)
    {
    }
}

public class InjectPropertyClass
{
    [Inject] public object Object1 { get; set; } = default!;
    [Inject] internal object Object2 { get; set; } = default!;
    [Inject] private object Object3 { get; set; } = default!;
    [Inject] protected object Object4 { get; set; } = default!;
    [Inject] public object Object5 { get; internal set; } = default!;
    [Inject] public object Object6 { get; private set; } = default!;
    [Inject] public object Object7 { get; protected set; } = default!;
    [Inject] public static object Object8 { get; set; } = default!;
}

[Obsolete]
class Obsolete
{
    public void Inject() { }
    public void Initialize() { }
}

static class Static 
{
    public class PublicNested { }
    internal class InternalNested { }
    private class PrivateNested { }
    static class StaticNested { }
}


namespace UnityEngine
{
    public class Object
    {
    }

    public class MonoBeheviour : Object
    {
        [Inject] public Object Something { get; set; } = default!;
        
        public void Initialize() { }
    }
}
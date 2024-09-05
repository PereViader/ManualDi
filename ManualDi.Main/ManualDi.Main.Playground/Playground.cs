using ManualDi.Main;
using UnityEngine;

Console.WriteLine("ManualDi Playground");

var container = new DiContainerBindings().Install(b =>
{
}).Build();

public class Public { }
internal class Internal { }

internal class InternalPrivateConstructor
{
    private InternalPrivateConstructor() { }
}

public class PublicAndPrivateConstructor
{
    public PublicAndPrivateConstructor(int x) : this() { }
    private PublicAndPrivateConstructor() { }
}

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

public class InjectReferencePropertyClass
{
    [Inject] public object Object0 { get; set; } = default!;
    [Inject] internal object Object2 { get; set; } = default!;
    [Inject] private object Object3 { get; set; } = default!;
    [Inject] protected object Object4 { get; set; } = default!;
    
    [Inject] public object Object5 { get; internal set; } = default!;
    [Inject] public object Object6 { get; private set; } = default!;
    [Inject] public object Object7 { get; protected set; } = default!;
    [Inject] public static object Object8 { get; set; } = default!;
    
}


public class InjectValueNullablePropertyClass
{
    [Inject] public int Int1 { get; set; } = default!;
    [Inject] internal int Int2 { get; set; } = default!;
    [Inject] private int Int3 { get; set; } = default!;
    [Inject] protected int Int4 { get; set; } = default!;
    [Inject] public int? Int5 { get; set; } = default!;
    [Inject] internal int? Int6 { get; set; } = default!;
    [Inject] private int? Int7 { get; set; } = default!;
    [Inject] protected int? Int8 { get; set; } = default!;
}

[Obsolete]
class Obsolete
{
    public void Inject() { }
    public void Initialize() { }
}

public class NullableDependency
{
    [Inject] public object? Object { get; set; }
    [Inject] public int? Int { get; set; }
    [Inject] public Nullable<int> Int3 { get; set; }
    public NullableDependency(object? obj, Nullable<int> value) { }
    public void Inject(object? obj, int? value) { }
    public void Initialize(object? obj, int? value) { }
}

static class Static 
{
    public class PublicNested { }
    internal class InternalNested { }
    private class PrivateNested { }
    static class StaticNested { }
}

class ListInject 
{
    public ListInject(List<object> objects) { }
    
    [Inject] public IEnumerable<object> IEnumerableObject { get; set; } = default!;
    [Inject] public IEnumerable<int> IEnumerableInt { get; set; } = default!;
    [Inject] public IReadOnlyList<object> IReadOnlyListObject { get; set; } = default!;
    [Inject] public IReadOnlyList<int> IReadOnlyListInt { get; set; } = default!;
    [Inject] public IList<object> IListObject { get; set; } = default!;
    [Inject] public IList<int> IListInt { get; set; } = default!;
    [Inject] public List<object> ListObject { get; set; } = default!;
    [Inject] public List<int> ListInt { get; set; } = default!;
    
    public void Inject(List<object> objects) { }
    public void Initialize(List<object> objects) { }
}

class LazyDependencies
{
    [Inject] public Lazy<object> Object { get; set; } = default!;
    [Inject] public Lazy<object?> NullableObject { get; set; } = default!;
    [Inject] public Lazy<int> Value { get; set; } = default!;
    [Inject] public Lazy<int?> NullableValue { get; set; } = default!;
    [Inject] public Lazy<Lazy<Lazy<int>>> RecursiveLazy { get; set; } = default!;
    [Inject] public Lazy<List<int>> LazyList { get; set; } = default!;
    
    public void Inject(Lazy<object> obj, Lazy<object?> nullObj, Lazy<int> val, Lazy<int?> nullVal) { }
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
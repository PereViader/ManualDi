﻿using ManualDi.Main;
using System;
using System.Collections.Generic;

var container = new DiContainerBindings().Install(b =>
{
}).Build();

namespace SomeNamespace.Subnamespace
{
    public class Public
    {
    }

    internal class Internal
    {
    }


    internal class InternalPrivateConstructor
    {
        private InternalPrivateConstructor()
        {
        }
    }

    public class PublicAndPrivateConstructor
    {
        public PublicAndPrivateConstructor(int x) : this()
        {
        }

        private PublicAndPrivateConstructor()
        {
        }
    }

    class Internal2
    {
    }

    class Generic<T>
    {
    }

    public class InternalInitialize
    {
        internal void Initialize()
        {
        }
    }

    public abstract class Abstract
    {
        public Abstract() { }
    }

    public class SomeDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }

    public class PublicInitialize
    {
        public void Initialize()
        {
        }

        internal void Inject()
        {
        }
    }

    class StaticInitialize
    {
        public static void Initialize()
        {
        }
    }

    class InternalInject
    {
        internal void Inject()
        {
        }
    }

    class PublicInject
    {
        public void Inject()
        {
        }
    }

    class StaticInject
    {
        public static void Inject()
        {
        }
    }

    class ConstructorWithGenericArgument
    {
        public ConstructorWithGenericArgument(Func<int> func)
        {
        }
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
        public void Inject()
        {
        }

        public void Initialize()
        {
        }
    }

    public class NullableDependency
    {
        [Inject] public object? Object { get; set; }
        [Inject] public int? Int { get; set; }
        [Inject] public Nullable<int> Int3 { get; set; }

        public NullableDependency(object? obj, Nullable<int> value)
        {
        }

        public void Inject(object? obj, int? value)
        {
        }

        public void Initialize()
        {
        }
    }

    static class Static
    {
        public class PublicNested
        {
        }

        internal class InternalNested
        {
        }

        private class PrivateNested
        {
        }

        static class StaticNested
        {
        }
    }

    class ListInject
    {
        public ListInject(List<object> objects)
        {
        }

        [Inject] public IEnumerable<object> IEnumerableObject { get; set; } = default!;
        [Inject] public IEnumerable<int> IEnumerableInt { get; set; } = default!;
        [Inject] public IReadOnlyList<object> IReadOnlyListObject { get; set; } = default!;
        [Inject] public IReadOnlyList<int> IReadOnlyListInt { get; set; } = default!;
        [Inject] public IList<object> IListObject { get; set; } = default!;
        [Inject] public IList<int> IListInt { get; set; } = default!;
        [Inject] public List<object> ListObject { get; set; } = default!;
        [Inject] public List<int> ListInt { get; set; } = default!;
        [Inject] public IReadOnlyCollection<object> IReadOnlyCollectionObject { get; set; } = default!;
        [Inject] public IReadOnlyCollection<int> IReadOnlyCollectionInt { get; set; } = default!;
        [Inject] public ICollection<object> ICollectionObject { get; set; } = default!;
        [Inject] public ICollection<int> ICollectionInt { get; set; } = default!;

        [Inject("A")] public IEnumerable<object> IEnumerableObjectId { get; set; } = default!;
        [Inject("A")] public IEnumerable<int> IEnumerableIntId { get; set; } = default!;
        [Inject("A")] public IReadOnlyList<object> IReadOnlyListObjectId { get; set; } = default!;
        [Inject("A")] public IReadOnlyList<int> IReadOnlyListIntId { get; set; } = default!;
        [Inject("A")] public IList<object> IListObjectId { get; set; } = default!;
        [Inject("A")] public IList<int> IListIntId { get; set; } = default!;
        [Inject("A")] public List<object> ListObjectId { get; set; } = default!;
        [Inject("A")] public List<int> ListIntId { get; set; } = default!;
        [Inject("A")] public IReadOnlyCollection<object> IReadOnlyCollectionObjectId { get; set; } = default!;
        [Inject("A")] public IReadOnlyCollection<int> IReadOnlyCollectionIntId { get; set; } = default!;
        [Inject("A")] public ICollection<object> ICollectionObjectId { get; set; } = default!;
        [Inject("A")] public ICollection<int> ICollectionIntId { get; set; } = default!;
        
        [Inject] public IEnumerable<object>? IEnumerableObjectNullable { get; set; } = default!;
        [Inject] public IEnumerable<int>? IEnumerableIntNullable { get; set; } = default!;
        [Inject] public IReadOnlyList<object>? IReadOnlyListObjectNullable { get; set; } = default!;
        [Inject] public IReadOnlyList<int>? IReadOnlyListIntNullable { get; set; } = default!;
        [Inject] public IList<object>? IListObjectNullable { get; set; } = default!;
        [Inject] public IList<int>? IListIntNullable { get; set; } = default!;
        [Inject] public List<object>? ListObjectNullable { get; set; } = default!;
        [Inject] public List<int>? ListIntNullable { get; set; } = default!;
        [Inject] public IReadOnlyCollection<object>? IReadOnlyCollectionObjectNullable { get; set; } = default!;
        [Inject] public IReadOnlyCollection<int>? IReadOnlyCollectionIntNullable { get; set; } = default!;
        [Inject] public ICollection<object>? ICollectionObjectNullable { get; set; } = default!;
        [Inject] public ICollection<int>? ICollectionIntNullable { get; set; } = default!;
        

        [Inject("A")] public IEnumerable<object>? IEnumerableObjectNullableId { get; set; } = default!;
        [Inject("A")] public IEnumerable<int>? IEnumerableIntNullableId { get; set; } = default!;
        [Inject("A")] public IReadOnlyList<object>? IReadOnlyListObjectNullableId { get; set; } = default!;
        [Inject("A")] public IReadOnlyList<int>? IReadOnlyListIntNullableId { get; set; } = default!;
        [Inject("A")] public IList<object>? IListObjectNullableId { get; set; } = default!;
        [Inject("A")] public IList<int>? IListIntNullableId { get; set; } = default!;
        [Inject("A")] public List<object>? ListObjectNullableId { get; set; } = default!;
        [Inject("A")] public List<int>? ListIntNullableId { get; set; } = default!;
        [Inject("A")] public IReadOnlyCollection<object>? IReadOnlyCollectionObjectNullableId { get; set; } = default!;
        [Inject("A")] public IReadOnlyCollection<int>? IReadOnlyCollectionIntNullableId { get; set; } = default!;
        [Inject("A")] public ICollection<object>? ICollectionObjectNullableId { get; set; } = default!;
        [Inject("A")] public ICollection<int>? ICollectionIntNullableId { get; set; } = default!;
        
        [Inject("A")] public IEnumerable<object?>? IEnumerableObjectNullableNullableId { get; set; } = default!;
        [Inject("A")] public IEnumerable<int?>? IEnumerableIntNullableNullableId { get; set; } = default!;
        [Inject("A")] public IReadOnlyList<object?>? IReadOnlyListObjectNullableNullableId { get; set; } = default!;
        [Inject("A")] public IReadOnlyList<int?>? IReadOnlyListIntNullableNullableId { get; set; } = default!;
        [Inject("A")] public IList<object?>? IListObjectNullableNullableId { get; set; } = default!;
        [Inject("A")] public IList<int?>? IListIntNullableNullableId { get; set; } = default!;
        [Inject("A")] public List<object?>? ListObjectNullableNullableId { get; set; } = default!;
        [Inject("A")] public List<int?>? ListIntNullableNullableId { get; set; } = default!;
        [Inject("A")] public IReadOnlyCollection<object?>? IReadOnlyCollectionObjectNullableNullableId { get; set; } = default!;
        [Inject("A")] public IReadOnlyCollection<int?>? IReadOnlyCollectionIntNullableNullableId { get; set; } = default!;
        [Inject("A")] public ICollection<object?>? ICollectionObjectNullableNullableId { get; set; } = default!;
        [Inject("A")] public ICollection<int?>? ICollectionIntNullableNullableId { get; set; } = default!;
        
        public void Inject(List<object> objects)
        {
        }

        public void Initialize()
        {
        }
    }

    class LazyDependencies
    {
        [Inject] public Lazy<object> Object { get; set; } = default!;
        [Inject] public Lazy<object?> NullableObject { get; set; } = default!;
        [Inject] public Lazy<int> Value { get; set; } = default!;
        [Inject] public Lazy<int?> NullableValue { get; set; } = default!;
        [Inject] public Lazy<Lazy<Lazy<int>>> RecursiveLazy { get; set; } = default!;
        [Inject] public Lazy<List<int>> LazyList { get; set; } = default!;
        
        [Inject] public Lazy<object>? ObjectNullable { get; set; } = default!;
        [Inject] public Lazy<object?>? NullableObjectNullable { get; set; } = default!;
        [Inject] public Lazy<int>? ValueNullable { get; set; } = default!;
        [Inject] public Lazy<int?>? NullableValueNullable { get; set; } = default!;
        [Inject] public Lazy<Lazy<Lazy<int>>>? RecursiveLazyNullable { get; set; } = default!;
        [Inject] public Lazy<List<int>>? LazyListNullable { get; set; } = default!;

        public void Inject(Lazy<object> obj, Lazy<object?> nullObj, Lazy<int> val, Lazy<int?> nullVal)
        {
        }
    }

    class InjectIdAttribute
    {
        private const string Something = "A";

        [Inject(Something)] public int HelloIdValue { get; set; }
        [Inject(Something)] public Lazy<int> LazyInt { get; set; } = default!;

        public void Inject(
            [Inject("Potato")] object potatoValue,
            [Inject("Banana")] float bananaValue,
            [Inject("P")] Lazy<int> lazyInt,
            object value)
        {
        }

        public void Initialize()
        {
        }
    }
}

class MultipleOfEach
{
    internal MultipleOfEach(object o) {}
    public MultipleOfEach(object o, object o2) {}  // <- it should use this one
    
    internal void Inject() {}
    public void Inject(object o) {} // <- it should use this one
    
    internal void Initialize() {} // <- it should use this one
}

class InjectContainer
{
    public InjectContainer(IDiContainer c) {} // The container should be provided as is
}

partial class Partial
{
    public Partial(object o) {}
    void Inject(object o) {}
}

partial class Partial
{
    public void Initialize() {}
}

namespace UnityEngine
{
    public class Object
    {
    }

    public class MonoBeheviour : Object
    {
        [Inject] public object Something { get; set; } = default!;
        
        public void Initialize() { }
    }
}
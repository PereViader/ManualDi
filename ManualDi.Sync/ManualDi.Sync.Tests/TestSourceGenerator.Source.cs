using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ManualDi.Sync;

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
        public Task InitailizeAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
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

    public class InjectReferencePropertyClass
    {
        public InjectReferencePropertyClass(
            object referece
        )
        {
        }
    }


    public class InjectValueNullablePropertyClass
    {
        public InjectValueNullablePropertyClass(
            int value,
            int? nullableValue
        )
        {
        }
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
    
    public class ArrayOfNullablesDependency
    {
        public ArrayOfNullablesDependency(object?[] objects, int?[] values)
        {
        }

        public void Inject(object?[] objects, int?[] values)
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
        public ListInject(
            object[] arrayObject,
            int[] arrayInt,
            object[]? arrayObjectNullable,
            int[]? arrayIntNullable,
            object?[] arrayNullableObject,
            int?[] arrayNullableInt,
            object?[]? arrayNullableObjectNullable,
            int?[]? arrayNullableIntNullable,
            IEnumerable<object> IEnumerableObject,
            IEnumerable<int> IEnumerableInt,
            IReadOnlyList<object> IReadOnlyListObject,
            IReadOnlyList<int> IReadOnlyListInt,
            IList<object> IListObject,
            IList<int> IListInt,
            List<object> ListObject,
            List<int> ListInt,
            IReadOnlyCollection<object> IReadOnlyCollectionObject,
            IReadOnlyCollection<int> IReadOnlyCollectionInt,
            ICollection<object> ICollectionObject,
            ICollection<int> ICollectionInt,

            [Id("A")] IEnumerable<object> IEnumerableObjectId,
            [Id("A")] IEnumerable<int> IEnumerableIntId,
            [Id("A")] IReadOnlyList<object> IReadOnlyListObjectId, 
            [Id("A")] IReadOnlyList<int> IReadOnlyListIntId,
            [Id("A")] IList<object> IListObjectId,
            [Id("A")] IList<int> IListIntId,
            [Id("A")] List<object> ListObjectId, 
            [Id("A")] List<int> ListIntId,
            [Id("A")] IReadOnlyCollection<object> IReadOnlyCollectionObjectId,
            [Id("A")] IReadOnlyCollection<int> IReadOnlyCollectionIntId,
            [Id("A")] ICollection<object> ICollectionObjectId,
            [Id("A")] ICollection<int> ICollectionIntId,
            
            IEnumerable<object>? IEnumerableObjectNullable,
            IEnumerable<int>? IEnumerableIntNullable,
            IReadOnlyList<object>? IReadOnlyListObjectNullable, 
            IReadOnlyList<int>? IReadOnlyListIntNullable,
            IList<object>? IListObjectNullable,
            IList<int>? IListIntNullable,
            List<object>? ListObjectNullable, 
            List<int>? ListIntNullable,
            IReadOnlyCollection<object>? IReadOnlyCollectionObjectNullable, 
            IReadOnlyCollection<int>? IReadOnlyCollectionIntNullable,
            ICollection<object>? ICollectionObjectNullable,
            ICollection<int>? ICollectionIntNullable,
            

            [Id("A")] IEnumerable<object>? IEnumerableObjectNullableId,
            [Id("A")] IEnumerable<int>? IEnumerableIntNullableId,
            [Id("A")] IReadOnlyList<object>? IReadOnlyListObjectNullableId, 
            [Id("A")] IReadOnlyList<int>? IReadOnlyListIntNullableId,
            [Id("A")] IList<object>? IListObjectNullableId,
            [Id("A")] IList<int>? IListIntNullableId,
            [Id("A")] List<object>? ListObjectNullableId, 
            [Id("A")] List<int>? ListIntNullableId,
            [Id("A")] IReadOnlyCollection<object>? IReadOnlyCollectionObjectNullableId, 
            [Id("A")] IReadOnlyCollection<int>? IReadOnlyCollectionIntNullableId,
            [Id("A")] ICollection<object>? ICollectionObjectNullableId,
            [Id("A")] ICollection<int>? ICollectionIntNullableId,
            
            [Id("A")] IEnumerable<object?>? IEnumerableObjectNullableNullableId,
            [Id("A")] IEnumerable<int?>? IEnumerableIntNullableNullableId,
            [Id("A")] IReadOnlyList<object?>? IReadOnlyListObjectNullableNullableId, 
            [Id("A")] IReadOnlyList<int?>? IReadOnlyListIntNullableNullableId,
            [Id("A")] IList<object?>? IListObjectNullableNullableId,
            [Id("A")] IList<int?>? IListIntNullableNullableId,
            [Id("A")] List<object?>? ListObjectNullableNullableId, 
            [Id("A")] List<int?>? ListIntNullableNullableId,
            [Id("A")] IReadOnlyCollection<object?>? IReadOnlyCollectionObjectNullableNullableId, 
            [Id("A")] IReadOnlyCollection<int?>? IReadOnlyCollectionIntNullableNullableId,
            [Id("A")] ICollection<object?>? ICollectionObjectNullableNullableId,
            [Id("A")] ICollection<int?>? ICollectionIntNullableNullableId
            )
        {
        }

        
        
        public void Inject(List<object> objects)
        {
        }

        public void Initialize()
        {
        }
    }

    class InjectIdAttribute
    {
        private const string Something = "A";

        public void Inject(
            [Id("Potato")] object potatoValue,
            [Id("Banana")] float bananaValue,
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
    public InjectContainer(IDiContainer c, CancellationToken ct) {} // The container should be provided as is
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

class InitializeAsyncCheck
{
    public Task InitializeAsync(CancellationToken ct) { return Task.CompletedTask; }
}

namespace UnityEngine
{
    public class Object
    {
    }

    public class MonoBeheviour : Object
    {
    }

    public class SomeMonoBehaviour : MonoBeheviour
    {
        public void Construct(int x) { }
        public void Inject(object o) { }
        public Task InitializeAsync(CancellationToken ct) { return Task.CompletedTask; }

        public void Initialize() { }
    }
}

public class MapNavMesh
{
    public class BakeData
    {
    }
}

[Obsolete("Use MapNavMesh.BakeData")]
public class MapNavMeshBakeData : MapNavMesh.BakeData { }

public class TestOutParam
{
    public TestOutParam(out string test)
    {
        test = "test";
    }
    
    public void Inject(out string test)
    {
        test = "test";
    }
}
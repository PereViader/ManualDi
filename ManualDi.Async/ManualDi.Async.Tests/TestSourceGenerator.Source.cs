using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ManualDi.Async;

namespace SomeNamespace.Subnamespace
{
    [ManualDi]
    public class Public
    {
    }

    [ManualDi]
    internal class Internal
    {
    }


    [ManualDi]
    internal class InternalPrivateConstructor
    {
        private InternalPrivateConstructor()
        {
        }
    }

    [ManualDi]
    public class PublicAndPrivateConstructor
    {
        public PublicAndPrivateConstructor(int x) : this()
        {
        }

        private PublicAndPrivateConstructor()
        {
        }
    }

    [ManualDi]
    class Internal2
    {
    }

    [ManualDi]
    class Generic<T>
    {
    }

    [ManualDi]
    sealed class SealedGeneric<T>
    {
    }

    [ManualDi]
    class Base
    {
    }

    [ManualDi]
    sealed class SelaledChildGeneric<T> : Base
    {
    }

    [ManualDi]
    sealed class SelaledChildGeneric<T, Y> : Base
    {
    }

    [ManualDi]
    public class InternalInitialize
    {
        internal void Initialize()
        {
        }
    }

    [ManualDi]
    public class TaskInitialize
    {
        internal Task Initialize()
        {
            return Task.CompletedTask;
        }
    }

    [ManualDi]
    public abstract class Abstract
    {
        public Abstract() { }
    }

    [ManualDi]
    public class SomeDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }

    [ManualDi]
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

    [ManualDi]
    class StaticInitialize
    {
        public static void Initialize()
        {
        }
    }

    [ManualDi]
    class InternalInject
    {
        internal void Inject()
        {
        }
    }

    [ManualDi]
    class PublicInject
    {
        public void Inject()
        {
        }
    }

    [ManualDi]
    class StaticInject
    {
        public static void Inject()
        {
        }
    }

    [ManualDi]
    class ConstructorWithGenericArgument
    {
        public ConstructorWithGenericArgument(Func<int> func)
        {
        }
    }

    [ManualDi]
    public class InjectReferencePropertyClass
    {
        public InjectReferencePropertyClass(
            object referece
        )
        {
        }
    }


    [ManualDi]
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
    [ManualDi]
    class Obsolete
    {
        public void Inject()
        {
        }

        public void Initialize()
        {
        }
    }

    [ManualDi]
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

    [ManualDi]
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
        [ManualDi]
        public class PublicNested
        {
        }

        [ManualDi]
        internal class InternalNested
        {
        }

        [ManualDi]
        private class PrivateNested
        {
        }

        [ManualDi]
        static class StaticNested
        {
        }
    }

    [ManualDi]
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

    [ManualDi]
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

    [ManualDi]
    public sealed class SealedClass
    {
        public void Inject() { }
    }
}

[ManualDi]
class MultipleOfEach
{
    internal MultipleOfEach(object o) { }
    public MultipleOfEach(object o, object o2) { }  // <- it should use this one

    internal void Inject() { }
    public void Inject(object o) { } // <- it should use this one

    internal void Initialize() { } // <- it should use this one
}

[ManualDi]
class InjectContainer
{
    public InjectContainer(IDiContainer c, CancellationToken ct) { } // The container should be provided as is
}

[ManualDi]
partial class Partial
{
    public Partial(object o) { }
    void Inject(object o) { }
}

partial class Partial
{
    public void Initialize() { }
}

[ManualDi]
class InitializeAsyncCheck
{
    public Task InitializeAsync(CancellationToken ct) { return Task.CompletedTask; }
}

namespace UnityEngine
{
    [ManualDi]
    public class Object
    {
    }

    [ManualDi]
    public class MonoBeheviour : Object
    {
    }

    [ManualDi]
    public class SomeMonoBehaviour : MonoBeheviour
    {
        public void Inject(object o, [CyclicDependency] int i) { }
        public Task InitializeAsync(CancellationToken ct) { return Task.CompletedTask; }

        public void Initialize() { }
    }
}

[ManualDi]
public class MapNavMesh
{
    [ManualDi]
    public class BakeData
    {
    }
}

[Obsolete("Use MapNavMesh.BakeData")]
[ManualDi]
public class MapNavMeshBakeData : MapNavMesh.BakeData { }

[ManualDi]
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

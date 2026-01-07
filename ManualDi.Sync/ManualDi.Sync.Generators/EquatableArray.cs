using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ManualDi.Sync.Generators
{
    internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T>
        where T : IEquatable<T>
    {
        private readonly T[]? _array;

        public EquatableArray(T[] array)
        {
            _array = array;
        }

        public T this[int index] => (_array ?? Array.Empty<T>())[index];
        public int Count => _array?.Length ?? 0;
        public bool HasValue => _array is not null;

        public bool Equals(EquatableArray<T> other)
        {
            return ReferenceEquals(_array, other._array) ||
                   ((_array is not null && other._array is not null) &&
                    _array.SequenceEqual(other._array));
        }

        public override bool Equals(object? obj)
        {
            return obj is EquatableArray<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            if (_array is null)
            {
                return 0;
            }

            int hash = 17;
            foreach (var item in _array)
            {
                hash = hash * 23 + (item?.GetHashCode() ?? 0);
            }
            return hash;
        }

        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)(_array ?? Array.Empty<T>())).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static implicit operator EquatableArray<T>(T[] array) => new EquatableArray<T>(array);
        public static implicit operator EquatableArray<T>(List<T> list) => new EquatableArray<T>(list.ToArray());
    }
}

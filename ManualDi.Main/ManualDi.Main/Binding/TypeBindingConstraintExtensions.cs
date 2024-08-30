using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    public static class TypeBindingConstraintExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBinding WithMetadata<TBinding, TMetadata>(this TBinding typeBinding, object key, TMetadata value)
            where TBinding : TypeBinding
        {
            typeBinding.Metadata ??= new Dictionary<object, object>();
            typeBinding.Metadata.Add(key, value!);
            return typeBinding;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBinding WithMetadata<TBinding>(this TBinding typeBinding, object key)
            where TBinding : TypeBinding
        {
            typeBinding.Metadata ??= new Dictionary<object, object>();
            typeBinding.Metadata.Add(key, key);
            return typeBinding;
        }
    }
}

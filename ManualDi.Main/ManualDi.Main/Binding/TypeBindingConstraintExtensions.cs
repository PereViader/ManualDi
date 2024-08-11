using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    public static class TypeBindingConstraintExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBinding<TInterface, TConcrete> WithMetadata<TInterface, TConcrete, TMetadata>(this TypeBinding<TInterface, TConcrete> typeBinding, object key, TMetadata value)
        {
            typeBinding.Metadata ??= new Dictionary<object, object>();
            typeBinding.Metadata.Add(key, value!);
            return typeBinding;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBinding<TInterface, TConcrete> WithMetadata<TInterface, TConcrete>(this TypeBinding<TInterface, TConcrete> typeBinding, object key)
        {
            typeBinding.Metadata ??= new Dictionary<object, object>();
            typeBinding.Metadata.Add(key, key);
            return typeBinding;
        }
    }
}

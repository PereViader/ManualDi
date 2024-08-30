using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    public static class TypeBindingScopeExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBinding Transient<TBinding>(this TBinding typeBinding)
            where TBinding : TypeBinding
        {
            typeBinding.TypeScope = TypeScope.Transient;
            return typeBinding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBinding Single<TBinding>(this TBinding typeBinding)
            where TBinding : TypeBinding
        {
            typeBinding.TypeScope = TypeScope.Single;
            return typeBinding;
        }
    }
}

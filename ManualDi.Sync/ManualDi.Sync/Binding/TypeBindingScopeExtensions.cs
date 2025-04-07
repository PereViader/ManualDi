using System.Runtime.CompilerServices;

namespace ManualDi.Sync
{
    public static class BindingScopeExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBinding Transient<TBinding>(this TBinding binding)
            where TBinding : Binding
        {
            binding.TypeScope = TypeScope.Transient;
            return binding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBinding Single<TBinding>(this TBinding binding)
            where TBinding : Binding
        {
            binding.TypeScope = TypeScope.Single;
            return binding;
        }
    }
}

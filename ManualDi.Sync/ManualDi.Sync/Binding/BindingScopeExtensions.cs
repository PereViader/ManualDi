using System.Runtime.CompilerServices;

namespace ManualDi.Sync
{
    public static class BindingScopeExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBinding Transient<TBinding>(this TBinding binding)
            where TBinding : Binding
        {
            binding.IsTransient = true;
            return binding;
        }
    }
}
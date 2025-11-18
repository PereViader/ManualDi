using System.Runtime.CompilerServices;

namespace ManualDi.Sync
{
    public static class BindingInitializationExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBinding Initialize<TBinding>(
            this TBinding binding, 
            InstanceContainerDelegate initializationDelegate)
            where TBinding : Binding
        {
            binding.InitializationDelegate += initializationDelegate;
            return binding;
        }
    }
}

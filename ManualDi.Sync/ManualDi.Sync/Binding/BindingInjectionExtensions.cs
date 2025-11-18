using System.Runtime.CompilerServices;

namespace ManualDi.Sync
{
    public static class BindingInjectionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBinding Inject<TBinding>(
            this TBinding binding, 
            InstanceContainerDelegate injectionDelegate)
            where TBinding : Binding
        {
            binding.InjectionDelegate += injectionDelegate;
            return binding;
        }
    }
}

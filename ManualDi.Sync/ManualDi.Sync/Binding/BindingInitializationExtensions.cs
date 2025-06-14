using System.Runtime.CompilerServices;

namespace ManualDi.Sync
{
    public static class BindingInitializationExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TApparent, TConcrete> Initialize<TApparent, TConcrete>(
            this Binding<TApparent, TConcrete> binding,
            InstanceContainerDelegate initializationDelegate
            )
        {
            binding.InitializationDelegate += initializationDelegate;
            return binding;
        }
    }
}

using System.Runtime.CompilerServices;

namespace ManualDi.Sync
{
    public static class BindingInjectionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TApparent, TConcrete> Inject<TApparent, TConcrete>(
            this Binding<TApparent, TConcrete> binding, 
            InstanceContainerDelegate<TConcrete> injectionDelegate)
        {
            binding.InjectionDelegate += injectionDelegate;
            return binding;
        }
    }
}

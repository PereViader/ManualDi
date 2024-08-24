using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    public static class DiContainerBindingExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBinding<TConcrete, TConcrete> Bind<TConcrete>(this DiContainerBindings diContainerBindings)
        {
            TypeBinding<TConcrete, TConcrete> typeBinding = new();
            diContainerBindings.AddBinding(typeBinding);
            return typeBinding;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBinding<TInterface, TConcrete> Bind<TInterface, TConcrete>(this DiContainerBindings diContainerBindings)
            where TConcrete : TInterface
        {
            TypeBinding<TInterface, TConcrete> typeBinding = new();
            diContainerBindings.AddBinding(typeBinding);
            return typeBinding;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBinding<TInterface, TConcrete> Bind<TInterface, TConcrete>(this DiContainerBindings diContainerBindings, TypeBinding<TInterface, TConcrete> typeBinding)
            where TConcrete : TInterface
        {
            diContainerBindings.AddBinding(typeBinding);
            return typeBinding;
        }
    }
}

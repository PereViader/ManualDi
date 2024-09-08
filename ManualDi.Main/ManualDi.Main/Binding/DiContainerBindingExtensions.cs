using System;
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
        public static UnsafeTypeBinding Bind(this DiContainerBindings diContainerBindings, Type concreteType)
        {
            UnsafeTypeBinding typeBinding = new(concreteType, concreteType);
            diContainerBindings.AddUnsafeBinding(typeBinding);
            return typeBinding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeTypeBinding Bind(this DiContainerBindings diContainerBindings, Type interfaceType, Type concreteType)
        {
            UnsafeTypeBinding typeBinding = new(interfaceType, concreteType);
            diContainerBindings.AddUnsafeBinding(typeBinding);
            return typeBinding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DiContainerBindings WithStartup<T>(this DiContainerBindings diContainerBindings, Action<T> startup)
        {
            diContainerBindings.QueueStartup(c =>
            {
                var resolved = c.Resolve<T>();
                startup.Invoke(resolved);
            });
            return diContainerBindings;
        }
    }
}

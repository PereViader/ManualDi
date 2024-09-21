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
        public static TypeBinding<TApparent, TConcrete> Bind<TApparent, TConcrete>(this DiContainerBindings diContainerBindings)
            where TConcrete : TApparent
        {
            TypeBinding<TApparent, TConcrete> typeBinding = new();
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
        public static UnsafeTypeBinding Bind(this DiContainerBindings diContainerBindings, Type apparentType, Type concreteType)
        {
            UnsafeTypeBinding typeBinding = new(apparentType, concreteType);
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

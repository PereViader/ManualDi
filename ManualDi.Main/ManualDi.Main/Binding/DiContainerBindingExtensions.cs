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
            diContainerBindings.AddBinding(typeBinding, typeof(TConcrete));
            return typeBinding;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBinding<TApparent, TConcrete> Bind<TApparent, TConcrete>(this DiContainerBindings diContainerBindings)
            where TConcrete : TApparent
        {
            TypeBinding<TApparent, TConcrete> typeBinding = new();
            diContainerBindings.AddBinding(typeBinding, typeof(TApparent));
            return typeBinding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBinding<TConcrete, TConcrete> BindBoth<TApparent, TConcrete>(this DiContainerBindings diContainerBindings)
        {
            TypeBinding<TApparent, TConcrete> typeBinding = new();
            typeBinding.Transient().FromContainerResolve();
            diContainerBindings.AddBinding(typeBinding, typeof(TApparent));

            TypeBinding<TConcrete, TConcrete> concrete = new();
            diContainerBindings.AddBinding(concrete, typeof(TConcrete));
            return concrete;
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

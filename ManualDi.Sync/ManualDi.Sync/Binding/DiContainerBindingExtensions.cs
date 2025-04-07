using System;
using System.Runtime.CompilerServices;

namespace ManualDi.Sync
{
    public static class DiContainerBindingExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TConcrete, TConcrete> Bind<TConcrete>(this DiContainerBindings diContainerBindings)
        {
            Binding<TConcrete, TConcrete> binding = new();
            diContainerBindings.AddBinding(binding, typeof(TConcrete));
            return binding;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TApparent, TConcrete> Bind<TApparent, TConcrete>(this DiContainerBindings diContainerBindings)
            where TConcrete : TApparent
        {
            Binding<TApparent, TConcrete> binding = new();
            diContainerBindings.AddBinding(binding, typeof(TApparent));
            return binding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TConcrete, TConcrete> BindBoth<TApparent, TConcrete>(this DiContainerBindings diContainerBindings)
        {
            Binding<TApparent, TConcrete> binding = new();
            binding.Transient().FromContainerResolve();
            diContainerBindings.AddBinding(binding, typeof(TApparent));

            Binding<TConcrete, TConcrete> concrete = new();
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

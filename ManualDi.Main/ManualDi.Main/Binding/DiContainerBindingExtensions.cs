using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ManualDi.Main
{
    public static class DiContainerBindingExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingSync<TConcrete, TConcrete> Bind<TConcrete>(this DiContainerBindings diContainerBindings)
        {
            TypeBindingSync<TConcrete, TConcrete> typeBindingSync = new();
            diContainerBindings.AddBinding(typeBindingSync, typeof(TConcrete));
            return typeBindingSync;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingSync<TApparent, TConcrete> Bind<TApparent, TConcrete>(this DiContainerBindings diContainerBindings)
            where TConcrete : TApparent
        {
            TypeBindingSync<TApparent, TConcrete> typeBindingSync = new();
            diContainerBindings.AddBinding(typeBindingSync, typeof(TApparent));
            return typeBindingSync;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingAsync<TConcrete, TConcrete> BindAsync<TConcrete>(this DiContainerBindings diContainerBindings)
        {
            TypeBindingAsync<TConcrete, TConcrete> typeBinding = new();
            diContainerBindings.AddBinding(typeBinding, typeof(Task<TConcrete>));
            return typeBinding;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingAsync<TApparent, TConcrete> BindAsync<TApparent, TConcrete>(this DiContainerBindings diContainerBindings)
            where TConcrete : TApparent
        {
            TypeBindingAsync<TApparent, TConcrete> typeBinding = new();
            diContainerBindings.AddBinding(typeBinding, typeof(Task<TApparent>));
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

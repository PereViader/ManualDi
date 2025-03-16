using System;
using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    public static class TypeBindingSyncFromExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingSync<TApparent, TConcrete> FromContainerResolve<TApparent, TConcrete>(
            this TypeBindingSync<TApparent, TConcrete> typeBindingSync
            )
        {
            typeBindingSync.Dependencies = static d => d.Dependency<TConcrete>();
            typeBindingSync.CreateDelegate = static c => c.Resolve<TConcrete>();
            return typeBindingSync;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingSync<TApparent, TConcrete> FromContainerResolve<TApparent, TConcrete>(
            this TypeBindingSync<TApparent, TConcrete> typeBindingSync,
            FilterBindingDelegate filterBindingDelegate
            )
        {
            typeBindingSync.Dependencies = d => d.Dependency<TConcrete>(filterBindingDelegate);
            typeBindingSync.CreateDelegate = c => c.Resolve<TConcrete>(filterBindingDelegate);
            return typeBindingSync;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingSync<TApparent, TConcrete> FromInstance<TApparent, TConcrete>(
            this TypeBindingSync<TApparent, TConcrete> typeBindingSync,
            TConcrete instance
            )
        {
            typeBindingSync.CreateDelegate = _ => instance;
            return typeBindingSync;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingSync<TApparent, TConcrete> FromMethod<TApparent, TConcrete>(
            this TypeBindingSync<TApparent, TConcrete> typeBindingSync,
            FromDelegate<TConcrete> fromDelegate
            )
            
        {
            typeBindingSync.CreateDelegate = fromDelegate;
            return typeBindingSync;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingSync<TApparent, TConcrete> FromMethod<TApparent, TConcrete>(
            this TypeBindingSync<TApparent, TConcrete> typeBindingSync,
            FromDelegate<TConcrete> fromDelegate,
            Action<IDependencyResolver> dependencies
        )
        {
            typeBindingSync.Dependencies = dependencies;
            typeBindingSync.CreateDelegate = fromDelegate;
            return typeBindingSync;
        }
    }
}

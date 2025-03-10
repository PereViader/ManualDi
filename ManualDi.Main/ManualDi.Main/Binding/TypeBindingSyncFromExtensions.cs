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
            typeBindingSync.Dependencies = new[] { typeof(TConcrete) };
            typeBindingSync.CreateDelegate = static c => c.Resolve<TConcrete>();
            return typeBindingSync;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingSync<TApparent, TConcrete> FromContainerResolve<TApparent, TConcrete>(
            this TypeBindingSync<TApparent, TConcrete> typeBindingSync,
            FilterBindingDelegate filterBindingDelegate
            )
        {
            typeBindingSync.Dependencies = new[] { typeof(TConcrete) };
            typeBindingSync.CreateDelegate = c => c.Resolve<TConcrete>(filterBindingDelegate);
            return typeBindingSync;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingSync<TApparent, TConcrete> FromInstance<TApparent, TConcrete>(
            this TypeBindingSync<TApparent, TConcrete> typeBindingSync,
            TConcrete instance
            )
        {
            typeBindingSync.Dependencies = Array.Empty<Type>();
            typeBindingSync.CreateDelegate = _ => instance;
            return typeBindingSync;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingSync<TApparent, TConcrete> FromMethod<TApparent, TConcrete>(
            this TypeBindingSync<TApparent, TConcrete> typeBindingSync,
            FromDelegate<TConcrete> fromDelegate,
            Type[] dependencies
            )
        {
            typeBindingSync.Dependencies = dependencies;
            typeBindingSync.CreateDelegate = fromDelegate;
            return typeBindingSync;
        }
    }
}

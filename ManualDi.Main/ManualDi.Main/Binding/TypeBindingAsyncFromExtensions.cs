using System;
using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    public static class TypeBindingAsyncFromExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingAsync<TApparent, TConcrete> FromContainerResolve<TApparent, TConcrete>(
            this TypeBindingAsync<TApparent, TConcrete> typeBindingAsync
        )
        {
            typeBindingAsync.Dependencies = static d => d.Dependency<TConcrete>();
            typeBindingAsync.CreateDelegate = static c => c.Resolve<TConcrete>();
            return typeBindingAsync;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingAsync<TApparent, TConcrete> FromContainerResolve<TApparent, TConcrete>(
            this TypeBindingAsync<TApparent, TConcrete> typeBindingAsync,
            FilterBindingDelegate filterBindingDelegate
        )
        {
            typeBindingAsync.Dependencies = d => d.Dependency<TConcrete>(filterBindingDelegate);
            typeBindingAsync.CreateDelegate = c => c.Resolve<TConcrete>(filterBindingDelegate);
            return typeBindingAsync;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingAsync<TApparent, TConcrete> FromSubContainerResolve<TApparent, TConcrete>(
            this TypeBindingAsync<TApparent, TConcrete> typeBindingAsync,
            InstallDelegate installDelegate,
            bool isContainerParent = true,
            Action<IDependencyResolver>? dependencies = null
        )
        {
            typeBindingAsync.Dependencies = dependencies;
            IDiContainer? subContainer = null;
            typeBindingAsync.CreateAsyncDelegate = async (c, ct) =>
            {
                var bindings = new DiContainerBindings().Install(installDelegate);
                if (isContainerParent)
                {
                    bindings.WithParentContainer(c);
                }
                subContainer = await bindings.Build(ct);
                c.QueueAsyncDispose(subContainer);
                return subContainer.Resolve<TConcrete>();
            };
            return typeBindingAsync;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingAsync<TApparent, TConcrete> FromInstance<TApparent, TConcrete>(
            this TypeBindingAsync<TApparent, TConcrete> typeBindingAsync,
            TConcrete instance
        )
        {
            typeBindingAsync.CreateDelegate = _ => instance;
            return typeBindingAsync;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingAsync<TApparent, TConcrete> FromMethod<TApparent, TConcrete>(
            this TypeBindingAsync<TApparent, TConcrete> typeBindingAsync,
            FromDelegate<TConcrete> fromDelegate
        )
        {
            typeBindingAsync.CreateDelegate = fromDelegate;
            return typeBindingAsync;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingAsync<TApparent, TConcrete> FromMethod<TApparent, TConcrete>(
            this TypeBindingAsync<TApparent, TConcrete> typeBindingAsync,
            FromDelegate<TConcrete> fromDelegate,
            Action<IDependencyResolver> dependencies
        )
        {
            typeBindingAsync.Dependencies = dependencies;
            typeBindingAsync.CreateDelegate = fromDelegate;
            return typeBindingAsync;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingAsync<TApparent, TConcrete> FromMethodAsync<TApparent, TConcrete>(
            this TypeBindingAsync<TApparent, TConcrete> typeBindingAsync,
            FromAsyncDelegate<TConcrete> fromAsyncDelegate,
            Action<IDependencyResolver> dependencies
        )
        {
            typeBindingAsync.Dependencies = dependencies;
            typeBindingAsync.CreateAsyncDelegate = fromAsyncDelegate;
            return typeBindingAsync;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingAsync<TApparent, TConcrete> FromMethodAsync<TApparent, TConcrete>(
            this TypeBindingAsync<TApparent, TConcrete> typeBindingAsync,
            FromAsyncDelegate<TConcrete> fromAsyncDelegate
        )
        {
            typeBindingAsync.CreateAsyncDelegate = fromAsyncDelegate;
            return typeBindingAsync;
        }
    }
}
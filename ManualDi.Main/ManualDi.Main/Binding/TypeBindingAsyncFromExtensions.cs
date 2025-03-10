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
            typeBindingAsync.Dependencies = new []{ (typeof(TConcrete), default(FilterBindingDelegate?)) };
            typeBindingAsync.CreateDelegate = static c => c.Resolve<TConcrete>();
            return typeBindingAsync;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingAsync<TApparent, TConcrete> FromContainerResolve<TApparent, TConcrete>(
            this TypeBindingAsync<TApparent, TConcrete> typeBindingAsync,
            FilterBindingDelegate filterBindingDelegate
        )
        {
            typeBindingAsync.Dependencies = new []{ (typeof(TConcrete), default(FilterBindingDelegate?)) };
            typeBindingAsync.CreateDelegate = c => c.Resolve<TConcrete>(filterBindingDelegate);
            return typeBindingAsync;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingAsync<TApparent, TConcrete> FromSubContainerResolve<TApparent, TConcrete>(
            this TypeBindingAsync<TApparent, TConcrete> typeBindingAsync,
            InstallDelegate installDelegate,
            bool isContainerParent = true,
            Type[]? parentDependencies = null
        )
        {
            typeBindingAsync.Dependencies = parentDependencies ?? Array.Empty<Type>();
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
            typeBindingAsync.Dependencies = Array.Empty<Type>();
            typeBindingAsync.CreateDelegate = _ => instance;
            return typeBindingAsync;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingAsync<TApparent, TConcrete> FromMethod<TApparent, TConcrete>(
            this TypeBindingAsync<TApparent, TConcrete> typeBindingAsync,
            FromDelegate<TConcrete> fromDelegate,
            Type[] dependencies
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
            Type[] dependencies
        )
        {
            typeBindingAsync.Dependencies = dependencies;
            typeBindingAsync.CreateAsyncDelegate = fromAsyncDelegate;
            return typeBindingAsync;
        }
    }
}
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
            typeBindingAsync.CreateDelegate = static c => c.Resolve<TConcrete>();
            return typeBindingAsync;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingAsync<TApparent, TConcrete> FromContainerResolve<TApparent, TConcrete>(
            this TypeBindingAsync<TApparent, TConcrete> typeBindingAsync,
            FilterBindingDelegate filterBindingDelegate
        )
        {
            typeBindingAsync.CreateDelegate = c => c.Resolve<TConcrete>(filterBindingDelegate);
            return typeBindingAsync;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingAsync<TApparent, TConcrete> FromSubContainerResolve<TApparent, TConcrete>(
            this TypeBindingAsync<TApparent, TConcrete> typeBindingAsync,
            InstallDelegate installDelegate,
            bool isContainerParent = true
        )
        {
            IDiContainer? subContainer = null;
            typeBindingAsync.CreateDelegate = c =>
            {
                var bindings = new DiContainerBindings().Install(installDelegate);
                if (isContainerParent)
                {
                    bindings.WithParentContainer(c);
                }
                subContainer = bindings.Build();
                c.QueueDispose(subContainer);
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
            CreateDelegate<TConcrete> createDelegate
        )
        {
            typeBindingAsync.CreateDelegate = createDelegate;
            return typeBindingAsync;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingAsync<TApparent, TConcrete> FromMethodAsync<TApparent, TConcrete>(
            this TypeBindingAsync<TApparent, TConcrete> typeBindingAsync,
            CreateAsyncDelegate<TConcrete> createAsyncDelegate
        )
        {
            typeBindingAsync.CreateAsyncDelegate = createAsyncDelegate;
            return typeBindingAsync;
        }
    }
}
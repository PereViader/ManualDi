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
            typeBindingSync.CreateDelegate = static c => c.Resolve<TConcrete>();
            return typeBindingSync;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingSync<TApparent, TConcrete> FromContainerResolve<TApparent, TConcrete>(
            this TypeBindingSync<TApparent, TConcrete> typeBindingSync,
            FilterBindingDelegate filterBindingDelegate
            )
        {
            typeBindingSync.CreateDelegate = c => c.Resolve<TConcrete>(filterBindingDelegate);
            return typeBindingSync;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBindingSync<TApparent, TConcrete> FromSubContainerResolve<TApparent, TConcrete>(
            this TypeBindingSync<TApparent, TConcrete> typeBindingSync,
            InstallDelegate installDelegate,
            bool isContainerParent = true
        )
        {
            IDiContainer? subContainer = null;
            typeBindingSync.CreateDelegate = c =>
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
            CreateDelegate<TConcrete> createDelegate
            )
        {
            typeBindingSync.CreateDelegate = createDelegate;
            return typeBindingSync;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBinding Lazy<TBinding>(this TBinding typeBinding)
            where TBinding : TypeBinding
        {
            typeBinding.IsLazy = true;
            return typeBinding;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBinding NonLazy<TBinding>(this TBinding typeBinding)
            where TBinding : TypeBinding
        {
            typeBinding.IsLazy = false;
            return typeBinding;
        }
    }
}

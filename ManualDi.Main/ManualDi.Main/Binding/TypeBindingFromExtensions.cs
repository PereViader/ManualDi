using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    public static class TypeBindingFromExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBinding<TApparent, TConcrete> FromContainerResolve<TApparent, TConcrete>(
            this TypeBinding<TApparent, TConcrete> typeBinding
            )
        {
            typeBinding.CreateConcreteDelegate = static c => c.Resolve<TConcrete>();
            return typeBinding;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBinding<TApparent, TConcrete> FromContainerResolve<TApparent, TConcrete>(
            this TypeBinding<TApparent, TConcrete> typeBinding,
            FilterBindingDelegate filterBindingDelegate
            )
        {
            typeBinding.CreateConcreteDelegate = c => c.Resolve<TConcrete>(filterBindingDelegate);
            return typeBinding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBinding<TApparent, TConcrete> FromSubContainerResolve<TApparent, TConcrete>(
            this TypeBinding<TApparent, TConcrete> typeBinding,
            InstallDelegate installDelegate,
            bool isContainerParent = true
        )
        {
            IDiContainer? subContainer = null;
            typeBinding.CreateConcreteDelegate = c =>
            {
                var bindings = new DiContainerBindings().Install(installDelegate);
                if (isContainerParent)
                {
                    bindings.WithParentContainer(c);
                }
                subContainer = bindings.Build();
                return subContainer.Resolve<TConcrete>();
            };
            typeBinding.Dispose((_, _) => subContainer?.Dispose());
            return typeBinding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBinding<TApparent, TConcrete> FromInstance<TApparent, TConcrete>(
            this TypeBinding<TApparent, TConcrete> typeBinding,
            TConcrete instance
            )
        {
            typeBinding.CreateConcreteDelegate = _ => instance;
            return typeBinding;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TypeBinding<TApparent, TConcrete> FromMethod<TApparent, TConcrete>(
            this TypeBinding<TApparent, TConcrete> typeBinding,
            CreateDelegate<TConcrete> createDelegate
            )
        {
            typeBinding.CreateConcreteDelegate = createDelegate;
            return typeBinding;
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

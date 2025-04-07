using System.Runtime.CompilerServices;

namespace ManualDi.Sync
{
    public static class BindingFromExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TApparent, TConcrete> FromContainerResolve<TApparent, TConcrete>(
            this Binding<TApparent, TConcrete> binding
            )
        {
            binding.CreateConcreteDelegate = static c => c.Resolve<TConcrete>();
            return binding;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TApparent, TConcrete> FromContainerResolve<TApparent, TConcrete>(
            this Binding<TApparent, TConcrete> binding,
            FilterBindingDelegate filterBindingDelegate
            )
        {
            binding.CreateConcreteDelegate = c => c.Resolve<TConcrete>(filterBindingDelegate);
            return binding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TApparent, TConcrete> FromSubContainerResolve<TApparent, TConcrete>(
            this Binding<TApparent, TConcrete> binding,
            InstallDelegate installDelegate,
            bool isContainerParent = true
        )
        {
            IDiContainer? subContainer = null;
            binding.CreateConcreteDelegate = c =>
            {
                var bindings = new DiContainerBindings().Install(installDelegate);
                if (isContainerParent)
                {
                    bindings.WithParentContainer(c);
                }
                subContainer = bindings.Build();
                return subContainer.Resolve<TConcrete>();
            };
            binding.Dispose((_, _) => subContainer?.Dispose());
            return binding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TApparent, TConcrete> FromInstance<TApparent, TConcrete>(
            this Binding<TApparent, TConcrete> binding,
            TConcrete instance
            )
        {
            binding.CreateConcreteDelegate = _ => instance;
            return binding;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TApparent, TConcrete> FromMethod<TApparent, TConcrete>(
            this Binding<TApparent, TConcrete> binding,
            CreateDelegate<TConcrete> createDelegate
            )
        {
            binding.CreateConcreteDelegate = createDelegate;
            return binding;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBinding Lazy<TBinding>(this TBinding binding)
            where TBinding : Binding
        {
            binding.IsLazy = true;
            return binding;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TBinding NonLazy<TBinding>(this TBinding binding)
            where TBinding : Binding
        {
            binding.IsLazy = false;
            return binding;
        }
    }
}

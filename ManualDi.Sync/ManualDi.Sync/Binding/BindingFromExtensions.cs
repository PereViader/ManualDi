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
            InstallDelegate installDelegate
        )
        {
            IDiContainer? subContainer = null;
            binding.CreateConcreteDelegate = c =>
            {
                var bindings = new DiContainerBindings()
                    .WithParentContainer(c)
                    .Install(installDelegate);
                
                subContainer = bindings.Build();
                return subContainer.Resolve<TConcrete>();
            };
            binding.Dispose((_, _) => subContainer?.Dispose());
            return binding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TApparent, TConcrete> FromIsolatedSubContainerResolve<TApparent, TConcrete>(
            this Binding<TApparent, TConcrete> binding,
            InstallDelegate installDelegate
        )
        {
            IDiContainer? subContainer = null;
            binding.CreateConcreteDelegate = c =>
            {
                var bindings = new DiContainerBindings()
                    .Install(installDelegate);
                
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
    }
}

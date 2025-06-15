using System;
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
            FromDelegate fromDelegate = static c => c.Resolve<TConcrete>();
            binding.FromDelegate = fromDelegate;
            return binding;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TApparent, TConcrete> FromContainerResolve<TApparent, TConcrete>(
            this Binding<TApparent, TConcrete> binding,
            FilterBindingDelegate filterBindingDelegate
            )
        {
            FromDelegate fromDelegate = c => c.Resolve<TConcrete>(filterBindingDelegate);
            binding.FromDelegate = fromDelegate;
            return binding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete("Use b.BindSubContainer<TApparent>(...) instead, when using this this method, it will not have access to b.ResolveInstance<TConfig> present on the parent DiContainerBindings")]
        public static Binding<TApparent, TConcrete> FromSubContainerResolve<TApparent, TConcrete>(
            this Binding<TApparent, TConcrete> binding,
            InstallDelegate installDelegate
        )
        {
            FromDelegate fromDelegate = c =>
            {
                var bindings = new DiContainerBindings()
                    .Install(installDelegate);
                
                IDiContainer subContainer = bindings.Build();
                binding.Dispose((_, _) => subContainer.Dispose());
                return subContainer.Resolve<TConcrete>();
            };
            
            IDiContainer? subContainer = null;
            binding.FromDelegate = fromDelegate;
            binding.Dispose((_, _) => subContainer?.Dispose());
            return binding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete("Use b.BindSubContainer<TApparent>(...) instead, when using this this method, it will not have access to b.ResolveInstance<TConfig> present on the parent DiContainerBindings")]
        public static Binding<TApparent, TConcrete> FromIsolatedSubContainerResolve<TApparent, TConcrete>(
            this Binding<TApparent, TConcrete> binding,
            InstallDelegate installDelegate
        )
        {
            IDiContainer? subContainer = null;
            FromDelegate fromDelegate = c =>
            {
                var bindings = new DiContainerBindings()
                    .Install(installDelegate);
                
                subContainer = bindings.Build();
                return subContainer.Resolve<TConcrete>();
            };
            binding.FromDelegate = fromDelegate;
            binding.Dispose((_, _) => subContainer?.Dispose());
            return binding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TApparent, TConcrete> FromInstance<TApparent, TConcrete>(
            this Binding<TApparent, TConcrete> binding,
            TConcrete instance
            )
        {
            binding.FromDelegate = instance;
            return binding;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TApparent, TConcrete> FromMethod<TApparent, TConcrete>(
            this Binding<TApparent, TConcrete> binding,
            FromDelegate fromDelegate
            )
        {
            binding.FromDelegate = fromDelegate;
            return binding;
        }
    }
}

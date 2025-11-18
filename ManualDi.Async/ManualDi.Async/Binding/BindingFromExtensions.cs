using System;
using System.Runtime.CompilerServices;

namespace ManualDi.Async
{
    public static class BindingFromExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TConcrete> FromMethod<TConcrete>(
            this Binding<TConcrete> binding,
            FromDelegate fromDelegate
        )
        {
            binding.FromDelegate = fromDelegate;
            return binding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TConcrete> FromMethodAsync<TConcrete>(
            this Binding<TConcrete> binding,
            FromAsyncDelegate fromAsyncDelegate
        )
        {
            binding.FromDelegate = fromAsyncDelegate;
            return binding;
        }
        
        public static FromAsyncDelegate? GetFromAsyncDelegateNullable<TConcrete>(
            this Binding<TConcrete> binding
        )
        {
            return binding.FromDelegate as FromAsyncDelegate;
        }
        
        public static FromDelegate? GetFromDelegateNullable<TConcrete>(
            this Binding<TConcrete> binding
        )
        {
            return binding.FromDelegate as FromDelegate;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TConcrete> DependsOn<TConcrete>(
            this Binding<TConcrete> binding,
            Action<IDependencyResolver> action
        )
        {
            binding.Dependencies += action;
            return binding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TConcrete> FromInstance<TConcrete>(
            this Binding<TConcrete> binding,
            TConcrete instance
        )
        {
            binding.FromDelegate = instance;
            return binding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TConcrete> FromContainerResolve<TConcrete>(
            this Binding<TConcrete> binding
        )
        {
            return binding
                .FromMethod(static c => c.Resolve<TConcrete>())
                .DependsOn(static d => d.ConstructorDependency<TConcrete>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TConcrete> FromContainerResolve<TConcrete>(
            this Binding<TConcrete> binding,
            FilterBindingDelegate filterBindingDelegate
        )
        {
            return binding
                .FromMethod(c => c.Resolve<TConcrete>(filterBindingDelegate))
                .DependsOn(d => d.ConstructorDependency<TConcrete>(filterBindingDelegate));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete("Use b.BindSubContainer<TApparent>(...) instead, when using this this method, it will not have access to b.ResolveInstance<TConfig> present on the parent DiContainerBindings")]
        public static Binding<TConcrete> FromSubContainerResolve<TConcrete>(
            this Binding<TConcrete> binding,
            InstallDelegate installDelegate
        )
        {
            return new DiContainerBindings()
                .Install(installDelegate)
                .BindAsSubContainer(binding, true);
        }
        
        /// <summary>
        /// Creates a subcontainer and resolves the apparent type from it
        /// The object graph must be self sufficient and not depend on the parent container 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete("Use b.BindSubContainer<TApparent>(...) instead, when using this this method, it will not have access to b.ResolveInstance<TConfig> present on the parent DiContainerBindings")]
        public static Binding<TConcrete> FromIsolatedSubContainerResolve<TConcrete>(
            this Binding<TConcrete> binding,
            InstallDelegate installDelegate
        )
        {
            return new DiContainerBindings()
                .Install(installDelegate)
                .BindAsSubContainer(binding, false);
        }
    }
}
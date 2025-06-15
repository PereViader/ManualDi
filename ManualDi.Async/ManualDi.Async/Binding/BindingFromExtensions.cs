using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ManualDi.Async
{
    public static class BindingFromExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TApparent, TConcrete> FromMethod<TApparent, TConcrete>(
            this Binding<TApparent, TConcrete> binding,
            FromDelegate fromDelegate
        )
        {
            binding.FromDelegate = fromDelegate;
            return binding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TApparent, TConcrete> FromMethodAsync<TApparent, TConcrete>(
            this Binding<TApparent, TConcrete> binding,
            FromAsyncDelegate fromAsyncDelegate
        )
        {
            binding.FromDelegate = fromAsyncDelegate;
            return binding;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TApparent, TConcrete> DependsOn<TApparent, TConcrete>(
            this Binding<TApparent, TConcrete> binding,
            Action<IDependencyResolver> action
        )
        {
            binding.Dependencies += action;
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
        public static Binding<TApparent, TConcrete> FromContainerResolve<TApparent, TConcrete>(
            this Binding<TApparent, TConcrete> binding
        )
        {
            return binding
                .FromMethod(static c => c.Resolve<TConcrete>())
                .DependsOn(static d => d.ConstructorDependency<TConcrete>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TApparent, TConcrete> FromContainerResolve<TApparent, TConcrete>(
            this Binding<TApparent, TConcrete> binding,
            FilterBindingDelegate filterBindingDelegate
        )
        {
            return binding
                .FromMethod(c => c.Resolve<TConcrete>(filterBindingDelegate))
                .DependsOn(d => d.ConstructorDependency<TConcrete>(filterBindingDelegate));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete("Use b.BindSubContainer<TApparent>(...) instead, when using this this method, it will not have access to b.ResolveInstance<TConfig> present on the parent DiContainerBindings")]
        public static Binding<TApparent, TConcrete> FromSubContainerResolve<TApparent, TConcrete>(
            this Binding<TApparent, TConcrete> binding,
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
        public static Binding<TApparent, TConcrete> FromIsolatedSubContainerResolve<TApparent, TConcrete>(
            this Binding<TApparent, TConcrete> binding,
            InstallDelegate installDelegate
        )
        {
            return new DiContainerBindings()
                .Install(installDelegate)
                .BindAsSubContainer(binding, false);
        }
    }
}
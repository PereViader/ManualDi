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
            return binding.FromMethod(_ => instance);
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
        public static Binding<TApparent, TConcrete> FromSubContainerResolve<TApparent, TConcrete>(
            this Binding<TApparent, TConcrete> binding,
            InstallDelegate installDelegate
        )
        {
            var bindings = new DiContainerBindings()
                .Install(installDelegate);

            // This will calculate the dependencies in runtime.
            // If this is too costly, use the overload that provides them manually
            var dependencies = bindings.GatherDependencies();

            // The construction of the subcontainer is broken down in two steps because otherwise
            // subcontainers would create and inject instances but instances required for inject would not yet be ready 
            Func<ValueTask<DiContainer>> injectFunc = null!;
            
            return binding
                .FromMethodAsync(async (c, ct) =>
                {
                    var (subContainer, continueTask) = await bindings
                        .WithParentContainer(c)
                        .BuildDelayed(ct);

                    injectFunc = continueTask;
                    
                    c.QueueAsyncDispose(subContainer);
                    return subContainer.Resolve<TConcrete>();
                })
                .InjectAsync(async (_, _, _) => await injectFunc.Invoke())
                .DependsOn(dependencies);
        }
        
        /// <summary>
        /// Use this method when subcontainer runtime dependency resolution is too costly.
        /// Note: You'll need to manually maintain them when they change
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TApparent, TConcrete> FromSubContainerResolve<TApparent, TConcrete>(
            this Binding<TApparent, TConcrete> binding,
            InstallDelegate installDelegate,
            Action<IDependencyResolver> dependencies
        )
        {
            // The construction of the subcontainer is broken down in two steps because otherwise
            // subcontainers would create and inject instances but instances required for inject would not yet be ready 
            Func<ValueTask<DiContainer>> injectFunc = null!;
            
            return binding
                .FromMethodAsync(async (c, ct) =>
                {
                    var (subContainer, continueTask) = await new DiContainerBindings()
                        .Install(installDelegate)
                        .WithParentContainer(c)
                        .BuildDelayed(ct);

                    injectFunc = continueTask;
                    
                    c.QueueAsyncDispose(subContainer);
                    return subContainer.Resolve<TConcrete>();
                })
                .InjectAsync(async (_, _, _) => await injectFunc.Invoke())
                .DependsOn(dependencies);
        }
        
        /// <summary>
        /// Creates a subcontainer and resolves the apparent type from it
        /// The object graph must be self sufficient and not depend on the parent container 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Binding<TApparent, TConcrete> FromIsolatedSubContainerResolve<TApparent, TConcrete>(
            this Binding<TApparent, TConcrete> binding,
            InstallDelegate installDelegate
        )
        {
            return binding
                .FromMethodAsync(async (c, ct) =>
                {
                    var subContainer = await new DiContainerBindings()
                        .Install(installDelegate)
                        .Build(ct);
                    c.QueueAsyncDispose(subContainer);
                    return subContainer.Resolve<TConcrete>();
                });
        }
    }
}
using System;
using System.Runtime.CompilerServices;

namespace ManualDi.Main
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
            InstallDelegate installDelegate,
            Action<IDependencyResolver> dependencyResolverAction,
            bool isContainerParent = true
        )
        {
            return binding
                .FromMethodAsync(async (c, ct) =>
                {
                    var bindings = new DiContainerBindings().Install(installDelegate);
                    if (isContainerParent)
                    {
                        bindings.WithParentContainer(c);
                    }
                    var subContainer = await bindings.Build(ct);
                    c.QueueAsyncDispose(subContainer);
                    return subContainer.Resolve<TConcrete>();
                })
                .DependsOn(dependencyResolverAction);
        }
    }
}
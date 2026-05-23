using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ManualDi.Async
{
    internal class DependencyExtractor : IDependencyResolver
    {
        private readonly Dictionary<IntPtr, BindingNode> bindingsByType;
        private readonly BindingContext bindingContext = new();
        
        private Binding? injectedBinding;
        private IDependencyResolver? parentResolver;

        public DependencyExtractor(Dictionary<IntPtr, BindingNode> bindingsByType)
        {
            this.bindingsByType = bindingsByType;
        }

        public void ConstructorDependency<T>()
        {
            if (GetBinding(typeof(T)) is not null)
            {
                return;
            }
            parentResolver!.ConstructorDependency<T>();
        }

        public void ConstructorDependency<T>(FilterBindingDelegate filter)
        {
            if (GetBinding(typeof(T), filter) is not null)
            {
                return;
            }
            parentResolver!.ConstructorDependency<T>(filter);
        }

        public void NullableConstructorDependency<T>()
        {
            if (GetBinding(typeof(T)) is not null)
            {
                return;
            }
            parentResolver!.NullableConstructorDependency<T>();
        }

        public void NullableConstructorDependency<T>(FilterBindingDelegate filter)
        {
            if (GetBinding(typeof(T), filter) is not null)
            {
                return;
            }
            parentResolver!.NullableConstructorDependency<T>(filter);
        }

        public void InjectionDependency<T>()
        {
            if (GetBinding(typeof(T)) is not null)
            {
                return; 
            }
            parentResolver!.InjectionDependency<T>();
        }

        public void InjectionDependency<T>(FilterBindingDelegate filter)
        {
            if (GetBinding(typeof(T), filter) is not null)
            {
                return;
            }
            parentResolver!.InjectionDependency<T>(filter);
        }

        public void NullableInjectionDependency<T>()
        {
            if (GetBinding(typeof(T)) is not null)
            {
                return;
            }
            parentResolver!.NullableInjectionDependency<T>();
        }

        public void NullableInjectionDependency<T>(FilterBindingDelegate filter)
        {
            if (GetBinding(typeof(T), filter) is not null)
            {
                return;
            }
            parentResolver!.NullableInjectionDependency<T>(filter);
        }


        public void ResolveDependencies(IDependencyResolver resolver)
        {
            parentResolver = resolver;
            
            foreach (var node in bindingsByType.Values)
            {
                injectedBinding = node.Binding;
                injectedBinding.Dependencies?.Invoke(this);

                var current = node.Next;
                while (current is not null)
                {
                    injectedBinding = current.Binding;
                    injectedBinding.Dependencies?.Invoke(this);
                    current = current.Next;
                }
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Binding? GetBinding(Type type)
        {
            if (!bindingsByType.TryGetValue(type.TypeHandle.Value, out var node))
            {
                return null;
            }

            if (node.Binding.FilterBindingDelegate is null)
            {
                return node.Binding;
            }

            bindingContext.InjectedIntoBinding = injectedBinding;
            
            bindingContext.Binding = node.Binding;
            if (node.Binding.FilterBindingDelegate?.Invoke(bindingContext) ?? true)
            {
                return node.Binding;
            }

            var current = node.Next;
            while (current is not null)
            {
                bindingContext.Binding = current.Binding;
                if (current.Binding.FilterBindingDelegate?.Invoke(bindingContext) ?? true)
                {
                    return current.Binding;
                }
                current = current.Next;
            }
            
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Binding? GetBinding(Type type, FilterBindingDelegate filterBindingDelegate)
        {
            if (!bindingsByType.TryGetValue(type.TypeHandle.Value, out var node))
            {
                return null;
            }

            bindingContext.InjectedIntoBinding = injectedBinding;
            
            bindingContext.Binding = node.Binding;
            if (filterBindingDelegate.Invoke(bindingContext) &&
                (node.Binding.FilterBindingDelegate?.Invoke(bindingContext) ?? true))
            {
                return node.Binding;
            }

            var current = node.Next;
            while (current is not null)
            {
                bindingContext.Binding = current.Binding;
                if (filterBindingDelegate.Invoke(bindingContext) &&
                    (current.Binding.FilterBindingDelegate?.Invoke(bindingContext) ?? true))
                {
                    return current.Binding;
                }
                current = current.Next;
            }
            
            return null;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ManualDi.Async
{
    internal class DependencyExtractor : IDependencyResolver
    {
        private readonly Dictionary<IntPtr, Binding> bindingsByType;
        private readonly BindingContext bindingContext = new();
        
        private Binding? injectedBinding;
        private IDependencyResolver? parentResolver;

        public DependencyExtractor(Dictionary<IntPtr, Binding> bindingsByType)
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
            
            foreach (var rootBinding in bindingsByType)
            {
                injectedBinding = rootBinding.Value;
                while (injectedBinding is not null)
                {
                    injectedBinding.Dependencies?.Invoke(this);
                    injectedBinding = injectedBinding.NextBinding;
                }
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Binding? GetBinding(Type type)
        {
            if (!bindingsByType.TryGetValue(type.TypeHandle.Value, out Binding? binding))
            {
                return null;
            }

            if (binding.FilterBindingDelegate is null)
            {
                return binding;
            }

            bindingContext.InjectedIntoBinding = injectedBinding;
            do
            {
                bindingContext.Binding = binding;

                if (binding.FilterBindingDelegate?.Invoke(bindingContext) ?? true)
                {
                    return binding;
                }

                binding = binding.NextBinding;
            } while (binding is not null);
            
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Binding? GetBinding(Type type, FilterBindingDelegate filterBindingDelegate)
        {
            if (!bindingsByType.TryGetValue(type.TypeHandle.Value, out Binding? binding))
            {
                return null;
            }

            bindingContext.InjectedIntoBinding = injectedBinding;
            do
            {
                bindingContext.Binding = binding;

                if (filterBindingDelegate.Invoke(bindingContext) &&
                    (binding.FilterBindingDelegate?.Invoke(bindingContext) ?? true))
                {
                    return binding;
                }

                binding = binding.NextBinding;
            } while (binding is not null);
            
            return null;
        }
    }
}
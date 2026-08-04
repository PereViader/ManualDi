using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ManualDi.Async
{
    internal class DependencyExtractor : IDependencyResolver
    {
        private readonly Dictionary<IntPtr, BindingNode> bindingsByType;
        
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

        public void NullableConstructorDependency<T>()
        {
            if (GetBinding(typeof(T)) is not null)
            {
                return;
            }
            parentResolver!.NullableConstructorDependency<T>();
        }

        public void InjectionDependency<T>()
        {
            if (GetBinding(typeof(T)) is not null)
            {
                return; 
            }
            parentResolver!.InjectionDependency<T>();
        }

        public void NullableInjectionDependency<T>()
        {
            if (GetBinding(typeof(T)) is not null)
            {
                return;
            }
            parentResolver!.NullableInjectionDependency<T>();
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

            return node.Binding;
        }
    }
}
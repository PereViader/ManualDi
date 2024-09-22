using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    public sealed class DiContainer : IDiContainer
    {
        private readonly Dictionary<Type, List<TypeBinding>> allTypeBindings;
        private readonly IDiContainer? parentDiContainer;
        private readonly BindingContext bindingContext = new();
        
        private DiContainerInitializer diContainerInitializer;
        private DiContainerDisposer diContainerDisposer;
        private TypeBinding? injectedTypeBinding;

        public DiContainer(
            Dictionary<Type, List<TypeBinding>> allTypeBindings, 
            IDiContainer? parentDiContainer,
            int? initializationsCount = null, 
            int? initializationsOnDepthCount = null,
            int? disposablesCount = null)
        {
            diContainerInitializer = new(initializationsCount, initializationsOnDepthCount);
            diContainerDisposer = new(disposablesCount);
            
            this.allTypeBindings = allTypeBindings;
            this.parentDiContainer = parentDiContainer;
        }

        public void Initialize()
        {
            foreach (var bindings in allTypeBindings)
            {
                foreach (var binding in bindings.Value)
                {
                    if (!binding.IsLazy)
                    {
                        ResolveBinding(binding);
                    }
                }
            }
        }

        public object? ResolveContainer(Type type, FilterBindingDelegate? filterBindingDelegate)
        {
            var typeBinding = GetTypeForConstraint(type, filterBindingDelegate);
            if (typeBinding is not null)
            {
                return ResolveBinding(typeBinding);
            }

            if (parentDiContainer is null)
            {
                return null;
            }

            return parentDiContainer.ResolveContainer(type, filterBindingDelegate);
        }

        private object ResolveBinding(TypeBinding typeBinding)
        {
            if (typeBinding.SingleInstance is not null) //Optimization: We don't check if Scope is Single
            {
                return typeBinding.SingleInstance;
            }
            
            var previousInjectedTypeBinding = injectedTypeBinding;
            injectedTypeBinding = typeBinding;
        
            var instance = typeBinding.CreateNew(this) ?? throw new InvalidOperationException($"Could not create object for {GetType().FullName}");
            if (typeBinding.TypeScope is TypeScope.Single)
            {
                typeBinding.SingleInstance = instance;
            }
            
            typeBinding.InjectObject(instance, this);
            if (typeBinding.TryToDispose && instance is IDisposable disposable)
            {
                QueueDispose(disposable);
            }
            diContainerInitializer.Queue(typeBinding, instance);

            injectedTypeBinding = previousInjectedTypeBinding;
            if (injectedTypeBinding is null)
            {
                diContainerInitializer.InitializeCurrentLevelQueued(this);
            }

            return instance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TypeBinding? GetTypeForConstraint(Type type, FilterBindingDelegate? filterBindingDelegate)
        {
            if (!allTypeBindings.TryGetValue(type, out var typeBindings))
            {
                return null;
            }

            if (filterBindingDelegate is null && typeBindings[0].FilterBindingDelegate is null)
            {
                return typeBindings[0];
            }

            bindingContext.InjectedIntoTypeBinding = injectedTypeBinding;

            if (filterBindingDelegate is null)
            {
                foreach (var typeBinding in typeBindings)
                {
                    bindingContext.TypeBinding = typeBinding;

                    if (typeBinding.FilterBindingDelegate?.Invoke(bindingContext) ?? true)
                    {
                        return typeBinding;
                    }
                }
            }
            else
            {
                foreach (var typeBinding in typeBindings)
                {
                    bindingContext.TypeBinding = typeBinding;

                    if (filterBindingDelegate.Invoke(bindingContext) && (typeBinding.FilterBindingDelegate?.Invoke(bindingContext) ?? true))
                    {
                        return typeBinding;
                    }
                }
            }
            
            return null;
        }

        public void ResolveAllContainer(Type type, FilterBindingDelegate? filterBindingDelegate, IList resolutions)
        {
            if (allTypeBindings.TryGetValue(type, out var typeBindings))
            {
                bindingContext.InjectedIntoTypeBinding = injectedTypeBinding;
            
                foreach (var typeBinding in typeBindings)
                {
                    bindingContext.TypeBinding = typeBinding;

                    if ((filterBindingDelegate?.Invoke(bindingContext) ?? true) && 
                        (typeBinding.FilterBindingDelegate?.Invoke(bindingContext) ?? true))
                    {
                        resolutions.Add(ResolveBinding(typeBinding));
                    }
                }
            }

            parentDiContainer?.ResolveAllContainer(type, filterBindingDelegate, resolutions);
        }

        public bool WouldResolveContainer(
            Type type, 
            FilterBindingDelegate? filterBindingDelegate,
            Type? overrideInjectedIntoType, 
            FilterBindingDelegate? overrideFilterBindingDelegate)
        {
            var previousInjectedTypeBinding = injectedTypeBinding;
            if (overrideInjectedIntoType is not null)
            {
                injectedTypeBinding = null;
                injectedTypeBinding = GetTypeForConstraint(overrideInjectedIntoType, overrideFilterBindingDelegate);
            }
            
            var typeBinding = GetTypeForConstraint(type, filterBindingDelegate);
            injectedTypeBinding = previousInjectedTypeBinding;
            if (typeBinding is not null)
            {
                return true;
            }

            if (parentDiContainer is null)
            {
                return false;
            }

            return parentDiContainer.WouldResolveContainer(type, filterBindingDelegate, overrideInjectedIntoType, 
                overrideFilterBindingDelegate);
        }
        
        public void QueueDispose(IDisposable disposable)
        {
            diContainerDisposer.QueueDispose(disposable);
        }
        
        public void QueueDispose(Action disposableAction)
        {
            diContainerDisposer.QueueDispose(disposableAction);
        }

        public void Dispose()
        {
            diContainerDisposer.Dispose();
        }
    }
}

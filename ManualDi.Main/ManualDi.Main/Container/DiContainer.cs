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
        
        private BindingInitializer bindingInitializer;
        private DisposableActionQueue disposableActionQueue;
        private bool isResolving;
        private bool disposedValue;
        private TypeBinding? injectedTypeBinding;

        public DiContainer(
            Dictionary<Type, List<TypeBinding>> allTypeBindings, 
            IDiContainer? parentDiContainer,
            int? initializationsCount = null, 
            int? initializationsOnDepthCount = null,
            int? disposablesCount = null)
        {
            bindingInitializer = new(initializationsCount, initializationsOnDepthCount);
            disposableActionQueue = new(disposablesCount);
            
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

            bool wasResolving = this.isResolving;
            isResolving = true;

            var previousType = injectedTypeBinding;
            injectedTypeBinding = typeBinding;
            var (instance, isNew) = typeBinding.Create(this);
            injectedTypeBinding = previousType;
            if (!isNew)
            {
                isResolving = wasResolving;
                return instance;
            }

            typeBinding.InjectObject(instance, this);
            if (typeBinding.TryToDispose && instance is IDisposable disposable)
            {
                QueueDispose(disposable);
            }
            bindingInitializer.Queue(typeBinding, instance);

            isResolving = wasResolving;

            if (!isResolving)
            {
                bindingInitializer.InitializeCurrentLevelQueued(this);
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

            var first = typeBindings[0];
            if (filterBindingDelegate is null && first.FilterBindingDelegate is null)
            {
                return first;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResolveAllContainer(Type type, FilterBindingDelegate? filterBindingDelegate, IList resolutions)
        {
            AddResolveAllInstances(type, filterBindingDelegate, resolutions);

            parentDiContainer?.ResolveAllContainer(type, filterBindingDelegate, resolutions);
        }

        public bool WouldResolveContainer(Type type, FilterBindingDelegate? filterBindingDelegate)
        {
            var typeBinding = GetTypeForConstraint(type, filterBindingDelegate);
            if (typeBinding is not null)
            {
                return true;
            }

            if (parentDiContainer is null)
            {
                return false;
            }

            return parentDiContainer.WouldResolveContainer(type, filterBindingDelegate);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddResolveAllInstances(Type type, FilterBindingDelegate? filterBindingDelegate, IList resolutions)
        {
            if (!this.allTypeBindings.TryGetValue(type, out var typeBindings))
            {
                return;
            }
            
            bindingContext.InjectedIntoTypeBinding = injectedTypeBinding;
            
            foreach (var typeBinding in typeBindings)
            {
                bindingContext.TypeBinding = typeBinding;

                if ((filterBindingDelegate?.Invoke(bindingContext) ?? true) && 
                    (typeBinding.FilterBindingDelegate?.Invoke(bindingContext) ?? true))
                {
                    var resolved = ResolveBinding(typeBinding);
                    resolutions.Add(resolved);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void QueueDispose(IDisposable disposable)
        {
            disposableActionQueue.QueueDispose(disposable);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void QueueDispose(Action disposableAction)
        {
            disposableActionQueue.QueueDispose(disposableAction);
        }

        public void Dispose()
        {
            if (disposedValue)
            {
                return;
            }
            disposedValue = true;

            disposableActionQueue.DisposeAll();
        }
    }
}

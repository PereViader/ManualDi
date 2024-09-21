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
        private readonly Stack<Type?> bindingStack = new();
        
        private BindingInitializer bindingInitializer;
        private DisposableActionQueue disposableActionQueue;
        private bool isResolving;
        private bool disposedValue;
        

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
            
            bindingStack.Push(null);
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

        public object? ResolveContainer(Type type, IsValidBindingDelegate? isValidBindingDelegate)
        {
            var typeBinding = GetTypeForConstraint(type, isValidBindingDelegate);
            if (typeBinding is not null)
            {
                return ResolveBinding(typeBinding);
            }

            if (parentDiContainer is not null)
            {
                return parentDiContainer.ResolveContainer(type, isValidBindingDelegate);
            }

            return null;
        }

        private object ResolveBinding(TypeBinding typeBinding)
        {

            bool wasResolving = this.isResolving;
            isResolving = true;

            bindingStack.Push(typeBinding.ConcreteType);
            var (instance, isNew) = typeBinding.Create(this);
            bindingStack.Pop();
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
        private TypeBinding? GetTypeForConstraint(Type type, IsValidBindingDelegate? isValidBindingDelegate)
        {
            if (!allTypeBindings.TryGetValue(type, out var typeBindings))
            {
                return null;
            }

            var first = typeBindings[0];
            if (isValidBindingDelegate is null && first.IsValidBindingDelegate is null)
            {
                return first;
            }

            bindingContext.InjectIntoType = bindingStack.Peek();

            if (isValidBindingDelegate is null)
            {
                foreach (var typeBinding in typeBindings)
                {
                    bindingContext.Id = typeBinding.Id;

                    if (typeBinding.IsValidBindingDelegate?.Invoke(bindingContext) ?? true)
                    {
                        return typeBinding;
                    }
                }
            }
            else
            {
                foreach (var typeBinding in typeBindings)
                {
                    bindingContext.Id = typeBinding.Id;

                    if (isValidBindingDelegate.Invoke(bindingContext) && (typeBinding.IsValidBindingDelegate?.Invoke(bindingContext) ?? true))
                    {
                        return typeBinding;
                    }
                }
            }
            
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResolveAllContainer(Type type, IsValidBindingDelegate? isValidBindingDelegate, IList resolutions)
        {
            AddResolveAllInstances(type, isValidBindingDelegate, resolutions);

            parentDiContainer?.ResolveAllContainer(type, isValidBindingDelegate, resolutions);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddResolveAllInstances(Type type, IsValidBindingDelegate? isValidBindingDelegate, IList resolutions)
        {
            if (!this.allTypeBindings.TryGetValue(type, out var typeBindings))
            {
                return;
            }
            
            bindingContext.InjectIntoType = bindingStack.Peek();
            
            foreach (var typeBinding in typeBindings)
            {
                bindingContext.Id = typeBinding.Id;

                if ((isValidBindingDelegate?.Invoke(bindingContext) ?? true) && 
                    (typeBinding.IsValidBindingDelegate?.Invoke(bindingContext) ?? true))
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

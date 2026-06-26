using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ManualDi.Sync
{
    public sealed class DiContainer : IDiContainer
    {
        private readonly Dictionary<IntPtr, BindingNode> allBindings;
        private readonly IDiContainer? parentDiContainer;
        private readonly BindingContext bindingContext;
        private readonly CancellationTokenSource cancellationTokenSource;
        
        private DiContainerInitializer diContainerInitializer;
        private DiContainerDisposer diContainerDisposer;
        private Binding? injectedBinding;
        
        public CancellationToken CancellationToken => cancellationTokenSource.Token;
        
        internal DiContainer(
            Dictionary<IntPtr, BindingNode> allBindings, 
            IDiContainer? parentDiContainer,
            BindingContext bindingContext,
            CancellationTokenSource cancellationTokenSource,
            int? initializationsCount = null, 
            int? disposablesCount = null)
        {
            diContainerInitializer = new(initializationsCount);
            diContainerDisposer = new(disposablesCount);

            this.allBindings = allBindings;
            this.parentDiContainer = parentDiContainer;
            this.bindingContext = bindingContext;
            this.cancellationTokenSource = cancellationTokenSource;
        }

        public void Initialize()
        {
            foreach (var node in allBindings.Values)
            {
                if (!node.Binding.IsTransient)
                {
                    ResolveBinding(node.Binding);
                }

                var current = node.Next;
                while (current is not null)
                {
                    if (!current.Binding.IsTransient)
                    {
                        ResolveBinding(current.Binding);
                    }
                    current = current.Next;
                }
            }
        }

        public object? ResolveContainer(Type type)
        {
            var binding = GetBinding(type);
            if (binding is not null)
            {
                return ResolveBinding(binding);
            }

            return parentDiContainer?.ResolveContainer(type);
        }
        
        public object? ResolveContainer(Type type, FilterBindingDelegate filterBindingDelegate)
        {
            var binding = GetBinding(type, filterBindingDelegate);
            if (binding is not null)
            {
                return ResolveBinding(binding);
            }

            return parentDiContainer?.ResolveContainer(type, filterBindingDelegate);
        }
        
        internal object ResolveBinding(Binding binding)
        {
            if (binding.Instance is not null) //Optimization: We don't check if Scope is Single
            {
                return binding.Instance;
            }
            
            var previousInjectedBinding = injectedBinding;
            injectedBinding = binding;

            if (injectedBinding.FromDelegate is null)
            {
                ThrowHelper.ThrowFromDelegateIsNull(injectedBinding.ConcreteType);
            }

            object? instance;
            if (injectedBinding.FromDelegate is FromDelegate fromDelegate)
            {
                instance = fromDelegate.Invoke(this);
                if (instance is null)
                {
                    ThrowHelper.ThrowCouldNotCreateObject(injectedBinding.ConcreteType);
                }
            }
            else
            {
                instance = injectedBinding.FromDelegate;
            }

            if (!binding.IsTransient)
            {
                binding.Instance = instance;
            }
            
            binding.InjectionDelegate?.Invoke(instance, this);
            if (binding.InitializationDelegate is not null)
            {
                diContainerInitializer.QueueInitialize(binding.InitializationDelegate, instance);
            }
            
            if (binding.TryToDispose && instance is IDisposable disposable)
            {
                QueueDispose(disposable);
            }

            injectedBinding = previousInjectedBinding;
            if (injectedBinding is null)
            {
                diContainerInitializer.InitializeCurrentLevelQueued(this);
            }

            return instance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Binding? GetBinding(Type type)
        {
            if (!allBindings.TryGetValue(type.TypeHandle.Value, out var node))
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
            if (!allBindings.TryGetValue(type.TypeHandle.Value, out var node))
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

        public void ResolveAllContainer(Type type, FilterBindingDelegate? filterBindingDelegate, IList resolutions)
        {
            if (allBindings.TryGetValue(type.TypeHandle.Value, out var node))
            {
                bindingContext.InjectedIntoBinding = injectedBinding;
                
                bindingContext.Binding = node.Binding;
                if ((filterBindingDelegate?.Invoke(bindingContext) ?? true) &&
                    (node.Binding.FilterBindingDelegate?.Invoke(bindingContext) ?? true))
                {
                    resolutions.Add(ResolveBinding(node.Binding));
                }

                var current = node.Next;
                while (current is not null)
                {
                    bindingContext.Binding = current.Binding;
                    if ((filterBindingDelegate?.Invoke(bindingContext) ?? true) &&
                        (current.Binding.FilterBindingDelegate?.Invoke(bindingContext) ?? true))
                    {
                        resolutions.Add(ResolveBinding(current.Binding));
                    }
                    current = current.Next;
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
            var previousInjectedBinding = injectedBinding;
            if (overrideInjectedIntoType is not null)
            {
                injectedBinding = null;
                injectedBinding = overrideFilterBindingDelegate is null 
                    ? GetBinding(overrideInjectedIntoType) 
                    : GetBinding(overrideInjectedIntoType, overrideFilterBindingDelegate);
            }
            
            var binding = filterBindingDelegate is null 
                ? GetBinding(type)
                : GetBinding(type, filterBindingDelegate);
            
            injectedBinding = previousInjectedBinding;
            if (binding is not null)
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
            if (diContainerDisposer.DisposedValue)
            {
                return;
            }
            
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            
            diContainerDisposer.Dispose();
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ManualDi.Sync
{
    public sealed class DiContainer : IDiContainer
    {
        private readonly Dictionary<IntPtr, Binding> allBindings;
        private readonly IDiContainer? parentDiContainer;
        private readonly BindingContext bindingContext = new();
        private readonly CancellationTokenSource cancellationTokenSource = new();
        
        private DiContainerInitializer diContainerInitializer;
        private DiContainerDisposer diContainerDisposer;
        private Binding? injectedBinding;
        
        public CancellationToken CancellationToken => cancellationTokenSource.Token;
        
        public DiContainer(
            Dictionary<IntPtr, Binding> allBindings, 
            IDiContainer? parentDiContainer,
            int? initializationsCount = null, 
            int? disposablesCount = null)
        {
            diContainerInitializer = new(initializationsCount);
            diContainerDisposer = new(disposablesCount);
            
            this.allBindings = allBindings;
            this.parentDiContainer = parentDiContainer;
        }

        public void Initialize()
        {
            foreach (var firstBinding in allBindings)
            {
                Binding? binding = firstBinding.Value;
                while (binding is not null)
                {
                    if (!binding.IsTransient)
                    {
                        ResolveBinding(binding);
                    }
                    binding = binding.NextBinding;
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

            var instance = injectedBinding.FromDelegate switch
            {
                FromDelegate fromDelegate => fromDelegate.Invoke(this) ??
                    throw new InvalidOperationException($"Could not create object for Binding with Concrete type {injectedBinding.ConcreteType}"),
                not null => injectedBinding.FromDelegate,
                null => throw new InvalidOperationException($"The from delegate for Binding with Concrete type {injectedBinding.ConcreteType} is null"),
            };

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
            if (!allBindings.TryGetValue(type.TypeHandle.Value, out Binding? binding))
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
            if (!allBindings.TryGetValue(type.TypeHandle.Value, out Binding? binding))
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

        public void ResolveAllContainer(Type type, FilterBindingDelegate? filterBindingDelegate, IList resolutions)
        {
            if (allBindings.TryGetValue(type.TypeHandle.Value, out Binding? binding))
            {
                bindingContext.InjectedIntoBinding = injectedBinding;
                do
                {
                    bindingContext.Binding = binding;

                    if ((filterBindingDelegate?.Invoke(bindingContext) ?? true) &&
                        (binding.FilterBindingDelegate?.Invoke(bindingContext) ?? true))
                    {
                        resolutions.Add(ResolveBinding(binding));
                    }

                    binding = binding.NextBinding;
                } while (binding is not null);
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

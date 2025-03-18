﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ManualDi.Main
{
    public sealed class DiContainer : IDiContainer, IDependencyResolver
    {
        private readonly Dictionary<IntPtr, Binding> bindingsByType;
        private readonly List<Binding> bindings;
        private readonly IDiContainer? parentDiContainer;
        private readonly BindingContext bindingContext = new();
        private Binding? injectedBinding;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly List<object> disposables;
        private bool disposedValue;
        
        public CancellationToken CancellationToken => cancellationTokenSource.Token;

        internal DiContainer(
            Dictionary<IntPtr, Binding> bindingsByType,
            int count,
            IDiContainer? parentDiContainer,
            CancellationToken cancellationToken,
            int? disposablesCount = null)
        {
            bindings = new (count);
            disposables = disposablesCount.HasValue ? new(disposablesCount.Value) : new();
            disposedValue = false;
            
            cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            
            this.bindingsByType = bindingsByType;
            this.parentDiContainer = parentDiContainer;
        }

        internal async ValueTask InitializeAsync()
        {
            SetupBindings();

            var ct = CancellationToken;

            var count = bindings.Count;
            for (int i = 0; i < count; i++)
            {
                injectedBinding = bindings[i];
                
                injectedBinding.Instance = injectedBinding.FromDelegate switch
                {
                    FromAsyncDelegate fromAsyncDelegate => await fromAsyncDelegate.Invoke(this, ct) ??
                                                           throw new InvalidOperationException(
                                                               $"Could not create object for Binding with Apparent type {injectedBinding.ApparentType} and Concrete type {injectedBinding.ConcreteType}"),
                    FromDelegate fromDelegate => fromDelegate.Invoke(this) ??
                                                 throw new InvalidOperationException(
                                                     $"Could not create object for Binding with Apparent type {injectedBinding.ApparentType} and Concrete type {injectedBinding.ConcreteType}"),
                    null => throw new InvalidOperationException($"The from delegate for Binding with Apparent type {injectedBinding.ApparentType} and Concrete type {injectedBinding.ConcreteType} is null"),
                    _ => throw new InvalidOperationException($"The from delegate for Binding with Apparent type {injectedBinding.ApparentType} and Concrete type {injectedBinding.ConcreteType} is of an unsupported type"),
                };
                
                if (injectedBinding.TryToDispose)
                {
                    switch (injectedBinding.Instance)
                    {
                        case IAsyncDisposable asyncDisposable:
                            QueueAsyncDispose(asyncDisposable);
                            break;
                        case IDisposable disposable:
                            QueueDispose(disposable);
                            break;
                    }
                }
            }

            for (int i = 0; i < count; i++)
            {
                injectedBinding = bindings[i];

                switch (injectedBinding.InjectionDelegate)
                {
                    case InjectAsyncDelegate injectAsyncDelegate:
                    {
                        await injectAsyncDelegate.Invoke(injectedBinding.Instance!, this, ct);
                        break;
                    }

                    case InjectDelegate injectDelegate:
                    {
                        injectDelegate.Invoke(injectedBinding.Instance!, this);
                        break;
                    }
                }
            }
            
            for (int i = 0; i < count; i++)
            {
                injectedBinding = bindings[i];
                
                switch (injectedBinding.InitializationDelegate)
                {
                    case InitializeAsyncDelegate initializeAsyncDelegate:
                    {
                        await initializeAsyncDelegate.Invoke(injectedBinding.Instance!, ct);
                        break;
                    }

                    case InitializeDelegate initializeDelegate:
                    {
                        initializeDelegate.Invoke(injectedBinding.Instance!);
                        break;
                    }
                }
            }
            
            injectedBinding = null;
        }

        private void SetupBindings()
        {
            foreach (var rootBinding in bindingsByType)
            {
                var binding = rootBinding.Value;
                while (binding is not null)
                {
                    if (!binding.IsAlreadyWired)
                    {
                        binding.IsAlreadyWired = true;
                        injectedBinding = binding;
                        binding.Dependencies?.Invoke(this);
                        bindings.Add(binding);
                    }
                    binding = binding.NextBinding;
                }
            }
        }
        
        public void Dependency<T>()
        {
            var binding = GetTypeForConstraint(typeof(T));
            if (binding is null)
            {
                throw new InvalidOperationException($"Type {typeof(T).FullName} injected into {injectedBinding?.GetType().FullName ?? "null"} is not registered.");
            }
            
            if (!binding.IsAlreadyWired)
            {
                binding.IsAlreadyWired = true;
                injectedBinding = binding;
                binding.Dependencies?.Invoke(this);
                bindings.Add(binding);
            }
        }

        public void Dependency<T>(FilterBindingDelegate filter)
        {
            var binding = GetTypeForConstraint(typeof(T), filter);
            if (binding is null)
            {
                throw new InvalidOperationException($"Type {typeof(T).FullName} injected into {injectedBinding?.GetType().FullName ?? "null"} with some filter is not registered.");
            }
            
            if (!binding.IsAlreadyWired)
            {
                binding.IsAlreadyWired = true;
                injectedBinding = binding;
                binding.Dependencies?.Invoke(this);
                bindings.Add(binding);
            }
        }

        public object? ResolveContainer(Type type)
        {
            var binding = GetTypeForConstraint(type);
            if (binding is not null)
            {
                return binding.Instance;
            }

            return parentDiContainer?.ResolveContainer(type);
        }
        
        public object? ResolveContainer(Type type, FilterBindingDelegate filterBindingDelegate)
        {
            var binding = GetTypeForConstraint(type, filterBindingDelegate);
            if (binding is not null)
            {
                return binding.Instance;
            }

            return parentDiContainer?.ResolveContainer(type, filterBindingDelegate);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Binding? GetTypeForConstraint(Type type)
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
        private Binding? GetTypeForConstraint(Type type, FilterBindingDelegate filterBindingDelegate)
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

        public void ResolveAllContainer(Type type, FilterBindingDelegate? filterBindingDelegate, IList resolutions)
        {
            if (bindingsByType.TryGetValue(type.TypeHandle.Value, out Binding? binding))
            {
                bindingContext.InjectedIntoBinding = injectedBinding;
                do
                {
                    bindingContext.Binding = binding;

                    if ((filterBindingDelegate?.Invoke(bindingContext) ?? true) &&
                        (binding.FilterBindingDelegate?.Invoke(bindingContext) ?? true))
                    {
                        resolutions.Add(binding.Instance);
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
                    ? GetTypeForConstraint(overrideInjectedIntoType) 
                    : GetTypeForConstraint(overrideInjectedIntoType, overrideFilterBindingDelegate);
            }
            
            var binding = filterBindingDelegate is null 
                ? GetTypeForConstraint(type)
                : GetTypeForConstraint(type, filterBindingDelegate);
            
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
            disposables.Add(disposable);
        }
        
        public void QueueDispose(Action disposableAction)
        {
            disposables.Add(disposableAction);
        }
        
        public void QueueAsyncDispose(Func<ValueTask> disposableFuncAsync)
        {
            disposables.Add(disposableFuncAsync);
        }
        
        public void QueueAsyncDispose(IAsyncDisposable asyncDisposable)
        {
            disposables.Add(asyncDisposable);
        }
        
        public async ValueTask DisposeAsync()
        {
            if (disposedValue)
            {
                return;
            }
            
            disposedValue = true;

            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            
            foreach (var disposable in disposables)
            {
                switch (disposable)
                {
                    case IDisposable disposableDisposable:
                        disposableDisposable.Dispose();
                        break;
                    case IAsyncDisposable asyncDisposableDisposable:
                        await asyncDisposableDisposable.DisposeAsync();
                        break;
                    case Action disposableAction:
                        disposableAction();
                        break;
                    case Func<ValueTask> disposableFuncAsync:
                        await disposableFuncAsync.Invoke();
                        break;
                    default:
                        throw new SwitchExpressionException(disposable);
                }
            }

            disposables.Clear();
        }
    }
}

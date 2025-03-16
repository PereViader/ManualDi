using System;
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
        private readonly Dictionary<IntPtr, TypeBinding> bindingsByType;
        private readonly List<TypeBinding> bindings = new();
        private readonly IDiContainer? parentDiContainer;
        private readonly BindingContext bindingContext = new();
        private readonly CancellationTokenSource _cancellationTokenSource;
        internal DiContainerDisposer diContainerDisposer; //Optimization: ref struct. Can't be readonly
        
        internal TypeBinding? injectedTypeBinding;
        
        public CancellationToken CancellationToken => _cancellationTokenSource.Token;

        internal DiContainer(
            Dictionary<IntPtr, TypeBinding> bindingsByType,
            IDiContainer? parentDiContainer,
            CancellationToken cancellationToken,
            int? disposablesCount = null)
        {
            diContainerDisposer = new(disposablesCount);
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            
            this.bindingsByType = bindingsByType;
            this.parentDiContainer = parentDiContainer;
        }

        internal async Task InitializeAsync()
        {
            SetupBindings();

            var count = bindings.Count;
            for (int i = 0; i < count; i++)
            {
                injectedTypeBinding = bindings[i];
                
                switch (bindings[i])
                {
                    case ITypeBindingAsyncSetup asyncSetup:
                    {
                        await asyncSetup.CreateAsync(this);
                        break;
                    }

                    case ITypeBindingSyncSetup syncSetup:
                    {
                        syncSetup.Create(this);
                        break;
                    }
                }
            }

            for (int i = 0; i < count; i++)
            {
                injectedTypeBinding = bindings[i];

                switch (bindings[i])
                {
                    case ITypeBindingAsyncSetup asyncSetup:
                    {
                        await asyncSetup.InjectAsync(this);
                        break;
                    }

                    case ITypeBindingSyncSetup syncSetup:
                    {
                        syncSetup.Inject(this);
                        break;
                    }
                }
            }
            
            injectedTypeBinding = null;
            
            var ct = CancellationToken;
            for (int i = 0; i < count; i++)
            {
                switch (bindings[i])
                {
                    case ITypeBindingAsyncSetup asyncSetup:
                    {
                        await asyncSetup.InitializeAsync(ct);
                        break;
                    }

                    case ITypeBindingSyncSetup syncSetup:
                    {
                        syncSetup.Initialize();
                        break;
                    }
                }
            }
        }

        private void SetupBindings()
        {
            foreach (var binding in bindingsByType.Values)
            {
                var iterationBinding = binding;
                while (iterationBinding is not null)
                {
                    if (!iterationBinding.IsAlreadyWired)
                    {
                        binding.IsAlreadyWired = true;
                        injectedTypeBinding = binding;
                        binding.Dependencies?.Invoke(this);
                        bindings.Add(iterationBinding);
                    }
                    iterationBinding = iterationBinding.NextTypeBinding;
                }
            }
        }
        
        public void Dependency<T>()
        {
            var binding = GetTypeForConstraint(typeof(T));
            if (binding is null)
            {
                throw new InvalidOperationException($"Type {typeof(T).FullName} injected into {injectedTypeBinding?.GetType().FullName ?? "null"} is not registered.");
            }
            
            if (!binding.IsAlreadyWired)
            {
                binding.IsAlreadyWired = true;
                injectedTypeBinding = binding;
                binding.Dependencies?.Invoke(this);
                bindings.Add(binding);
            }
        }

        public void Dependency<T>(FilterBindingDelegate filter)
        {
            var binding = GetTypeForConstraint(typeof(T), filter);
            if (binding is null)
            {
                throw new InvalidOperationException($"Type {typeof(T).FullName} injected into {injectedTypeBinding?.GetType().FullName ?? "null"} with some filter is not registered.");
            }
            
            if (!binding.IsAlreadyWired)
            {
                binding.IsAlreadyWired = true;
                injectedTypeBinding = binding;
                binding.Dependencies?.Invoke(this);
                bindings.Add(binding);
            }
        }

        public object? ResolveContainer(Type type)
        {
            var typeBinding = GetTypeForConstraint(type);
            if (typeBinding is not null)
            {
                return typeBinding.Instance;
            }

            return parentDiContainer?.ResolveContainer(type);
        }
        
        public object? ResolveContainer(Type type, FilterBindingDelegate filterBindingDelegate)
        {
            var typeBinding = GetTypeForConstraint(type, filterBindingDelegate);
            if (typeBinding is not null)
            {
                return typeBinding.Instance;
            }

            return parentDiContainer?.ResolveContainer(type, filterBindingDelegate);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TypeBinding? GetTypeForConstraint(Type type)
        {
            if (!bindingsByType.TryGetValue(type.TypeHandle.Value, out TypeBinding? typeBinding))
            {
                return null;
            }

            if (typeBinding.FilterBindingDelegate is null)
            {
                return typeBinding;
            }

            bindingContext.InjectedIntoTypeBinding = injectedTypeBinding;
            do
            {
                bindingContext.TypeBinding = typeBinding;

                if (typeBinding.FilterBindingDelegate?.Invoke(bindingContext) ?? true)
                {
                    return typeBinding;
                }

                typeBinding = typeBinding.NextTypeBinding;
            } while (typeBinding is not null);
            
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TypeBinding? GetTypeForConstraint(Type type, FilterBindingDelegate filterBindingDelegate)
        {
            if (!bindingsByType.TryGetValue(type.TypeHandle.Value, out TypeBinding? typeBinding))
            {
                return null;
            }

            bindingContext.InjectedIntoTypeBinding = injectedTypeBinding;
            do
            {
                bindingContext.TypeBinding = typeBinding;

                if (filterBindingDelegate.Invoke(bindingContext) &&
                    (typeBinding.FilterBindingDelegate?.Invoke(bindingContext) ?? true))
                {
                    return typeBinding;
                }

                typeBinding = typeBinding.NextTypeBinding;
            } while (typeBinding is not null);
            
            return null;
        }

        public void ResolveAllContainer(Type type, FilterBindingDelegate? filterBindingDelegate, IList resolutions)
        {
            if (bindingsByType.TryGetValue(type.TypeHandle.Value, out TypeBinding? typeBinding))
            {
                bindingContext.InjectedIntoTypeBinding = injectedTypeBinding;
                do
                {
                    bindingContext.TypeBinding = typeBinding;

                    if ((filterBindingDelegate?.Invoke(bindingContext) ?? true) &&
                        (typeBinding.FilterBindingDelegate?.Invoke(bindingContext) ?? true))
                    {
                        resolutions.Add(typeBinding.Instance);
                    }

                    typeBinding = typeBinding.NextTypeBinding;
                } while (typeBinding is not null);
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
                injectedTypeBinding = overrideFilterBindingDelegate is null 
                    ? GetTypeForConstraint(overrideInjectedIntoType) 
                    : GetTypeForConstraint(overrideInjectedIntoType, overrideFilterBindingDelegate);
            }
            
            var typeBinding = filterBindingDelegate is null 
                ? GetTypeForConstraint(type)
                : GetTypeForConstraint(type, filterBindingDelegate);
            
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
        
        public void QueueAsyncDispose(Func<ValueTask> disposableFuncAsync)
        {
            diContainerDisposer.QueueAsyncDispose(disposableFuncAsync);
        }
        
        public void QueueAsyncDispose(IAsyncDisposable asyncDisposable)
        {
            diContainerDisposer.QueueAsyncDispose(asyncDisposable);
        }
        
        public async ValueTask DisposeAsync()
        {
            if (diContainerDisposer.DisposedValue)
            {
                return;
            }
            
            diContainerDisposer.DisposedValue = true;

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            
            await Task.WhenAll(diContainerDisposer.AsyncDisposables.Select(x => x.DisposeAsync().AsTask()));
            
            foreach (var disposable in diContainerDisposer.AsyncDisposables)
            {
                await disposable.DisposeAsync();
            }
            
            diContainerDisposer.AsyncDisposables.Clear();
            
            foreach (var disposable in diContainerDisposer.Disposables)
            {
                disposable.Dispose();
            }

            diContainerDisposer.Disposables.Clear();
        }
    }
}

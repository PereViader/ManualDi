using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ManualDi.Main
{
    public sealed class DiContainer : IDiContainer
    {
        private readonly Dictionary<IntPtr, TypeBinding> allTypeBindings;
        private readonly IDiContainer? parentDiContainer;
        private readonly BindingContext bindingContext = new();
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        internal DiContainerInitializer diContainerInitializer; //Optimization: ref struct. Can't be readonly
        internal DiContainerDisposer diContainerDisposer; //Optimization: ref struct. Can't be readonly
        
        internal TypeBinding? injectedTypeBinding;
        
        public CancellationToken CancellationToken => _cancellationTokenSource.Token;

        public DiContainer(
            Dictionary<IntPtr, TypeBinding> allTypeBindings, 
            IDiContainer? parentDiContainer,
            int? initializationsCount = null, 
            int? disposablesCount = null)
        {
            diContainerInitializer = new(initializationsCount);
            diContainerDisposer = new(disposablesCount);
            
            this.allTypeBindings = allTypeBindings;
            this.parentDiContainer = parentDiContainer;
        }

        public void Initialize()
        {
            foreach (var firstTypeBinding in allTypeBindings)
            {
                TypeBinding? typeBinding = firstTypeBinding.Value;
                while (typeBinding is not null)
                {
                    if (!typeBinding.IsLazy)
                    {
                        typeBinding.Resolve(this);
                    }

                    typeBinding = typeBinding.NextTypeBinding;
                }
            }
        }

        public object? ResolveContainer(Type type)
        {
            var typeBinding = GetTypeForConstraint(type);
            if (typeBinding is not null)
            {
                return typeBinding.Resolve(this);;
            }

            return parentDiContainer?.ResolveContainer(type);
        }
        
        public object? ResolveContainer(Type type, FilterBindingDelegate filterBindingDelegate)
        {
            var typeBinding = GetTypeForConstraint(type, filterBindingDelegate);
            if (typeBinding is not null)
            {
                return typeBinding.Resolve(this);
            }

            return parentDiContainer?.ResolveContainer(type, filterBindingDelegate);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TypeBinding? GetTypeForConstraint(Type type)
        {
            if (!allTypeBindings.TryGetValue(type.TypeHandle.Value, out TypeBinding? typeBinding))
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
            if (!allTypeBindings.TryGetValue(type.TypeHandle.Value, out TypeBinding? typeBinding))
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
            if (allTypeBindings.TryGetValue(type.TypeHandle.Value, out TypeBinding? typeBinding))
            {
                bindingContext.InjectedIntoTypeBinding = injectedTypeBinding;
                do
                {
                    bindingContext.TypeBinding = typeBinding;

                    if ((filterBindingDelegate?.Invoke(bindingContext) ?? true) &&
                        (typeBinding.FilterBindingDelegate?.Invoke(bindingContext) ?? true))
                    {
                        resolutions.Add(typeBinding.Resolve(this));
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

        public void Dispose()
        {
            if (diContainerDisposer.DisposedValue)
            {
                return;
            }

            if (diContainerDisposer.AsyncDisposables.Count > 0)
            {
                throw new InvalidOperationException("Trying to Dispose of DiContainer but there are IAsyncDisposables registered. Use DisposeAsync instead.");
            }
            
            diContainerDisposer.DisposedValue = true;
            
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            
            foreach (var disposable in diContainerDisposer.Disposables)
            {
                disposable.Dispose();
            }

            diContainerDisposer.Disposables.Clear();
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

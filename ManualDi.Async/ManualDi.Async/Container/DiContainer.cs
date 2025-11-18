using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ManualDi.Async
{
    public sealed class DiContainer : IDiContainer
    {
        public const string FailureDebugReportKey = "ManualDi.FailureDebugReport";

        private readonly Dictionary<IntPtr, Binding> bindingsByType;
        private readonly List<Binding> bindings;
        private readonly IDiContainer? parentDiContainer;
        private readonly BindingContext bindingContext = new();
        private Binding? injectedBinding;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly List<object> disposables;
        private HashSet<Binding>? injectBindings;
        private bool disposedValue;
        
        public CancellationToken CancellationToken => cancellationTokenSource.Token;

        internal DiContainer(
            Dictionary<IntPtr, Binding> bindingsByType,
            int count,
            IDiContainer? parentDiContainer,
            List<object> disposables,
            CancellationToken cancellationToken)
        {
            bindings = new (count);
            this.disposables = disposables;
            disposedValue = false;
            
            cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            
            this.bindingsByType = bindingsByType;
            this.parentDiContainer = parentDiContainer;
            SetupBindings();
        }
        
        public string GetFailureDebugReport()
        {
            var stringBuilder = new StringBuilder();
            foreach (var binding in bindings)
            {
                stringBuilder.AppendLine($"Concrete: {binding.ConcreteType}, Id: {binding.Id}");
            }
            
            return stringBuilder.ToString();
        }
        
        internal async ValueTask InitializeCreate()
        {
            var ct = CancellationToken;

            var count = bindings.Count;
            for (int i = 0; i < count; i++)
            {
                injectedBinding = bindings[i];
                
                injectedBinding.Instance = injectedBinding.FromDelegate switch
                {
                    FromAsyncDelegate fromAsyncDelegate => await fromAsyncDelegate.Invoke(this, ct) ??
                                                           throw new InvalidOperationException(
                                                               $"Could not create object for Binding with Concrete type {injectedBinding.ConcreteType}"),
                    FromDelegate fromDelegate => fromDelegate.Invoke(this) ??
                                                 throw new InvalidOperationException(
                                                     $"Could not create object for Binding with Concrete type {injectedBinding.ConcreteType}"),
                    not null => injectedBinding.FromDelegate,
                    null => throw new InvalidOperationException($"The from delegate for Binding with Concrete type {injectedBinding.ConcreteType} is null"),
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
        }

        internal async ValueTask InitializeInject()
        {
            var ct = CancellationToken;
            var count = bindings.Count;
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
        }

        internal async ValueTask IntiailizeInitialize()
        {
            var ct = CancellationToken;
            var count = bindings.Count;
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
            injectBindings = new();
            
            foreach (var rootBinding in bindingsByType)
            {
                var binding = rootBinding.Value;
                while (binding is not null)
                {
                    if (binding.BindingWiredState < BindingWiredState.Wired)
                    {
                        binding.BindingWiredState = BindingWiredState.Wired;
                        injectedBinding = binding;
                        binding.Dependencies?.Invoke(this);
                        bindings.Add(binding);
                    }
                    binding = binding.NextBinding;
                }
            }

            foreach (var binding in injectBindings)
            {
                if (binding.BindingWiredState is BindingWiredState.Wired)
                {
                    continue;
                }
                
                bindings.Add(binding);
            }

            injectBindings = null;
        }
        
        public void ConstructorDependency<T>()
        {
            var binding = GetBinding(typeof(T));
            if (binding is null)
            {
                if (parentDiContainer is null)
                {
                    throw new InvalidOperationException($"Type {typeof(T).FullName} injected into {injectedBinding?.ConcreteType.FullName ?? "null"} is not registered.");
                }
                parentDiContainer.ConstructorDependency<T>();
                return;
            }
            
            if (binding.BindingWiredState < BindingWiredState.Wired)
            {
                binding.BindingWiredState = BindingWiredState.Wired;
                var previousInjectedBinding = injectedBinding;
                injectedBinding = binding;
                binding.Dependencies?.Invoke(this);
                injectedBinding = previousInjectedBinding;
                bindings.Add(binding);
            }
        }

        public void ConstructorDependency<T>(FilterBindingDelegate filter)
        {
            var binding = GetBinding(typeof(T), filter);
            if (binding is null)
            {
                if (parentDiContainer is null)
                {
                    throw new InvalidOperationException($"Type {typeof(T).FullName} injected into {injectedBinding?.ConcreteType.FullName ?? "null"} with some filter is not registered.");
                }
                parentDiContainer.ConstructorDependency<T>(filter);
                return;
            }
            
            if (binding.BindingWiredState < BindingWiredState.Wired)
            {
                binding.BindingWiredState = BindingWiredState.Wired;
                var previousInjectedBinding = injectedBinding;
                injectedBinding = binding;
                binding.Dependencies?.Invoke(this);
                injectedBinding = previousInjectedBinding;
                bindings.Add(binding);
            }
        }
        
        public void NullableConstructorDependency<T>()
        {
            var binding = GetBinding(typeof(T));
            if (binding is null)
            {
                parentDiContainer?.NullableConstructorDependency<T>();
                return;
            }
            
            if (binding.BindingWiredState < BindingWiredState.Wired)
            {
                binding.BindingWiredState = BindingWiredState.Wired;
                var previousInjectedBinding = injectedBinding;
                injectedBinding = binding;
                binding.Dependencies?.Invoke(this);
                injectedBinding = previousInjectedBinding;
                bindings.Add(binding);
            }
        }

        public void NullableConstructorDependency<T>(FilterBindingDelegate filter)
        {
            var binding = GetBinding(typeof(T), filter);
            if (binding is null)
            {
                parentDiContainer?.NullableConstructorDependency<T>(filter);
                return;
            }
            
            if (binding.BindingWiredState < BindingWiredState.Wired)
            {
                binding.BindingWiredState = BindingWiredState.Wired;
                var previousInjectedBinding = injectedBinding;
                injectedBinding = binding;
                binding.Dependencies?.Invoke(this);
                injectedBinding = previousInjectedBinding;
                bindings.Add(binding);
            }
        }   

        public void InjectionDependency<T>()
        {
            var binding = GetBinding(typeof(T));
            if (binding is null)
            {
                if (parentDiContainer is null)
                {
                    throw new InvalidOperationException($"Type {typeof(T).FullName} injected into {injectedBinding?.ConcreteType.FullName ?? "null"} is not registered.");
                }
                parentDiContainer.InjectionDependency<T>();
                return;
            }
            
            if (binding.BindingWiredState is BindingWiredState.Wired)
            {
                return;
            }
            
            binding.BindingWiredState = BindingWiredState.Injected;
            injectBindings!.Add(binding);
        }

        public void InjectionDependency<T>(FilterBindingDelegate filter)
        {
            var binding = GetBinding(typeof(T), filter);
            if (binding is null)
            {
                if (parentDiContainer is null)
                {
                    throw new InvalidOperationException($"Type {typeof(T).FullName} injected into {injectedBinding?.ConcreteType.FullName ?? "null"} is not registered.");
                }
                parentDiContainer.InjectionDependency<T>(filter);
                return;
            }
            
            if (binding.BindingWiredState is BindingWiredState.Wired)
            {
                return;
            }
            
            binding.BindingWiredState = BindingWiredState.Injected;
            injectBindings!.Add(binding);
        }
        
        public void NullableInjectionDependency<T>()
        {
            var binding = GetBinding(typeof(T));
            if (binding is null)
            {
                parentDiContainer?.NullableInjectionDependency<T>();
                return;
            }
            
            if (binding.BindingWiredState is BindingWiredState.Wired)
            {
                return;
            }
            
            binding.BindingWiredState = BindingWiredState.Injected;
            injectBindings!.Add(binding);
        }

        public void NullableInjectionDependency<T>(FilterBindingDelegate filter)
        {
            var binding = GetBinding(typeof(T), filter);
            if (binding is null)
            {
                parentDiContainer?.NullableInjectionDependency<T>(filter);
                return;
            }
            
            if (binding.BindingWiredState is BindingWiredState.Wired)
            {
                return;
            }
            
            binding.BindingWiredState = BindingWiredState.Injected;
            injectBindings!.Add(binding);
        }

        public object? ResolveContainer(Type type)
        {
            var binding = GetBinding(type);
            if (binding is not null)
            {
                return binding.Instance;
            }

            return parentDiContainer?.ResolveContainer(type);
        }
        
        public object? ResolveContainer(Type type, FilterBindingDelegate filterBindingDelegate)
        {
            var binding = GetBinding(type, filterBindingDelegate);
            if (binding is not null)
            {
                return binding.Instance;
            }

            return parentDiContainer?.ResolveContainer(type, filterBindingDelegate);
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

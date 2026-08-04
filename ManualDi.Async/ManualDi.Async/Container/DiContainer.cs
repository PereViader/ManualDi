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

        private readonly Dictionary<IntPtr, BindingNode> bindingsByType;
        private readonly List<Binding> bindings;
        private readonly IDiContainer? parentDiContainer;
        private Binding? injectedBinding;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly List<object> disposables;
        private bool disposedValue;
        
        public CancellationToken CancellationToken => cancellationTokenSource.Token;
        public Binding? InjectedBinding => injectedBinding;

        internal DiContainer(
            Dictionary<IntPtr, BindingNode> bindingsByType,
            int count,
            IDiContainer? parentDiContainer,
            List<object> disposables,
            CancellationTokenSource cancellationTokenSource)
        {
            bindings = new (count);
            this.disposables = disposables;
            disposedValue = false;
            
            this.cancellationTokenSource = cancellationTokenSource;
            
            this.bindingsByType = bindingsByType;
            this.parentDiContainer = parentDiContainer;
            SetupBindings();
        }
        
        public string GetFailureDebugReport()
        {
            var stringBuilder = new StringBuilder();
            foreach (var binding in bindings)
            {
                stringBuilder.AppendLine($"Concrete: {binding.ConcreteType}");
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
                
                if (injectedBinding.FromDelegate is null)
                {
                    ThrowHelper.ThrowFromDelegateIsNull(injectedBinding);
                }

                object? instance;
                if (injectedBinding.FromDelegate is FromAsyncDelegate fromAsyncDelegate)
                {
                    instance = await fromAsyncDelegate.Invoke(this, ct);
                    if (instance is null)
                    {
                        ThrowHelper.ThrowCouldNotCreateObject(injectedBinding);
                    }
                }
                else if (injectedBinding.FromDelegate is FromDelegate fromDelegate)
                {
                    instance = fromDelegate.Invoke(this);
                    if (instance is null)
                    {
                        ThrowHelper.ThrowCouldNotCreateObject(injectedBinding);
                    }
                }
                else
                {
                    instance = injectedBinding.FromDelegate;
                }

                injectedBinding.Instance = instance;
                
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
            foreach (var node in bindingsByType.Values)
            {
                var binding = node.Binding;
                if (!binding.IsWired)
                {
                    binding.IsWired = true;
                    injectedBinding = binding;
                    binding.Dependencies?.Invoke(this);
                    bindings.Add(binding);
                }

                var current = node.Next;
                while (current is not null)
                {
                    binding = current.Binding;
                    if (!binding.IsWired)
                    {
                        binding.IsWired = true;
                        injectedBinding = binding;
                        binding.Dependencies?.Invoke(this);
                        bindings.Add(binding);
                    }
                    current = current.Next;
                }
            }
        }
        
        public void ConstructorDependency<T>()
        {
            var binding = GetBinding(typeof(T));
            if (binding is null)
            {
                if (parentDiContainer is null)
                {
                    ThrowHelper.ThrowTypeNotRegistered(typeof(T), injectedBinding);
                }
                parentDiContainer.ConstructorDependency<T>();
                return;
            }
            
            if (!binding.IsWired)
            {
                binding.IsWired = true;
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
            
            if (!binding.IsWired)
            {
                binding.IsWired = true;
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
                    ThrowHelper.ThrowTypeNotRegistered(typeof(T), injectedBinding);
                }
                parentDiContainer.InjectionDependency<T>();
                return;
            }
        }
        
        public void NullableInjectionDependency<T>()
        {
            var binding = GetBinding(typeof(T));
            if (binding is null)
            {
                parentDiContainer?.NullableInjectionDependency<T>();
                return;
            }
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Binding? GetBinding(Type type)
        {
            if (!bindingsByType.TryGetValue(type.TypeHandle.Value, out var node))
            {
                return null;
            }

            return node.Binding;
        }

        public void ResolveAllContainer(Type type, IList resolutions)
        {
            if (bindingsByType.TryGetValue(type.TypeHandle.Value, out var node))
            {
                resolutions.Add(node.Binding.Instance);

                var current = node.Next;
                while (current is not null)
                {
                    resolutions.Add(current.Binding.Instance);
                    current = current.Next;
                }
            }

            parentDiContainer?.ResolveAllContainer(type, resolutions);
        }

        public bool WouldResolveContainer(Type type)
        {
            var binding = GetBinding(type);
            if (binding is not null)
            {
                return true;
            }

            if (parentDiContainer is null)
            {
                return false;
            }

            return parentDiContainer.WouldResolveContainer(type);
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

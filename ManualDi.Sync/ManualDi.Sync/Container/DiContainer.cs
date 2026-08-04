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
        private readonly CancellationTokenSource cancellationTokenSource;
        
        private DiContainerInitializer diContainerInitializer;
        private DiContainerDisposer diContainerDisposer;
        private Binding? injectedBinding;
        
        public CancellationToken CancellationToken => cancellationTokenSource.Token;
        public Binding? InjectedBinding => injectedBinding;
        
        internal DiContainer(
            Dictionary<IntPtr, BindingNode> allBindings, 
            IDiContainer? parentDiContainer,
            CancellationTokenSource cancellationTokenSource,
            int? initializationsCount = null, 
            int? disposablesCount = null)
        {
            diContainerInitializer = new(initializationsCount);
            diContainerDisposer = new(disposablesCount);

            this.allBindings = allBindings;
            this.parentDiContainer = parentDiContainer;
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
                ThrowHelper.ThrowFromDelegateIsNull(injectedBinding);
            }

            object? instance;
            if (injectedBinding.FromDelegate is FromDelegate fromDelegate)
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

            return node.Binding;
        }

        public void ResolveAllContainer(Type type, IList resolutions)
        {
            if (allBindings.TryGetValue(type.TypeHandle.Value, out var node))
            {
                resolutions.Add(ResolveBinding(node.Binding));

                var current = node.Next;
                while (current is not null)
                {
                    resolutions.Add(ResolveBinding(current.Binding));
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

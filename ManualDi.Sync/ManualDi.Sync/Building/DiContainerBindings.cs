using System;
using System.Collections.Generic;
using System.Threading;

namespace ManualDi.Sync
{
    public delegate void ContainerDelegate(IDiContainer diContainer);
    
    public sealed class DiContainerBindings
    {
        internal readonly Dictionary<IntPtr, BindingNode> bindingsByType;
        private readonly List<ContainerDelegate> injectDelegates;
        private readonly List<ContainerDelegate> initializationDelegates;
        private readonly List<ContainerDelegate> startupDelegates;
        private readonly List<Action> disposeActions;
        private readonly int? containerInitializationsCount;
        private readonly int? containerDisposablesCount;
        
        internal readonly BindingContext bindingContext = new();

        internal IDiContainer? parentDiContainer;
        internal DiContainerBindings? parentDiContainerBindings;
        private CancellationTokenSource? cancellationTokenSource;

        public DiContainerBindings(
            int? bindingsCapacity = null, 
            int? injectCapacity = null,
            int? initializationCapacity = null,
            int? disposeCapacity = null,
            int? startupCapacity = null,
            int? containerInitializationsCount = null, 
            int? containerDisposablesCount = null
            )
        {
            bindingsByType = bindingsCapacity.HasValue ? new(bindingsCapacity.Value) : new();
            injectDelegates = injectCapacity.HasValue ? new(injectCapacity.Value) : new();
            initializationDelegates = initializationCapacity.HasValue ? new(initializationCapacity.Value) : new();
            disposeActions = disposeCapacity.HasValue ? new(disposeCapacity.Value) : new();
            startupDelegates = startupCapacity.HasValue ? new(startupCapacity.Value) : new();
            this.containerInitializationsCount = containerInitializationsCount;
            this.containerDisposablesCount = containerDisposablesCount;
        }
        
        internal void AddBinding(Binding binding, Type type)
        {
            var typeIntPtr = type.TypeHandle.Value;
#if NETSTANDARD2_1 //TryAdd is not available on netstandard2.0
            if (bindingsByType.TryAdd(typeIntPtr, new BindingNode(binding)))
            {
                return;
            }
            var node = bindingsByType[typeIntPtr];
#elif NETSTANDARD2_0
            if (!bindingsByType.TryGetValue(typeIntPtr, out var node))
            {
                bindingsByType.Add(typeIntPtr, new BindingNode(binding));
                return;
            }
#endif
            if (node.Next is null)
            {
                var nextChainNode = new BindingChainNode(binding);
                bindingsByType[typeIntPtr] = new BindingNode(node.Binding, nextChainNode);
            }
            else
            {
                var current = node.Next;
                while (current.Next is not null)
                {
                    current = current.Next;
                }
                current.Next = new BindingChainNode(binding);
            }
        }

        internal Binding? GetBinding(Type type)
        {
            var typeIntPtr = type.TypeHandle.Value;
            if (!bindingsByType.TryGetValue(typeIntPtr, out var node))
            {
                return null;
            }
            return node.Binding;
        }
        
        internal bool RemoveBinding(Type type)
        {
            var typeIntPtr = type.TypeHandle.Value;
            return bindingsByType.Remove(typeIntPtr);
        }
        
        public void QueueInjection(ContainerDelegate containerDelegate)
        {
            injectDelegates.Add(containerDelegate);
        }

        public void QueueInitialization(ContainerDelegate containerDelegate)
        {
            initializationDelegates.Add(containerDelegate);
        }
        
        public void QueueStartup(ContainerDelegate containerDelegate)
        {
            startupDelegates.Add(containerDelegate);
        }
        
        public void QueueDispose(Action action)
        {
            disposeActions.Add(action);
        }
        
        public DiContainerBindings WithParentContainer(IDiContainer? diContainer)
        {
            if (parentDiContainerBindings is not null)
            {
                throw new InvalidOperationException($"Can't have both parent DiContainer and DiContainerBindings");
            }
            parentDiContainer = diContainer;
            return this;
        }
        
        public DiContainerBindings WithParentBindings(DiContainerBindings? diContainerBindings)
        {
            if (parentDiContainer is not null)
            {
                throw new InvalidOperationException($"Can't have both parent DiContainer and DiContainerBindings");
            }
            parentDiContainerBindings = diContainerBindings;
            return this;
        }

        public DiContainerBindings WithCancellationTokenSource(CancellationTokenSource cancellationTokenSource)
        {
            this.cancellationTokenSource = cancellationTokenSource;
            return this;
        }
        
        public IDiContainer Build()
        {
            var diContainer = new DiContainer(
                bindingsByType,
                parentDiContainer,
                bindingContext,
                cancellationTokenSource ?? new CancellationTokenSource(),
                containerInitializationsCount,
                containerDisposablesCount);

            try
            {
                if (disposeActions.Count > 0)
                {
                    diContainer.QueueDispose(new ActionDisposableWrapper(() =>
                    {
                        foreach (var action in disposeActions)
                        {
                            action.Invoke();
                        }
                    }));
                }

                diContainer.Initialize();

                foreach (var injectDelegate in injectDelegates)
                {
                    injectDelegate.Invoke(diContainer);
                }

                foreach (var initializationDelegate in initializationDelegates)
                {
                    initializationDelegate.Invoke(diContainer);
                }

                foreach (var startupDelegate in startupDelegates)
                {
                    startupDelegate.Invoke(diContainer);
                }

                return diContainer;
            }
            catch (Exception)
            {
                diContainer.Dispose();
                throw;
            }
        }
    }
}

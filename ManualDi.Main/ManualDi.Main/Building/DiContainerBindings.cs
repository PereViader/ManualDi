﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ManualDi.Main
{
    public delegate void ContainerDelegate(IDiContainer diContainer);
    
    public sealed class DiContainerBindings
    {
        private readonly Dictionary<IntPtr, TypeBinding> bindingsByType;
        private readonly List<ContainerDelegate> injectDelegates;
        private readonly List<ContainerDelegate> initializationDelegates;
        private readonly List<ContainerDelegate> startupDelegates;
        private readonly List<Action> disposeActions;
        private readonly int? containerDisposablesCount;
        
        int bindingCount;

        private IDiContainer? parentDiContainer;

        public DiContainerBindings(
            int? bindingsCapacity = null, 
            int? injectCapacity = null,
            int? initializationCapacity = null,
            int? disposeCapacity = null,
            int? startupCapacity = null,
            int? containerDisposablesCount = null
            )
        {
            bindingsByType = bindingsCapacity.HasValue ? new(bindingsCapacity.Value) : new();
            injectDelegates = injectCapacity.HasValue ? new(injectCapacity.Value) : new();
            initializationDelegates = initializationCapacity.HasValue ? new(initializationCapacity.Value) : new();
            disposeActions = disposeCapacity.HasValue ? new(disposeCapacity.Value) : new();
            startupDelegates = startupCapacity.HasValue ? new(startupCapacity.Value) : new();
            this.containerDisposablesCount = containerDisposablesCount;
        }
        
        internal void AddBinding(TypeBinding typeBinding, Type type)
        {
            bindingCount++;
            
            var apparentType = type.TypeHandle.Value;
            if (!bindingsByType.TryGetValue(apparentType, out var innerTypeBinding)) //TODO: Maybe this is more efficient if we do a TryAdd instead (common case)
            {
                bindingsByType.Add(apparentType, typeBinding);
                return;
            }

            while (innerTypeBinding.NextTypeBinding is not null)
            {
                innerTypeBinding = innerTypeBinding.NextTypeBinding;
            }
            
            innerTypeBinding.NextTypeBinding = typeBinding;
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
            parentDiContainer = diContainer;
            return this;
        }
        
        public async ValueTask<IDiContainer> Build(CancellationToken cancellationToken)
        {
            var diContainer = new DiContainer(
                bindingsByType,
                parentDiContainer,
                cancellationToken,
                containerDisposablesCount);

            diContainer.QueueDispose(new ActionDisposableWrapper(() =>
            {
                foreach (var action in disposeActions)
                {
                    action.Invoke();
                }
            }));

            await diContainer.InitializeAsync();
            
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
    }
}

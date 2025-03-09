﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ManualDi.Main
{
    public delegate void ContainerDelegate(IDiContainer diContainer);
    
    public sealed class DiContainerBindings
    {
        private readonly Dictionary<IntPtr, TypeBinding> typeBindings;
        private readonly List<ContainerDelegate> injectDelegates;
        private readonly List<ContainerDelegate> initializationDelegates;
        private readonly List<ContainerDelegate> startupDelegates;
        private readonly List<Action> disposeActions;
        private readonly int? containerInitializationsCount;
        private readonly int? containerDisposablesCount;
        private readonly List<ITypeBindingSyncSetup> syncSetups = new();
        private readonly List<ITypeBindingAsyncSetup> asyncSetups = new();

        private IDiContainer? parentDiContainer;

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
            typeBindings = bindingsCapacity.HasValue ? new(bindingsCapacity.Value) : new();
            injectDelegates = injectCapacity.HasValue ? new(injectCapacity.Value) : new();
            initializationDelegates = initializationCapacity.HasValue ? new(initializationCapacity.Value) : new();
            disposeActions = disposeCapacity.HasValue ? new(disposeCapacity.Value) : new();
            startupDelegates = startupCapacity.HasValue ? new(startupCapacity.Value) : new();
            this.containerInitializationsCount = containerInitializationsCount;
            this.containerDisposablesCount = containerDisposablesCount;
        }
        
        internal void AddBinding(TypeBinding typeBinding, Type type)
        {
            if (typeBinding is ITypeBindingSyncSetup syncSetup)
            {
                syncSetups.Add(syncSetup);
            }
            else
            {
                asyncSetups.Add((ITypeBindingAsyncSetup) typeBinding);    
            }
            
            var apparentType = type.TypeHandle.Value;
            if (!typeBindings.TryGetValue(apparentType, out var innerTypeBinding))
            {
                typeBindings.Add(apparentType, typeBinding);
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
                typeBindings,
                parentDiContainer,
                cancellationToken,
                containerInitializationsCount,
                containerDisposablesCount);

            diContainer.QueueDispose(new ActionDisposableWrapper(() =>
            {
                foreach (var action in disposeActions)
                {
                    action.Invoke();
                }
            }));

            await diContainer.InitializeAsync(syncSetups, asyncSetups);
            
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

using System;
using System.Collections.Generic;

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
        private readonly int? containerInitializationsOnDepthCount;
        private readonly int? containerDisposablesCount;

        private IDiContainer? parentDiContainer;

        public DiContainerBindings(
            int? bindingsCapacity = null, 
            int? injectCapacity = null,
            int? initializationCapacity = null,
            int? disposeCapacity = null,
            int? entryPointCapacity = null,
            int? containerInitializationsCount = null, 
            int? containerInitializationsOnDepthCount = null,
            int? containerDisposablesCount = null
            )
        {
            this.containerInitializationsCount = containerInitializationsCount;
            this.containerInitializationsOnDepthCount = containerInitializationsOnDepthCount;
            this.containerDisposablesCount = containerDisposablesCount;
            typeBindings = bindingsCapacity.HasValue ? new(bindingsCapacity.Value) : new();
            injectDelegates = injectCapacity.HasValue ? new(injectCapacity.Value) : new();
            initializationDelegates = initializationCapacity.HasValue ? new(initializationCapacity.Value) : new();
            startupDelegates = entryPointCapacity.HasValue ? new(entryPointCapacity.Value) : new();
            disposeActions = disposeCapacity.HasValue ? new(disposeCapacity.Value) : new();
        }
        
        public void AddBinding<TApparent, TConcrete>(TypeBinding<TApparent, TConcrete> typeBinding)
        {
            var apparentType = typeof(TApparent).TypeHandle.Value;
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
        
        public void AddUnsafeBinding(UnsafeTypeBinding typeBinding)
        {
            var apparentType = typeBinding.ApparentType.TypeHandle.Value;
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
        
        public IDiContainer Build()
        {
            var diContainer = new DiContainer(
                typeBindings,
                parentDiContainer,
                containerInitializationsCount,
                containerInitializationsOnDepthCount,
                containerDisposablesCount);

            diContainer.QueueDispose(new ActionDisposableWrapper(() =>
            {
                foreach (var action in disposeActions)
                {
                    action.Invoke();
                }
            }));

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
    }
}

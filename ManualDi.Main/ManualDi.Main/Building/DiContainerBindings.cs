using System;
using System.Collections.Generic;

namespace ManualDi.Main
{
    public delegate void ContainerDelegate(IDiContainer diContainer);
    
    public sealed class DiContainerBindings
    {
        private readonly Dictionary<Type, List<TypeBinding>> typeBindings;
        private readonly List<ContainerDelegate> injectDelegates;
        private readonly List<ContainerDelegate> initializationDelegates;
        private readonly List<Action> disposeActions;

        private IDiContainer? parentDiContainer;

        public DiContainerBindings(
            int? bindingsCapacity = null, 
            int? injectCapacity = null,
            int? initializationCapacity = null,
            int? disposeCapacity = null
            )
        {
            typeBindings = bindingsCapacity.HasValue ? new(bindingsCapacity.Value) : new();
            injectDelegates = injectCapacity.HasValue ? new(injectCapacity.Value) : new();
            initializationDelegates = initializationCapacity.HasValue ? new(initializationCapacity.Value) : new();
            disposeActions = disposeCapacity.HasValue ? new(disposeCapacity.Value) : new();
        }
        
        public void AddBinding<TInterface, TConcrete>(TypeBinding<TInterface, TConcrete> typeBinding)
        {
            if (!typeBindings.TryGetValue(typeof(TInterface), out var bindings))
            {
                bindings = new List<TypeBinding>(1);
                typeBindings[typeof(TInterface)] = bindings;
            }

            bindings.Add(typeBinding);
        }
        
        public void QueueInjection(ContainerDelegate containerDelegate)
        {
            injectDelegates.Add(containerDelegate);
        }

        public void QueueInitialization(ContainerDelegate containerDelegate)
        {
            initializationDelegates.Add(containerDelegate);
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
            var diContainer = new DiContainer(typeBindings, parentDiContainer);

            foreach (var action in disposeActions)
            {
                diContainer.QueueDispose(action);
            }

            diContainer.Init();
            
            foreach (var injectDelegate in injectDelegates)
            {
                injectDelegate.Invoke(diContainer);
            }

            foreach (var initializationDelegate in initializationDelegates)
            {
                initializationDelegate.Invoke(diContainer);
            }

            return diContainer;
        }
    }
}

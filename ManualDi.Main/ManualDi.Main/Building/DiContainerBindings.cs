using System;
using System.Collections.Generic;

namespace ManualDi.Main
{
    public delegate void ContainerDelegate(IDiContainer diContainer);
    
    public sealed class DiContainerBindings
    {
        private readonly Dictionary<Type, List<TypeBinding>> typeBindings = new();
        private readonly List<Action> disposeActions = new();
        private readonly List<ContainerDelegate> initializationDelegates = new();
        private readonly List<ContainerDelegate> injectDelegates = new();

        private IDiContainer? parentDiContainer;

        public void AddBinding<TInterface, TConcrete>(TypeBinding<TInterface, TConcrete> typeBinding)
        {
            Type type = typeof(TInterface);
            if (!typeBindings.TryGetValue(type, out var bindings))
            {
                bindings = new List<TypeBinding>();
                typeBindings[type] = bindings;
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

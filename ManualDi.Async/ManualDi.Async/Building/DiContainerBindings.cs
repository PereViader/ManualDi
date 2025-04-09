using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ManualDi.Async
{
    public delegate void ContainerDelegate(IDiContainer diContainer);
    
    public sealed class DiContainerBindings
    {
        private readonly Dictionary<IntPtr, Binding> bindingsByType;
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
        
        internal void AddBinding(Binding binding, Type type)
        {
            bindingCount++;
            
            var apparentType = type.TypeHandle.Value;
            if (!bindingsByType.TryGetValue(apparentType, out var innerbinding)) //TODO: Maybe this is more efficient if we do a TryAdd instead (common case)
            {
                bindingsByType.Add(apparentType, binding);
                return;
            }

            while (innerbinding.NextBinding is not null)
            {
                innerbinding = innerbinding.NextBinding;
            }
            
            innerbinding.NextBinding = binding;
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

        public Action<IDependencyResolver> GatherDependencies()
        {
            var dependencyExtractor = new DependencyExtractor(bindingsByType);
            return dependencyExtractor.ResolveDependencies;
        }
        
        public Binding<TApparent, TConcrete> BindAsSubContainer<TApparent, TConcrete>(Binding<TApparent, TConcrete> binding, bool calculateDependencies)
        {
            if (calculateDependencies)
            {
                binding.DependsOn(GatherDependencies());
            }
            
            DiContainer? subContainer = null!;

            return binding
                .FromMethodAsync(async (c, ct) =>
                {
                    subContainer = new DiContainer(
                        bindingsByType,
                        bindingCount,
                        c,
                        ct,
                        containerDisposablesCount);

                    subContainer.QueueDispose(() =>
                    {
                        foreach (var action in disposeActions)
                        {
                            action.Invoke();
                        }
                    });

                    await subContainer.InitializeCreate();

                    c.QueueAsyncDispose(subContainer);
                    return subContainer.Resolve<TConcrete>();
                })
                .InjectAsync(async (_, _, _) => await subContainer.InitializeInject())
                .InitializeAsync(async (_, _) =>
                {
                    await subContainer.IntiailizeInitialize();

                    foreach (var injectDelegate in injectDelegates)
                    {
                        injectDelegate.Invoke(subContainer);
                    }

                    foreach (var initializationDelegate in initializationDelegates)
                    {
                        initializationDelegate.Invoke(subContainer);
                    }

                    foreach (var startupDelegate in startupDelegates)
                    {
                        startupDelegate.Invoke(subContainer);
                    }
                });
        }
        
        public async ValueTask<DiContainer> Build(CancellationToken cancellationToken)
        {
            var diContainer = new DiContainer(
                bindingsByType,
                bindingCount,
                parentDiContainer,
                cancellationToken,
                containerDisposablesCount);

            diContainer.QueueDispose(() =>
            {
                foreach (var action in disposeActions)
                {
                    action.Invoke();
                }
            });

            await diContainer.InitializeCreate();
            await diContainer.InitializeInject();
            await diContainer.IntiailizeInitialize();
            
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

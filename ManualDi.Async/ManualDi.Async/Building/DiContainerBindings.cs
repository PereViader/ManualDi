using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ManualDi.Async
{
    public delegate void ContainerDelegate(IDiContainer diContainer);
    public delegate Task AsyncContainerDelegate(IDiContainer diContainer, CancellationToken token);

    public sealed class DiContainerBindings
    {
        internal readonly Dictionary<IntPtr, BindingNode> bindingsByType;
        private readonly List<ContainerDelegate> injectDelegates;
        private readonly List<object> initializeDelegates;
        private readonly List<object> startupDelegates;
        private readonly List<object> disposables;
        
        private int bindingCount;
        private bool failureDebugReportEnabled;

        internal IDiContainer? parentDiContainer;
        internal DiContainerBindings? parentDiContainerBindings;
        
        internal BindingContext bindingContext = new();

        public DiContainerBindings(
            int? bindingsCapacity = null, 
            int? injectCapacity = null,
            int? initializationCapacity = null,
            int? disposeCapacity = null,
            int? startupCapacity = null
            )
        {
            bindingsByType = bindingsCapacity.HasValue ? new(bindingsCapacity.Value) : new();
            injectDelegates = injectCapacity.HasValue ? new(injectCapacity.Value) : new();
            initializeDelegates = initializationCapacity.HasValue ? new(initializationCapacity.Value) : new();
            disposables = disposeCapacity.HasValue ? new(disposeCapacity.Value) : new();
            startupDelegates = startupCapacity.HasValue ? new(startupCapacity.Value) : new();
        }
        
        internal void AddBinding(Binding binding, Type type)
        {
            bindingCount++;
            
            var apparentType = type.TypeHandle.Value;
#if NETSTANDARD2_1 //TryAdd is not available on netstandard2.0
            if (bindingsByType.TryAdd(apparentType, new BindingNode(binding)))
            {
                return;
            }
            var node = bindingsByType[apparentType];
#elif NETSTANDARD2_0
            if (!bindingsByType.TryGetValue(apparentType, out var node))
            {
                bindingsByType.Add(apparentType, new BindingNode(binding));
                return;
            }
#endif
            if (node.Next is null)
            {
                var nextChainNode = new BindingChainNode(binding);
                bindingsByType[apparentType] = new BindingNode(node.Binding, nextChainNode);
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
        
        public void QueueInject(ContainerDelegate containerDelegate)
        {
            injectDelegates.Add(containerDelegate);
        }
        
        [Obsolete("Use QueueInject instead")]
        public void QueueInjection(ContainerDelegate containerDelegate)
        {
            QueueInject(containerDelegate);
        }

        public void QueueInitialize(ContainerDelegate containerDelegate)
        {
            initializeDelegates.Add(containerDelegate);
        }
        
        public void QueueInitializeAsync(AsyncContainerDelegate asyncContainerDelegate)
        {
            initializeDelegates.Add(asyncContainerDelegate);
        }
        
        public void QueueStartup(ContainerDelegate containerDelegate)
        {
            startupDelegates.Add(containerDelegate);
        }
        
        public void QueueStartupAsync(AsyncContainerDelegate asyncContainerDelegate)
        {
            startupDelegates.Add(asyncContainerDelegate);
        }
        
        public void QueueDispose(Action action)
        {
            disposables.Add(action);
        }
        
        public void QueueDispose(IDisposable disposable)
        {
            disposables.Add(disposable);
        }
        
        public void QueueAsyncDispose(Func<ValueTask> action)
        {
            disposables.Add(action);
        }
        
        public void QueueAsyncDispose(IAsyncDisposable asyncDisposable)
        {
            disposables.Add(asyncDisposable);
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
        
        public DiContainerBindings WithFailureDebugReport()
        {
            failureDebugReportEnabled = true;
            return this;
        }

        public Action<IDependencyResolver> GatherDependencies()
        {
            var dependencyExtractor = new DependencyExtractor(bindingsByType);
            return dependencyExtractor.ResolveDependencies;
        }
        
        internal Binding<TConcrete> BindAsSubContainer<TConcrete>(Binding<TConcrete> binding, bool calculateDependencies)
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
                        bindingContext,
                        disposables,
                        ct);

                    await subContainer.InitializeCreate();

                    c.QueueAsyncDispose(subContainer);
                    return subContainer.Resolve<TConcrete>();
                })
                .InjectAsync(async (_, _, _) => await subContainer.InitializeInject())
                .InitializeAsync(async (_, ct) =>
                {
                    await subContainer.IntiailizeInitialize();

                    foreach (var injectDelegate in injectDelegates)
                    {
                        injectDelegate.Invoke(subContainer);
                    }

                    foreach (var initializationDelegate in initializeDelegates)
                    {
                        switch (initializationDelegate)
                        {
                            case AsyncContainerDelegate asyncInitializationDelegate:
                                await asyncInitializationDelegate.Invoke(subContainer, ct);
                                break;
                            case ContainerDelegate syncInitializationDelegate:
                                syncInitializationDelegate.Invoke(subContainer);
                                break;
                            default:
                                ThrowHelper.ThrowUnexpectedInitializationDelegateType(initializationDelegate.GetType());
                                break;
                        }
                    }

                    foreach (var startupDelegate in startupDelegates)
                    {
                        switch (startupDelegate)
                        {
                            case AsyncContainerDelegate asyncStartupDelegate:
                                await asyncStartupDelegate.Invoke(subContainer, ct);
                                break;
                            case ContainerDelegate syncStartupDelegate:
                                syncStartupDelegate.Invoke(subContainer);
                                break;
                            default:
                                ThrowHelper.ThrowUnexpectedStartupDelegateType(startupDelegate.GetType());
                                break;
                        }
                    }
                });
        }
        
        public async ValueTask<DiContainer> Build(CancellationToken ct)
        {
            var diContainer = new DiContainer(
                bindingsByType,
                bindingCount,
                parentDiContainer,
                bindingContext,
                disposables,
                ct);

            try
            {
                await diContainer.InitializeCreate();
                await diContainer.InitializeInject();
                await diContainer.IntiailizeInitialize();
            
                foreach (var injectDelegate in injectDelegates)
                {
                    injectDelegate.Invoke(diContainer);
                }

                foreach (var initializationDelegate in initializeDelegates)
                {
                    switch (initializationDelegate)
                    {
                        case AsyncContainerDelegate asyncInitializationDelegate:
                            await asyncInitializationDelegate.Invoke(diContainer, ct);
                            break;
                        case ContainerDelegate syncInitializationDelegate:
                            syncInitializationDelegate.Invoke(diContainer);
                            break;
                        default:
                            ThrowHelper.ThrowUnexpectedInitializationDelegateType(initializationDelegate.GetType());
                            break;
                    }
                }

                foreach (var startupDelegate in startupDelegates)
                {
                    switch (startupDelegate)
                    {
                        case AsyncContainerDelegate asyncStartupDelegate:
                            await asyncStartupDelegate.Invoke(diContainer, ct);
                            break;
                        case ContainerDelegate syncStartupDelegate:
                            syncStartupDelegate.Invoke(diContainer);
                            break;
                        default:
                            ThrowHelper.ThrowUnexpectedStartupDelegateType(startupDelegate.GetType());
                            break;
                    }
                }

                return diContainer;
            }
            catch (Exception buildException)
            {
                if (failureDebugReportEnabled)
                {
                    buildException.Data.Add(DiContainer.FailureDebugReportKey, diContainer.GetFailureDebugReport());
                }
                
                try
                {
                    await diContainer.DisposeAsync();
                }
                catch (Exception disposeException)
                {
                    throw new AggregateException(buildException, disposeException);
                }
                throw;
            }
        }
    }
}

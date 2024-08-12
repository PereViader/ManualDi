using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ManualDi.Main
{
    public sealed class DiContainer : IDiContainer
    {
        private readonly BindingInitializer bindingInitializer = new();
        private readonly DisposableActionQueue disposableActionQueue = new();
        private readonly Dictionary<Type, List<TypeBinding>> allTypeBindings;
        private readonly IDiContainer? parentDiContainer;

        private bool isResolving;
        private bool hasBeenInitialized;
        private bool disposedValue;

        public DiContainer(Dictionary<Type, List<TypeBinding>> allTypeBindings, IDiContainer? parentDiContainer)
        {
            this.allTypeBindings = allTypeBindings;
            this.parentDiContainer = parentDiContainer;
        }

        public void Init()
        {
            if (hasBeenInitialized)
            {
                throw new InvalidOperationException("Container has already finished binding. Make sure you bind everything only when creating it");
            }

            hasBeenInitialized = true;

            foreach (var bindings in allTypeBindings)
            {
                foreach (var binding in bindings.Value)
                {
                    if (!binding.IsLazy)
                    {
                        ResolveBinding(binding);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object? ResolveContainer(Type type, ResolutionConstraints? resolutionConstraints)
        {
            var typeBinding = GetTypeForConstraint(type, resolutionConstraints);
            if (typeBinding is not null)
            {
                return ResolveBinding(typeBinding);
            }

            if (parentDiContainer is not null)
            {
                return parentDiContainer.ResolveContainer(type, resolutionConstraints);
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object ResolveBinding(TypeBinding typeBinding)
        {
            bool wasResolving = this.isResolving;
            isResolving = true;

            var (instance, isNew) = typeBinding.Create(this);
            if (!isNew)
            {
                isResolving = wasResolving;
                return instance;
            }

            typeBinding.InjectObject(instance, this);
            if (typeBinding.TryToDispose && instance is IDisposable disposable)
            {
                QueueDispose(disposable);
            }
            bindingInitializer.Queue(typeBinding, instance);

            isResolving = wasResolving;

            if (!isResolving)
            {
                bindingInitializer.InitializeCurrentLevelQueued(this);
            }

            return instance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TypeBinding? GetTypeForConstraint(Type type, ResolutionConstraints? resolutionConstraints)
        {
            if (!allTypeBindings.TryGetValue(type, out var bindings))
            {
                return null;
            }

            if (resolutionConstraints is null)
            {
                return bindings[0];
            }

            foreach (var binding in bindings)
            {
                if (resolutionConstraints.Accepts(binding))
                {
                    return binding;
                }
            }
            
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResolveAllContainer(Type type, ResolutionConstraints? resolutionConstraints, IList resolutions)
        {
            AddResolveAllInstances(type, resolutionConstraints, resolutions);

            parentDiContainer?.ResolveAllContainer(type, resolutionConstraints, resolutions);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddResolveAllInstances(Type type, ResolutionConstraints? resolutionConstraints, IList resolutions)
        {
            if (!this.allTypeBindings.TryGetValue(type, out var typeBindings))
            {
                return;
            }

            if (resolutionConstraints is null)
            {
                foreach (var typeBinding in typeBindings)
                {
                    var resolved = ResolveBinding(typeBinding);
                    resolutions.Add(resolved);
                }
            }
            else
            {
                foreach (var typeBinding in typeBindings)
                {
                    if (resolutionConstraints.Accepts(typeBinding))
                    {
                        var resolved = ResolveBinding(typeBinding);
                        resolutions.Add(resolved);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void QueueDispose(IDisposable disposable)
        {
            disposableActionQueue.QueueDispose(disposable);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void QueueDispose(Action disposableAction)
        {
            disposableActionQueue.QueueDispose(disposableAction);
        }

        public void Dispose()
        {
            if (disposedValue)
            {
                return;
            }
            disposedValue = true;

            disposableActionQueue.DisposeAll();
        }
    }
}

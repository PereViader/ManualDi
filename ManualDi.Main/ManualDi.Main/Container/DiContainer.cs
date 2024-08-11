using System;
using System.Collections;
using ManualDi.Main.Scopes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

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

        public bool TryResolveContainer(Type type, ResolutionConstraints? resolutionConstraints, [MaybeNullWhen(false)] out object resolution)
        {
            if (TryGetTypeForConstraint(type, resolutionConstraints, out var typeBinding))
            {
                resolution = ResolveBinding(typeBinding);
                return true;
            }

            if (parentDiContainer != null)
            {
                return parentDiContainer.TryResolveContainer(type, resolutionConstraints, out resolution);
            }

            resolution = default;
            return false;
        }

        private object ResolveBinding(TypeBinding typeBinding)
        {
            bool wasResolving = this.isResolving;
            isResolving = true;

            var resolvedInstance = typeBinding.TypeScope switch
            {
                TypeScope.Single => ResolveSingle(typeBinding),
                TypeScope.Transient => ResolveTransient(typeBinding),
                _ => throw new ArgumentOutOfRangeException(nameof(typeBinding.TypeScope))
            };
            var instance = resolvedInstance.Instance;

            if (!resolvedInstance.IsNew)
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
        
        private ResolvedInstance ResolveSingle(TypeBinding typeBinding)
        {
            var instance = typeBinding.SingleInstance;
            if (instance is not null)
            {
                return ResolvedInstance.Reused(instance);
            }

            instance = typeBinding.Create(this);
            typeBinding.SingleInstance = instance;

            return ResolvedInstance.New(instance);
        }
        
        private ResolvedInstance ResolveTransient(TypeBinding typeBinding)
        {
            var instance = typeBinding.Create(this);
            return ResolvedInstance.New(instance);
        }

        private bool TryGetTypeForConstraint(Type type, ResolutionConstraints? resolutionConstraints, [MaybeNullWhen(false)] out TypeBinding typeBinding)
        {
            if (!allTypeBindings.TryGetValue(type, out var bindings) || bindings.Count == 0)
            {
                typeBinding = default;
                return false;
            }

            if (resolutionConstraints is null)
            {
                typeBinding = bindings[0];
                return true;
            }

            foreach (var binding in bindings)
            {
                if (resolutionConstraints.Accepts(binding))
                {
                    typeBinding = binding;
                    return true;
                }
            }

            typeBinding = default;
            return false;
        }

        private bool TryGetAllTypeForConstraint(Type type, ResolutionConstraints? resolutionConstraints, [MaybeNullWhen(false)] out List<TypeBinding> typeBindings)
        {
            if (!this.allTypeBindings.TryGetValue(type, out var bindings) || bindings.Count == 0)
            {
                typeBindings = default;
                return false;
            }

            if (resolutionConstraints is null)
            {
                typeBindings = new List<TypeBinding>(bindings);
                return true;
            }

            typeBindings = new List<TypeBinding>();
            foreach (var binding in bindings)
            {
                if (resolutionConstraints.Accepts(binding))
                {
                    typeBindings.Add(binding);
                }
            }

            return typeBindings.Count > 0;
        }

        public void ResolveAllContainer(Type type, ResolutionConstraints? resolutionConstraints, IList resolutions)
        {
            if (TryGetAllTypeForConstraint(type, resolutionConstraints, out var typeBindings))
            {
                foreach (var typeBinding in typeBindings)
                {
                    var resolved = ResolveBinding(typeBinding);
                    resolutions.Add(resolved);
                }
            }

            parentDiContainer?.ResolveAllContainer(type, resolutionConstraints, resolutions);
        }

        public void QueueDispose(IDisposable disposable)
        {
            disposableActionQueue.QueueDispose(disposable);
        }
        
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

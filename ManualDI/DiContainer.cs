using ManualDi.TypeResolvers;
using System;
using System.Collections.Generic;

namespace ManualDi
{
    public class DiContainer : IDiContainer
    {
        public Dictionary<Type, List<ITypeBinding>> TypeBindings { get; } = new Dictionary<Type, List<ITypeBinding>>();
        public List<ITypeResolver> TypeResolvers { get; } = new List<ITypeResolver>();
        public List<IInjectionCommand> InjectionCommands { get; } = new List<IInjectionCommand>();
        public ITypeBindingFactory TypeBindingFactory { get; set; }
        public IDiContainer ParentDiContainer { get; set; }

        public void Bind<T>(Action<ITypeBinding<T>> action)
        {
            var typeBinding = TypeBindingFactory.Create<T>();
            action.Invoke(typeBinding);

            if (!TypeBindings.TryGetValue(typeof(T), out var bindings))
            {
                bindings = new List<ITypeBinding>();
                TypeBindings[typeof(T)] = bindings;
            }

            bindings.Add(typeBinding);
        }

        public T Resolve<T>()
        {
            var typeBinding = GetTypeForConstraint<T>(null);
            return Resolve(typeBinding);
        }

        public T Resolve<T>(Action<IResolutionConstraints> resolution)
        {
            var resolutionConstraints = new ResolutionConstraints();
            resolution.Invoke(resolutionConstraints);

            var typeBinding = GetTypeForConstraint<T>(resolutionConstraints);
            return Resolve(typeBinding);
        }

        public bool TryResolve<T>(out T result)
        {
            throw new NotImplementedException();
        }

        public bool TryResolve<T>(Action<IResolutionConstraints> resolution, out T result)
        {
            throw new NotImplementedException();
        }

        private T Resolve<T>(ITypeBinding<T> typeBinding)
        {
            return (T)ResolveUntyped(typeBinding);
        }

        private object ResolveUntyped(ITypeBinding typeBinding)
        {
            var typeResolver = GetResolverFor(typeBinding);

            var willTriggerInject = InjectionCommands.Count == 0;

            var instance = typeResolver.Resolve(this, typeBinding, InjectionCommands);

            if (willTriggerInject)
            {
                InjectQueuedInstances();
            }

            return instance;
        }

        private ITypeBinding<T> GetTypeForConstraint<T>(IResolutionConstraints resolutionConstraints)
        {
            if (!TypeBindings.TryGetValue(typeof(T), out var bindings) || bindings.Count == 0)
            {
                throw new InvalidOperationException($"There are no bindings for type {typeof(T).FullName}");
            }

            if (resolutionConstraints == null)
            {
                return (ITypeBinding<T>)bindings[0];
            }

            foreach (var binding in bindings)
            {
                var typeBinding = (ITypeBinding<T>)binding;
                if (resolutionConstraints.Accepts(typeBinding))
                {
                    return typeBinding;
                }
            }

            throw new InvalidOperationException("No binding could satisfy constraint");
        }

        private List<ITypeBinding<T>> GetAllTypeForConstraint<T>(IResolutionConstraints resolutionConstraints)
        {
            if (!TypeBindings.TryGetValue(typeof(T), out var bindings) || bindings.Count == 0)
            {
                throw new InvalidOperationException($"There are no bindings for type {typeof(T).FullName}");
            }

            var typeBindings = new List<ITypeBinding<T>>();

            if (resolutionConstraints == null)
            {
                foreach (var typeBinding in bindings)
                {
                    typeBindings.Add((ITypeBinding<T>)typeBinding);
                }
                return typeBindings;
            }

            foreach (var binding in bindings)
            {
                var typeBinding = (ITypeBinding<T>)binding;
                if (resolutionConstraints.Accepts(typeBinding))
                {
                    typeBindings.Add(typeBinding);
                }
            }

            if (typeBindings.Count == 0)
            {
                throw new InvalidOperationException("No binding could satisfy constraint");
            }

            return typeBindings;
        }

        private void InjectQueuedInstances()
        {
            while (InjectionCommands.Count > 0)
            {
                var index = InjectionCommands.Count - 1;
                var injectionCommand = InjectionCommands[index];
                injectionCommand.Inject(this);
                InjectionCommands.RemoveAt(index);
            }
        }

        private ITypeResolver GetResolverFor(ITypeBinding typeBinding)
        {
            foreach (var resolver in TypeResolvers)
            {
                if (resolver.IsResolverFor(typeBinding))
                {
                    return resolver;
                }
            }

            throw new InvalidOperationException($"Could not find resolver for type binding of type {typeBinding.GetType().FullName}");
        }

        public List<T> ResolveAll<T>()
        {
            var typeBindings = GetAllTypeForConstraint<T>(null);
            return ResolveAll(typeBindings);
        }

        public List<T> ResolveAll<T>(Action<IResolutionConstraints> resolution)
        {
            var resolutionConstraints = new ResolutionConstraints();
            resolution.Invoke(resolutionConstraints);
            var typeBindings = GetAllTypeForConstraint<T>(resolutionConstraints);

            return ResolveAll(typeBindings);
        }

        private List<T> ResolveAll<T>(List<ITypeBinding<T>> typeBindings)
        {
            var resolved = new List<T>();
            foreach (var typeBinding in typeBindings)
            {
                resolved.Add(Resolve(typeBinding));
            }
            return resolved;
        }

        public void FinishBinding()
        {
            foreach (var bindings in TypeBindings)
            {
                foreach (var binding in bindings.Value)
                {
                    if (!binding.IsLazy)
                    {
                        ResolveUntyped(binding);
                    }
                }
            }
        }
    }
}

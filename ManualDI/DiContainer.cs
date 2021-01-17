using ManualDI.TypeResolvers;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace ManualDI
{
    public class DiContainer : IDiContainer
    {
        public Dictionary<Type, object> TypeBindings { get; } = new Dictionary<Type, object>();
        public List<ITypeResolver> TypeResolvers { get; } = new List<ITypeResolver>();
        public List<IInjectionCommand> InjectionCommands { get; } = new List<IInjectionCommand>();
        public ITypeBindingFactory TypeBindingFactory { get; set; }

        public void Bind<T>(Action<ITypeBinding<T>> action)
        {
            var typeBinding = TypeBindingFactory.Create<T>();
            action.Invoke(typeBinding);
            TypeBindings[typeof(T)] = typeBinding;
        }

        public T Resolve<T>()
        {
            var typeBinding = (ITypeBinding<T>)TypeBindings[typeof(T)];
            if (!TryGetResolverFor(typeBinding, out var typeResolver))
            {
                throw new InvalidOperationException($"Could not find resolver for type binding of type {typeof(ITypeBinding<T>).FullName}");
            }

            var willTriggerInject = InjectionCommands.Count == 0;

            var instance = typeResolver.Resolve(this, typeBinding, InjectionCommands);

            if(willTriggerInject)
            {
                InjectQueuedInstances();
            }

            return instance;
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

        private bool TryGetResolverFor<T>(ITypeBinding<T> typeBinding, out ITypeResolver typeResolver)
        {
            foreach(var resolver in TypeResolvers)
            {
                if(resolver.IsResolverFor(typeBinding))
                {
                    typeResolver = resolver;
                    return true;
                }
            }

            typeResolver = default;
            return false;
        }
    }
}

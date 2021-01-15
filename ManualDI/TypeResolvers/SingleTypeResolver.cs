using System;
using System.Collections.Generic;

namespace ManualDI.TypeResolvers
{
    public class SingleTypeResolver : ITypeResolver
    {
        public Dictionary<Type, object> Instances { get; } = new Dictionary<Type, object>();

        public bool IsResolverFor<T>(ITypeBinding<T> typeBinding)
        {
            return typeBinding.TypeScope is SingleTypeScope;
        }

        public T Resolve<T>(IContainer container, ITypeBinding<T> typeBinding, List<IInjectionCommand> injectionCommands)
        {
            var type = typeof(T);
            if (Instances.TryGetValue(type, out var singleInstance))
            {
                return (T)singleInstance;
            }

            var instance = typeBinding.Factory.Create(container);
            Instances[type] = instance;

            injectionCommands.Add(new InjectionCommand<T>(typeBinding.TypeInjection, instance));

            return instance;
        }
    }
}

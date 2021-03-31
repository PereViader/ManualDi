using ManualDI.TypeScopes;
using System.Collections.Generic;

namespace ManualDI.TypeResolvers
{
    public class SingleTypeResolver : ITypeResolver
    {
        public Dictionary<object, object> Instances { get; } = new Dictionary<object, object>();

        public bool IsResolverFor<T>(ITypeBinding<T> typeBinding)
        {
            return typeBinding.TypeScope is SingleTypeScope;
        }

        public T Resolve<T>(IDiContainer container, ITypeBinding<T> typeBinding, List<IInjectionCommand> injectionCommands)
        {
            if (Instances.TryGetValue(typeBinding, out var singleInstance))
            {
                return (T)singleInstance;
            }

            var instance = typeBinding.Factory.Create(container);
            Instances[typeBinding] = instance;

            if (typeBinding.TypeInjection != null)
            {
                injectionCommands.Add(new InjectionCommand<T>(typeBinding.TypeInjection, instance));
            }

            return instance;
        }
    }
}

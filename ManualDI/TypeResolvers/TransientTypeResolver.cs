using System.Collections.Generic;

namespace ManualDI.TypeResolvers
{
    public class TransientTypeResolver : ITypeResolver
    {
        public bool IsResolverFor<T>(ITypeBinding<T> typeBinding)
        {
            return typeBinding.TypeScope is TransientTypeScope;
        }

        public T Resolve<T>(IContainer container, ITypeBinding<T> typeBinding, List<IInjectionCommand> injectionCommands)
        {
            var instance = typeBinding.Factory.Create(container);

            injectionCommands.Add(new InjectionCommand<T>(typeBinding.TypeInjection, instance));

            return instance;
        }
    }
}

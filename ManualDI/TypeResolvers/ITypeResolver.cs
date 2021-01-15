using System.Collections.Generic;

namespace ManualDI.TypeResolvers
{
    public interface ITypeResolver
    {
        bool IsResolverFor<T>(ITypeBinding<T> typeBinding);
        T Resolve<T>(IContainer container, ITypeBinding<T> typeBinding, List<IInjectionCommand> injectionCommands);
    }
}

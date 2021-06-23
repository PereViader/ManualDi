using System.Collections.Generic;

namespace ManualDI.TypeResolvers
{
    public interface ITypeResolver
    {
        bool IsResolverFor(ITypeBinding typeBinding);
        object Resolve(IDiContainer container, ITypeBinding typeBinding, List<IInjectionCommand> injectionCommands);
    }
}

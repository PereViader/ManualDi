using ManualDi.Main.Initialization;
using ManualDi.Main.TypeScopes;
using System.Collections.Generic;

namespace ManualDi.Main.TypeResolvers
{
    public class TransientTypeResolver : ITypeResolver
    {
        public bool IsResolverFor(ITypeBinding typeBinding)
        {
            return typeBinding.TypeScope is TransientTypeScope;
        }

        public object Resolve(IDiContainer container, ITypeBinding typeBinding, List<IInjectionCommand> injectionCommands, IBindingInitializer bindingInitializer)
        {
            var instance = typeBinding.Factory.Create(container);

            if (typeBinding.TypeInjections != null)
            {
                foreach (var typeInjection in typeBinding.TypeInjections)
                {
                    injectionCommands.Add(new InjectionCommand(typeInjection, instance));
                }
            }

            bindingInitializer.Injest(typeBinding, instance);

            return instance;
        }
    }
}

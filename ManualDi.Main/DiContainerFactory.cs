using ManualDi.Main.Disposing;
using ManualDi.Main.Initialization;
using ManualDi.Main.Injection;
using ManualDi.Main.TypeResolvers;
using System.Collections.Generic;

namespace ManualDi.Main
{
    public class DiContainerFactory : IDiContainerFactory
    {
        public IDiContainer Create(IDiContainerBindings diContainerBindings, IDiContainer parentDiContainer)
        {
            var diContainer = new DiContainer()
            {
                TypeBindings = diContainerBindings.TypeBindings,
                ParentDiContainer = parentDiContainer,
                TypeResolvers = new List<ITypeResolver>()
                {
                    new SingleTypeResolver(),
                    new TransientTypeResolver()
                },

                BindingInjector = new BindingInjector(),
                BindingInitializer = new BindingInitializer(),
                BindingDisposer = new BindingDisposer(),
            };

            diContainer.Init();

            return diContainer;
        }
    }
}

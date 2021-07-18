using ManualDi.TypeResolvers;

namespace ManualDi
{
    public class ContainerBuilder
    {
        private IDiContainer parentDiContainer;

        public ContainerBuilder WithParentContainer(IDiContainer diContainer)
        {
            this.parentDiContainer = diContainer;
            return this;
        }

        public IDiContainer Build()
        {
            var container = new DiContainer();

            container.ParentDiContainer = parentDiContainer;

            container.TypeResolvers.Add(new SingleTypeResolver());
            container.TypeResolvers.Add(new TransientTypeResolver());

            container.TypeBindingFactory = new TypeBindingFactory();

            return container;
        }
    }
}

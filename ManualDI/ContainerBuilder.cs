using ManualDI.TypeResolvers;

namespace ManualDI
{
    public class ContainerBuilder
    {
        public IDiContainer Build()
        {
            var container = new DiContainer();
            container.TypeResolvers.Add(new SingleTypeResolver());
            container.TypeResolvers.Add(new TransientTypeResolver());
            return container;
        }
    }
}

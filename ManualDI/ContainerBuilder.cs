using ManualDI.TypeResolvers;

namespace ManualDI
{
    public class ContainerBuilder
    {
        public IContainer Build()
        {
            var container = new Container();
            container.TypeResolvers.Add(new SingleTypeResolver());
            container.TypeResolvers.Add(new TransientTypeResolver());
            return container;
        }
    }
}

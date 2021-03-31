namespace ManualDI.TypeFactories
{
    public class ContainerTypeFactory<T> : ITypeFactory<T>
    {
        public T Create(IDiContainer container)
        {
            return container.Resolve<T>();
        }
    }
}

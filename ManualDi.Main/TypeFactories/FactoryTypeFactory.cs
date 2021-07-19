namespace ManualDi.Main.TypeFactories
{
    public class FactoryTypeFactory<TFactory, TValue> : ITypeFactory<TValue>
        where TFactory : IFactory<TValue>
    {
        public TValue Create(IDiContainer container)
        {
            var factory = container.Resolve<TFactory>();
            var value = factory.Create();
            return value;
        }

        object ITypeFactory.Create(IDiContainer container)
        {
            return Create(container);
        }
    }
}

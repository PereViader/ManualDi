namespace ManualDi.TypeFactories
{
    public class MethodTypeFactory<T> : ITypeFactory<T>
    {
        public FactoryMethodDelegate<T> FactoryMethodDelegate { get; }

        public MethodTypeFactory(FactoryMethodDelegate<T> factoryMehtodDelegate)
        {
            FactoryMethodDelegate = factoryMehtodDelegate;
        }

        public T Create(IDiContainer container)
        {
            return FactoryMethodDelegate.Invoke(container);
        }

        object ITypeFactory.Create(IDiContainer container)
        {
            return Create(container);
        }
    }
}

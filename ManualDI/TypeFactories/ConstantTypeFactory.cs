namespace ManualDI.TypeFactories
{
    public class ConstantTypeFactory<T> : ITypeFactory<T>
    {
        public T Constant { get; }

        public ConstantTypeFactory(T constant)
        {
            Constant = constant;
        }

        public T Create(IDiContainer container)
        {
            return Constant;
        }
    }
}

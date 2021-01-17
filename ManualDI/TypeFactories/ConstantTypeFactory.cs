namespace ManualDI.TypeFactories
{
    public class InstanceTypeFactory<T> : ITypeFactory<T>
    {
        public T Constant { get; }

        public InstanceTypeFactory(T constant)
        {
            Constant = constant;
        }

        public T Create(IDiContainer container)
        {
            return Constant;
        }
    }
}

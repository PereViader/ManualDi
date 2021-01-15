namespace ManualDI
{
    public class ConstantTypeFactory<T> : ITypeFactory<T>
    {
        public T Constant { get; }

        public ConstantTypeFactory(T constant)
        {
            Constant = constant;
        }

        public T Create(IContainer container)
        {
            return Constant;
        }
    }
}

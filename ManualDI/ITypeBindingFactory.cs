namespace ManualDI
{
    public interface ITypeBindingFactory
    {
        public ITypeBinding<T> Create<T>();
    }
}

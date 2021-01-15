namespace ManualDI
{
    public interface ITypeInjection<T>
    {
        public void Inject(IContainer container, T instance);
    }
}
namespace ManualDI
{
    public interface ITypeFactory<T>
    {
        T Create(IContainer container);
    }
}

namespace ManualDI.TypeFactories
{
    public interface ITypeFactory<T>
    {
        T Create(IDiContainer container);
    }
}

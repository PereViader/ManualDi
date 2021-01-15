namespace ManualDI
{
    public interface ITypeBinding<T>
    {
        ITypeScope TypeScope { get; }
        ITypeFactory<T> Factory { get; }
        ITypeInjection<T> TypeInjection { get; }
    }
}

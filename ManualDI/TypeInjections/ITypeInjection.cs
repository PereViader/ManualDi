namespace ManualDi.TypeInjections
{
    public interface ITypeInjection
    {
        void Inject(object instance, IDiContainer container);
    }

    public interface ITypeInjection<T> : ITypeInjection
    {
        void Inject(T instance, IDiContainer container);
    }
}

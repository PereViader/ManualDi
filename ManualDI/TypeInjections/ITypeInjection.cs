namespace ManualDI.TypeInjections
{
    public interface ITypeInjection<T>
    {
        public void Inject(T instance, IDiContainer container);
    }
}
namespace ManualDI.TypeInjections
{
    public class NopTypeInjection<T> : ITypeInjection<T>
    {
        public static NopTypeInjection<T> Instance { get; } = new NopTypeInjection<T>();

        private NopTypeInjection()
        {
        }

        public void Inject(T instance, IDiContainer container)
        {
        }
    }
}
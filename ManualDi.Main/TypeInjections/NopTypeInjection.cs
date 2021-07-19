namespace ManualDi.Main.TypeInjections
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

        public void Inject(object instance, IDiContainer container)
        {
        }
    }
}

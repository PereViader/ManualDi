namespace ManualDi.Main.TypeInjections
{
    public class MethodTypeInjection<T> : ITypeInjection<T>
    {
        public InjectionDelegate<T> InjectionDelegate { get; }

        public MethodTypeInjection(InjectionDelegate<T> injectionDelegate)
        {
            InjectionDelegate = injectionDelegate;
        }

        public void Inject(T instance, IDiContainer container)
        {
            InjectionDelegate.Invoke(instance, container);
        }

        public void Inject(object instance, IDiContainer container)
        {
            Inject((T)instance, container);
        }
    }
}

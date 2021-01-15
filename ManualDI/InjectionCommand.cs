namespace ManualDI
{
    public class InjectionCommand<T> : IInjectionCommand
    {
        public ITypeInjection<T> TypeInjection { get; }
        public T Instance { get; }

        public InjectionCommand(ITypeInjection<T> typeInjection, T instance)
        {
            TypeInjection = typeInjection;
            Instance = instance;
        }

        public void Inject(IContainer container)
        {
            TypeInjection.Inject(container, Instance);
        }
    }
}

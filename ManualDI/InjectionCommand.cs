using ManualDI.TypeInjections;

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

        public void Inject(IDiContainer container)
        {
            TypeInjection.Inject(Instance, container);
        }
    }
}

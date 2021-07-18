using ManualDi.TypeInjections;

namespace ManualDi
{
    public class InjectionCommand : IInjectionCommand
    {
        public ITypeInjection TypeInjection { get; }
        public object Instance { get; }

        public InjectionCommand(ITypeInjection typeInjection, object instance)
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

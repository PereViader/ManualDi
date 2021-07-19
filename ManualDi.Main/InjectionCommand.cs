using ManualDi.Main.TypeInjections;

namespace ManualDi.Main
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

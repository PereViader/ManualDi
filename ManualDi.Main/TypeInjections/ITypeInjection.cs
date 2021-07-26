namespace ManualDi.Main.TypeInjections
{
    public interface ITypeInjection
    {
        void Inject(object instance, IDiContainer container);
    }
}

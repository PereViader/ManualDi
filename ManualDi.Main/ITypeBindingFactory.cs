namespace ManualDi.Main
{
    public interface ITypeBindingFactory
    {
        ITypeBinding<TInterface, TConcrete> Create<TInterface, TConcrete>();
    }
}

namespace ManualDi.Main
{
    public interface ITypeBindingFactory
    {
        public ITypeBinding<T> Create<T>(RegisterDisposeDelegate registerDisposeDelegate);
    }
}

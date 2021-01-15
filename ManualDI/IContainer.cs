namespace ManualDI
{
    public interface IContainer
    {
        void Bind<T>(ITypeBinding<T> typeSetup);
        T Resolve<T>();
    }
}
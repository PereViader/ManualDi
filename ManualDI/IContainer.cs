namespace ManualDI
{
    public interface IContainer
    {
        void Bind<T>(ITypeSetup<T> typeSetup);
        T Resolve<T>();
    }
}
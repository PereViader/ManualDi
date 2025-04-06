namespace ManualDi.Async
{
    public delegate void InstallDelegate(DiContainerBindings bindings);

    public interface IInstaller
    {
        void Install(DiContainerBindings bindings);
    }
}

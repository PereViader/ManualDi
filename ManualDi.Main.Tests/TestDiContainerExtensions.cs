namespace ManualDi.Main.Tests
{
    public static class TestDiContainerExtensions
    {
        public static T FinishAndResolve<T>(this IDiContainer diContainer)
        {
            diContainer.FinishBinding();
            return diContainer.Resolve<T>();
        }
    }
}

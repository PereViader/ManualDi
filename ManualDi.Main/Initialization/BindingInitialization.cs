namespace ManualDi.Main.Initialization
{
    public delegate void InitializationDelegate<T>(T instance);

    public class BindingInitialization<T> : IBindingInitialization
    {
        public InitializationDelegate<T> InitializationDelegate { get; }

        public BindingInitialization(InitializationDelegate<T> initializationDelegate)
        {
            InitializationDelegate = initializationDelegate;
        }

        public void Initialize(object instance)
        {
            T typedInstance = (T)instance;
            InitializationDelegate.Invoke(typedInstance);
        }
    }
}

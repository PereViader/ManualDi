namespace ManualDi.TypeScopes
{
    public class TransientTypeScope : ITypeScope
    {
        public static TransientTypeScope Instance { get; } = new TransientTypeScope();

        private TransientTypeScope()
        {
        }
    }
}

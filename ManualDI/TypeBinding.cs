namespace ManualDI
{
    public class TypeBinding<T> : ITypeBinding<T>
    {
        public ITypeScope TypeScope { get; }
        public ITypeFactory<T> Factory { get; }
        public ITypeInjection<T> TypeInjection { get; }

        public TypeBinding(ITypeScope typeScope, ITypeFactory<T> typeFactory, ITypeInjection<T> typeInjection)
        {
            TypeScope = typeScope;
            Factory = typeFactory;
            TypeInjection = typeInjection;
        }
    }
}

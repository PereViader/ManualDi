namespace ManualDI
{
    public class TypeSetup<T> : ITypeSetup<T>
    {
        public TypeScope TypeScope { get; }
        public ITypeFactory<T> Factory { get; }

        public ITypeInjection<T> TypeInjection { get; }

        public TypeSetup(TypeScope typeScope, ITypeFactory<T> typeFactory, ITypeInjection<T> typeInjection)
        {
            TypeScope = typeScope;
            Factory = typeFactory;
            TypeInjection = typeInjection;
        }
    }
}

using ManualDI.TypeFactories;
using ManualDI.TypeInjections;
using ManualDI.TypeScopes;

namespace ManualDI
{
    public class TypeBinding<T> : ITypeBinding<T>
    {
        public ITypeMetadata TypeMetadata { get; set; }
        public ITypeScope TypeScope { get; set; }
        public ITypeFactory<T> Factory { get; set; }
        public ITypeInjection<T> TypeInjection { get; set; }
        public bool IsLazy { get; set; }
        
        ITypeFactory ITypeBinding.Factory 
        {
            get => Factory; 
            set => Factory = (ITypeFactory<T>)value; 
        }

        ITypeInjection ITypeBinding.TypeInjection 
        { 
            get => TypeInjection; 
            set => TypeInjection = (ITypeInjection<T>)value; 
        }
    }
}

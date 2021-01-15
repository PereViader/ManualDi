using System;

namespace ManualDI
{
    public interface ITypeSetup<T>
    {
        TypeScope TypeScope { get; }
        ITypeFactory<T> Factory { get; }
        ITypeInjection<T> TypeInjection { get; }
    }
}

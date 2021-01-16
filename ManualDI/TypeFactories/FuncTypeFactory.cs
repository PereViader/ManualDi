using System;
using System.Reflection;

namespace ManualDI.TypeFactories
{
    public class FuncTypeFactory<T> : ITypeFactory<T>
    {
        public Func<IDiContainer, T> Func { get; }

        public FuncTypeFactory(Func<IDiContainer, T> func)
        {
            Func = func;
        }

        public T Create(IDiContainer container)
        {
            return Func.Invoke(container);
        }
    }
}

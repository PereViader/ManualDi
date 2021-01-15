using System;
using System.Reflection;

namespace ManualDI
{
    public class FuncTypeFactory<T> : ITypeFactory<T>
    {
        public Func<IContainer, T> Func { get; }

        public FuncTypeFactory(Func<IContainer, T> func)
        {
            Func = func;
        }

        public T Create(IContainer container)
        {
            return Func.Invoke(container);
        }
    }
}

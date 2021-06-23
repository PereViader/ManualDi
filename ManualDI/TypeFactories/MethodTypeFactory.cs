using System;

namespace ManualDI.TypeFactories
{
    public class MethodTypeFactory<T> : ITypeFactory<T>
    {
        public Func<IDiContainer, T> Func { get; }

        public MethodTypeFactory(Func<IDiContainer, T> func)
        {
            Func = func;
        }

        public T Create(IDiContainer container)
        {
            return Func.Invoke(container);
        }

        object ITypeFactory.Create(IDiContainer container)
        {
            return Create(container);
        }
    }
}

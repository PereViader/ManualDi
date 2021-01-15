using System.ComponentModel;

namespace ManualDI
{
    public class NopTypeInjection<T> : ITypeInjection<T>
    {
        public static NopTypeInjection<T> Instance { get; } = new NopTypeInjection<T>();

        private NopTypeInjection()
        {
        }

        public void Inject(IContainer container, T instance)
        {
        }
    }
}
﻿namespace ManualDi
{
    public interface ITypeBindingFactory
    {
        public ITypeBinding<T> Create<T>();
    }
}

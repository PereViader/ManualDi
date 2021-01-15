using System;
using System.Collections.Generic;

namespace ManualDI
{
    public class Container : IContainer
    {
        public Dictionary<Type, object> TypeFactories { get; } = new Dictionary<Type, object>();
        public Dictionary<Type, object> SingleInstances { get; } = new Dictionary<Type, object>();

        public void Bind<T>(ITypeSetup<T> typeSetup)
        {
            TypeFactories[typeof(T)] = typeSetup;
        }

        public T Resolve<T>()
        {
            var type = typeof(T);

            if (!TypeFactories.TryGetValue(type, out var objectTypeSetup))
            {
                throw new Exception($"Type {typeof(T).FullName} was not bound");
            }

            var typeSetup = (ITypeSetup<T>)objectTypeSetup;
            if (typeSetup.TypeScope == TypeScope.Single && SingleInstances.TryGetValue(type, out var singleInstance))
            {
                return (T)singleInstance;
            }
            
            var instance = typeSetup.Factory.Create(this);
            if (typeSetup.TypeScope == TypeScope.Single)
            {
                SingleInstances[type] = instance;
            }
            typeSetup.TypeInjection.Inject(this, instance);
            return instance;
        }
    }
}

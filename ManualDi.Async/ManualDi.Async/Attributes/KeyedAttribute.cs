using System;

namespace ManualDi.Async
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class KeyedAttribute : Attribute
    {
        public Type KeyType { get; }

        public KeyedAttribute(Type keyType)
        {
            KeyType = keyType;
        }
    }
}

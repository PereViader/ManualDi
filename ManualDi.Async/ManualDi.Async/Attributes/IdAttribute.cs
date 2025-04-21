using System;

namespace ManualDi.Async
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class IdAttribute : Attribute
    {
        public string Id { get; }

        public IdAttribute(string id)
        {
            Id = id;
        }
    }
}
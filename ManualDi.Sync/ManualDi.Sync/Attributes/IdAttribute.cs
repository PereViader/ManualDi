using System;

namespace ManualDi.Sync
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
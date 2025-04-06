using System;

namespace ManualDi.Async
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class InjectAttribute : Attribute
    {
        public string? Id { get; }

        public InjectAttribute(string? id = null)
        {
            Id = id;
        }
    }
}
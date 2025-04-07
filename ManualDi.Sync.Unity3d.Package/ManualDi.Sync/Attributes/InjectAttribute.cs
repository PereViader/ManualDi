using System;

namespace ManualDi.Sync
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
using System;

namespace ManualDi.Async
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class CyclicDependencyAttribute : Attribute
    {
    }
}
using System;

namespace ManualDi.Async
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class CyclicDependencyAttribute : Attribute
    {
    }
}
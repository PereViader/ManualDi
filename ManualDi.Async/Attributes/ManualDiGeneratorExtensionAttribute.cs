using System;

namespace ManualDi.Async.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class ManualDiGeneratorExtensionAttribute : Attribute
    {
    }
}

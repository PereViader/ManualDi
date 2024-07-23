using System;

namespace ManualDi.Main
{
    [AttributeUsage(AttributeTargets.Property)]
    public class InjectAttribute : Attribute
    {
    }
}
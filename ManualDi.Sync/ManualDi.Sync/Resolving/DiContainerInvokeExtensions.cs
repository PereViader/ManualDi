using System;
using System.Collections.Generic;
using System.Reflection;

namespace ManualDi.Sync
{
    public static class DiContainerInvokeExtensions
    {
        public static object? InvokeDelegateUsingReflexion(this IDiContainer diContainer, Delegate @delegate)
        {
            var methodInfo = @delegate.Method;
            var parameters = methodInfo.GetParameters();
            var arguments = new object?[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var type = parameter.ParameterType;

                var underlyingType = Nullable.GetUnderlyingType(type);
                if (underlyingType is not null)
                {
                    arguments[i] = diContainer.ResolveContainer(underlyingType);
                    continue;
                }

                var resolution = diContainer.ResolveContainer(type);
                
                if (resolution is not null)
                {
                    arguments[i] = resolution;
                    continue;
                }

                if (IsNullable(parameter))
                {
                    arguments[i] = null;
                    continue;
                }

                throw new InvalidOperationException($"Could not resolve element of type {type.FullName} for parameter {parameter.Name}");
            }

            return @delegate.DynamicInvoke(arguments);
        }

        private static bool IsNullable(ParameterInfo parameter)
        {
            if (Nullable.GetUnderlyingType(parameter.ParameterType) != null)
            {
                return true;
            }

            if (parameter.ParameterType.IsValueType)
            {
                return false;
            }

            // Reference type
            return IsNullableReferenceType(parameter);
        }

        private static bool IsNullableReferenceType(ParameterInfo parameter)
        {
            if (TryGetNullableFlags(parameter.GetCustomAttributes(false), out var flags) && flags != null)
            {
                return flags[0] == 2;
            }

            // 2. Check method context
            if (TryGetNullableContext(parameter.Member.GetCustomAttributes(false), out var methodContext))
            {
                return methodContext == 2;
            }

            // 3. Check type context
            if (TryGetNullableContext(parameter.Member.DeclaringType.GetCustomAttributes(false), out var typeContext))
            {
                return typeContext == 2;
            }

            // Default to non-nullable (1) if no context found, or oblivious
            return false;
        }

        private static bool TryGetNullableFlags(object[] attributes, out byte[]? flags)
        {
            foreach (var attribute in attributes)
            {
                var type = attribute.GetType();
                if (type.FullName == "System.Runtime.CompilerServices.NullableAttribute")
                {
                    var field = type.GetField("NullableFlags");
                    if (field != null)
                    {
                        flags = field.GetValue(attribute) as byte[];
                        return flags != null;
                    }
                }
            }
            flags = null;
            return false;
        }

        private static bool TryGetNullableContext(object[] attributes, out byte context)
        {
            foreach (var attribute in attributes)
            {
                var type = attribute.GetType();
                if (type.FullName == "System.Runtime.CompilerServices.NullableContextAttribute")
                {
                    var field = type.GetField("Flag");
                    if (field != null)
                    {
                        context = (byte)field.GetValue(attribute);
                        return true;
                    }
                }
            }
            context = 0;
            return false;
        }
    }
}

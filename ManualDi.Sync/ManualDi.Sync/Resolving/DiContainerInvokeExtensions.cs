using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ManualDi.Sync
{
    public static class DiContainerInvokeExtensions
    {
        public static object? InvokeDelegateUsingReflexion(this IDiContainer diContainer, Delegate @delegate)
        {
            var arguments = ResolveParameters(diContainer, @delegate);
            return @delegate.DynamicInvoke(arguments);
        }

        public static async Task<object?> InvokeDelegateUsingReflexionAsync(this IDiContainer diContainer, Delegate @delegate)
        {
            var result = InvokeDelegateUsingReflexion(diContainer, @delegate);
            if (result is not Task task)
            {
                return result;
            }
            await task;
            var resultProperty = task.GetType().GetProperty("Result");
            if (resultProperty is null)
            {
                return null;
            }
            return resultProperty.GetValue(task);
        }

        private static object?[] ResolveParameters(IDiContainer diContainer, Delegate @delegate)
        {
            return @delegate.Method.GetParameters()
                .Select(parameter =>
                {
                    var type = parameter.ParameterType;
                    var underlyingType = Nullable.GetUnderlyingType(type);
                    var resolutionType = underlyingType ?? type;

                    if (resolutionType == typeof(IDiContainer))
                    {
                        return diContainer;
                    }

                    if (resolutionType == typeof(CancellationToken))
                    {
                        return diContainer.CancellationToken;
                    }
                    
                    var filter = CreateFilterForParameter(parameter);

                    var resolution = filter is null
                        ? diContainer.ResolveContainer(resolutionType)
                        : diContainer.ResolveContainer(resolutionType, filter);

                    if (resolution is not null)
                    {
                        return resolution;
                    }

                    if (IsNullable(parameter))
                    {
                        return null;
                    }

                    throw new InvalidOperationException($"Could not resolve element of type {type.FullName} for parameter {parameter.Name}");
                })
                .ToArray();
        }

        private static FilterBindingDelegate? CreateFilterForParameter(ParameterInfo parameter)
        {
            var idAttribute = parameter.GetCustomAttribute<IdAttribute>();
            if (idAttribute is null)
            {
                return null;
            }

            return c => c.Id(idAttribute.Id);
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
                return flags[0] is 2;
            }

            // 2. Check method context
            if (TryGetNullableContext(parameter.Member.GetCustomAttributes(false), out var methodContext))
            {
                return methodContext is 2;
            }

            // 3. Check type context
            if (TryGetNullableContext(parameter.Member.DeclaringType.GetCustomAttributes(false), out var typeContext))
            {
                return typeContext is 2;
            }

            // Default to non-nullable (1) if no context found, or oblivious
            return false;
        }

        private static bool TryGetNullableFlags(object[] attributes, out byte[]? flags)
        {
            foreach (var attribute in attributes)
            {
                var type = attribute.GetType();
                if (type.FullName is "System.Runtime.CompilerServices.NullableAttribute")
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
                if (type.FullName is "System.Runtime.CompilerServices.NullableContextAttribute")
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

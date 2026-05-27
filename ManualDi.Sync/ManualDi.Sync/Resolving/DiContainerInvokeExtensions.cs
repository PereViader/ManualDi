using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ManualDi.Sync
{
    public static class DiContainerInvokeExtensions
    {
        // Cache reflection lookups to avoid runtime overhead during delegate invocation
        private static readonly ConcurrentDictionary<Type, PropertyInfo?> TaskResultPropertyCache = new();
        private static readonly ConcurrentDictionary<Type, FieldInfo?> NullableFlagsFieldCache = new();
        private static readonly ConcurrentDictionary<Type, FieldInfo?> NullableContextFlagFieldCache = new();

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

            var taskType = task.GetType();
            var resultProperty = TaskResultPropertyCache.GetOrAdd(taskType, t => t.GetProperty("Result"));

            if (resultProperty is null)
            {
                return null;
            }
            return resultProperty.GetValue(task);
        }

        private static object?[] ResolveParameters(IDiContainer diContainer, Delegate @delegate)
        {
            var parameters = @delegate.Method.GetParameters();
            var resolvedParameters = new object?[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var type = parameter.ParameterType;
                var underlyingType = Nullable.GetUnderlyingType(type);
                var resolutionType = underlyingType ?? type;

                if (resolutionType == typeof(IDiContainer))
                {
                    resolvedParameters[i] = diContainer;
                    continue;
                }

                if (resolutionType == typeof(CancellationToken))
                {
                    resolvedParameters[i] = diContainer.CancellationToken;
                    continue;
                }

                var filter = CreateFilterForParameter(parameter);

                var resolution = filter is null
                    ? diContainer.ResolveContainer(resolutionType)
                    : diContainer.ResolveContainer(resolutionType, filter);

                if (resolution is not null)
                {
                    resolvedParameters[i] = resolution;
                    continue;
                }

                if (IsNullable(parameter))
                {
                    resolvedParameters[i] = null;
                    continue;
                }

                throw new InvalidOperationException($"Could not resolve element of type {type.FullName} for parameter {parameter.Name}");
            }

            return resolvedParameters;
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
                if (type.Name is "NullableAttribute" && type.FullName is "System.Runtime.CompilerServices.NullableAttribute")
                {
                    var field = NullableFlagsFieldCache.GetOrAdd(type, t => t.GetField("NullableFlags"));
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
                if (type.Name is "NullableContextAttribute" && type.FullName is "System.Runtime.CompilerServices.NullableContextAttribute")
                {
                    var field = NullableContextFlagFieldCache.GetOrAdd(type, t => t.GetField("Flag"));
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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ManualDi.Async
{
    internal static class ThrowHelper
    {
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowCouldNotResolveInstance(Type type)
        {
            throw new InvalidOperationException($"Could not resolve instance for binding of type {type.FullName}");
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowUnexpectedInitializationDelegateType(Type delegateType)
        {
            throw new InvalidOperationException($"Unexpected initialization delegate type: {delegateType}");
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowUnexpectedStartupDelegateType(Type delegateType)
        {
            throw new InvalidOperationException($"Unexpected startup delegate type: {delegateType}");
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowCouldNotCreateObject(Type concreteType)
        {
            throw new InvalidOperationException($"Could not create object for Binding with Concrete type {concreteType}");
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowFromDelegateIsNull(Type concreteType)
        {
            throw new InvalidOperationException($"The from delegate for Binding with Concrete type {concreteType} is null");
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowTypeNotRegistered(Type injectedType, Type? concreteType)
        {
            throw new InvalidOperationException($"Type {injectedType.FullName} injected into {concreteType?.FullName ?? "null"} is not registered.");
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowTypeWithFilterNotRegistered(Type injectedType, Type? concreteType)
        {
            throw new InvalidOperationException($"Type {injectedType.FullName} injected into {concreteType?.FullName ?? "null"} with some filter is not registered.");
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowCouldNotResolveParameter(Type type, string parameterName)
        {
            throw new InvalidOperationException($"Could not resolve element of type {type.FullName} for parameter {parameterName}");
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowCouldNotResolveElement(Type type)
        {
            throw new InvalidOperationException($"Could not resolve element of type {type.FullName}");
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowCouldNotResolveAsyncElement(Type type)
        {
            throw new InvalidOperationException($"Could not resolve async element of type {type.FullName}");
        }
    }
}

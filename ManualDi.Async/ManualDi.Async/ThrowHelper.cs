using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ManualDi.Async
{
    internal static class ThrowHelper
    {
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowCouldNotResolveInstance(Type type, IDiContainer? diContainer)
        {
            var injectedIntoType = diContainer?.InjectedBinding?.ConcreteType;
            throw new InvalidOperationException($"Could not resolve instance for binding of type {type.FullName} injected into {injectedIntoType?.FullName ?? "null"}");
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowUnexpectedInitializationDelegateType(object delegateObject)
        {
            throw new InvalidOperationException($"Unexpected initialization delegate type: {delegateObject.GetType()}");
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowUnexpectedStartupDelegateType(object delegateObject)
        {
            throw new InvalidOperationException($"Unexpected startup delegate type: {delegateObject.GetType()}");
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowCouldNotCreateObject(Binding binding)
        {
            throw new InvalidOperationException($"Could not create object for Binding with Concrete type {binding.ConcreteType}");
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowFromDelegateIsNull(Binding binding)
        {
            throw new InvalidOperationException($"The from delegate for Binding with Concrete type {binding.ConcreteType} is null");
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowTypeNotRegistered(Type injectedType, Binding? injectedBinding)
        {
            throw new InvalidOperationException($"Type {injectedType.FullName} injected into {injectedBinding?.ConcreteType.FullName ?? "null"} is not registered.");
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowCouldNotResolveParameter(ParameterInfo parameter, IDiContainer? diContainer)
        {
            var injectedIntoType = diContainer?.InjectedBinding?.ConcreteType;
            throw new InvalidOperationException($"Could not resolve element of type {parameter.ParameterType.FullName} for parameter {parameter.Name} injected into {injectedIntoType?.FullName ?? "null"}");
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowCouldNotResolveElement(Type type, IDiContainer? diContainer)
        {
            var injectedIntoType = diContainer?.InjectedBinding?.ConcreteType;
            throw new InvalidOperationException($"Could not resolve element of type {type.FullName} injected into {injectedIntoType?.FullName ?? "null"}");
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowCouldNotResolveAsyncElement(Type type, IDiContainer? diContainer)
        {
            var injectedIntoType = diContainer?.InjectedBinding?.ConcreteType;
            throw new InvalidOperationException($"Could not resolve async element of type {type.FullName} injected into {injectedIntoType?.FullName ?? "null"}");
        }

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowTypeNotRegisteredForInjection(Type type)
        {
            throw new InvalidOperationException($"Type {type.FullName} is not registered for injection in ManualDiInjector.");
        }
    }
}

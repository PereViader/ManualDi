using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace ManualDi.Sync.Generators
{
    [Generator]
    public class ManualDiInjectableSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var classData = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    "ManualDi.Sync.ManualDiInjectableAttribute",
                    IsSyntaxNodeValid,
                    GetClassData)
                .Where(x => x is not null);

            context.RegisterSourceOutput(classData, static (spc, data) => GenerateInjector(spc, data!));

            var assemblyFlags = context.CompilationProvider.Select(static (compilation, _) =>
            {
                var moduleInitializerSymbol = compilation.GetTypeByMetadataName("System.Runtime.CompilerServices.ModuleInitializerAttribute");
                var hasModuleInitializer = moduleInitializerSymbol is not null && compilation.IsSymbolAccessibleWithin(moduleInitializerSymbol, compilation.Assembly);

                return new AssemblyFlags(
                    HasModuleInitializer: hasModuleInitializer,
                    HasUnityPreserve: compilation.GetTypeByMetadataName("UnityEngine.Scripting.PreserveAttribute") is not null,
                    HasUnityRuntimeInitialize: compilation.GetTypeByMetadataName("UnityEngine.RuntimeInitializeOnLoadMethodAttribute") is not null
                );
            });

            var registrationData = assemblyFlags.Combine(classData.Collect());
            context.RegisterSourceOutput(registrationData, static (spc, tuple) => GenerateRegistration(spc, tuple.Left, tuple.Right!));
        }

        private static bool IsSyntaxNodeValid(SyntaxNode node, CancellationToken ct)
        {
            if (node is not ClassDeclarationSyntax classDeclarationSyntax)
            {
                return false;
            }

            if (classDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                return false;
            }

            return true;
        }

        private static ClassData? GetClassData(GeneratorAttributeSyntaxContext context, CancellationToken ct)
        {
            if (context.TargetSymbol is not INamedTypeSymbol symbol)
            {
                return null;
            }

            var wellKnownTypes = new WellKnownTypes(context.SemanticModel.Compilation);

            var accessibility = GetSymbolAccessibility(symbol);
            if (accessibility is not (Accessibility.Public or Accessibility.Internal))
            {
                return null;
            }

            var className = FullyQualifyTypeWithoutNullable(symbol);
            var injectorClassName = InjectorClassName(className);
            var fileName = injectorClassName;

            var typeParameters = symbol.TypeParameters.Length > 0
                ? string.Join(", ", symbol.TypeParameters.Select(x => x.Name))
                : null;

            var obsoleteText = wellKnownTypes.IsSymbolObsolete(symbol) ? "[System.Obsolete]\r\n" : "";
            var injectParameters = GetInjectMethodParameters(symbol, wellKnownTypes);
            var baseTypeCall = GetBaseInjectableCall(symbol, wellKnownTypes);
            var typeParameterConstraints = GetTypeParameterConstraints(symbol);
            var hasUnityPreserve = context.SemanticModel.Compilation.GetTypeByMetadataName("UnityEngine.Scripting.PreserveAttribute") is not null;

            return new ClassData(
                FileName: fileName,
                ClassName: className,
                InjectorClassName: injectorClassName,
                Namespace: symbol.ContainingNamespace.IsGlobalNamespace ? null : symbol.ContainingNamespace.ToDisplayString(),
                Accessibility: GetAccessibilityString(accessibility),
                TypeParameters: typeParameters,
                TypeParameterConstraints: typeParameterConstraints,
                ObsoleteText: obsoleteText,
                InjectMethodParameters: injectParameters,
                BaseTypeCall: baseTypeCall,
                IsAbstract: symbol.IsAbstract,
                IsOpenGeneric: symbol.TypeParameters.Length > 0,
                HasUnityPreserve: hasUnityPreserve
            );
        }

        private static string InjectorClassName(string className)
        {
            var sb = new StringBuilder("ManualDi_", className.Length + 20);
            foreach (var c in className)
            {
                switch (c)
                {
                    case '.':
                    case '<':
                    case ',':
                        sb.Append('_');
                        break;
                    case '>':
                    case ' ':
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            sb.Append("_Injector");
            return sb.ToString();
        }

        private static EquatableArray<Resolution> GetInjectMethodParameters(INamedTypeSymbol classSymbol, WellKnownTypes types)
        {
            var injectMethod = classSymbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .Where(x => x is { Name: "Inject", DeclaredAccessibility: Accessibility.Public or Accessibility.Internal, IsStatic: false })
                .OrderByDescending(x => x.DeclaredAccessibility)
                .FirstOrDefault();

            if (injectMethod is null)
            {
                return default;
            }

            var parameters = new List<Resolution>(injectMethod.Parameters.Length);
            foreach (var parameter in injectMethod.Parameters)
            {
                parameters.Add(CreateResolution(parameter, types));
            }

            return parameters;
        }

        private static BaseTypeCall? GetBaseInjectableCall(INamedTypeSymbol symbol, WellKnownTypes types)
        {
            var currentBase = symbol.BaseType;
            int depth = 0;
            while (currentBase != null && depth++ < 100)
            {
                if (currentBase.SpecialType == SpecialType.System_Object)
                {
                    return null;
                }

                if (types.HasManualDiInjectableAttribute(currentBase))
                {
                    var accessibility = GetSymbolAccessibility(currentBase);
                    if (accessibility is not (Accessibility.Public or Accessibility.Internal))
                    {
                        return null;
                    }

                    var baseClassName = FullyQualifyTypeWithoutNullable(currentBase.OriginalDefinition);
                    var baseInjectorClassName = InjectorClassName(baseClassName);
                    var typeArguments = new List<string>(currentBase.TypeArguments.Length);
                    foreach (var typeArgument in currentBase.TypeArguments)
                    {
                        typeArguments.Add(FullyQualifyTypeWithoutNullable(typeArgument));
                    }

                    return new BaseTypeCall(baseInjectorClassName, string.Join(", ", typeArguments));
                }

                currentBase = currentBase.BaseType;
            }

            return null;
        }

        private static string? GetTypeParameterConstraints(INamedTypeSymbol symbol)
        {
            if (symbol.TypeParameters.Length is 0)
            {
                return null;
            }

            var sb = new StringBuilder();
            foreach (var typeParameter in symbol.TypeParameters)
            {
                var constraints = new List<string>();
                if (typeParameter.HasReferenceTypeConstraint)
                {
                    constraints.Add("class");
                }
                if (typeParameter.HasValueTypeConstraint)
                {
                    constraints.Add("struct");
                }
                if (typeParameter.HasNotNullConstraint)
                {
                    constraints.Add("notnull");
                }
                if (typeParameter.HasUnmanagedTypeConstraint)
                {
                    constraints.Add("unmanaged");
                }

                foreach (var constraintType in typeParameter.ConstraintTypes)
                {
                    constraints.Add(constraintType.ToDisplayString());
                }

                if (typeParameter.HasConstructorConstraint)
                {
                    constraints.Add("new()");
                }

                if (constraints.Count > 0)
                {
                    sb.Append($" where {typeParameter.Name} : {string.Join(", ", constraints)}");
                }
            }

            return sb.ToString();
        }

        private static Resolution CreateResolution(IParameterSymbol parameter, WellKnownTypes types)
        {
            var typeSymbol = parameter.Type;
            var isOutParam = parameter.RefKind == RefKind.Out;
            if (isOutParam)
            {
                return OutResolution.Instance;
            }

            if (SymbolEqualityComparer.Default.Equals(typeSymbol, types.CancellationToken))
            {
                return CancellationTokenResolution.Instance;
            }

            if (SymbolEqualityComparer.Default.Equals(typeSymbol, types.DiContainer))
            {
                return ContainerResolution.Instance;
            }

            var keyedAttribute = parameter.GetAttributes()
                .FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, types.KeyedAttribute));

            string? keyTypeName = null;
            if (keyedAttribute is not null && keyedAttribute.ConstructorArguments.Length > 0 && keyedAttribute.ConstructorArguments[0].Value is ITypeSymbol keyTypeSymbol)
            {
                keyTypeName = FullyQualifyTypeWithoutNullable(keyTypeSymbol);
            }

            // Enumerable check
            var arraySymbol = (typeSymbol as IArrayTypeSymbol)?.ElementType;
            var listGenericType = arraySymbol ?? types.TryGetEnumerableType(typeSymbol);

            if (listGenericType is not null)
            {
                var isListNullable = IsNullableTypeSymbol(typeSymbol);
                var elementTypeWithNullability = FullyQualifyTypeWithNullable(listGenericType);
                var elementTypeNoNullable = FullyQualifyTypeWithoutNullable(listGenericType);
                var isElementNullable = IsNullableTypeSymbol(listGenericType);

                return new EnumerableResolution(
                    elementTypeNoNullable,
                    new EnumerableInfo(isListNullable, isElementNullable, elementTypeWithNullability, arraySymbol is not null)
                );
            }

            // Standard resolution
            var typeName = FullyQualifyTypeWithoutNullable(typeSymbol);
            var method = "Resolve";
            if (keyTypeName is not null)
            {
                method = IsNullableTypeSymbol(typeSymbol) ? "ResolveKeyedNullable" : "ResolveKeyed";
            }
            else if (IsNullableTypeSymbol(typeSymbol))
            {
                method = typeSymbol.IsValueType ? "ResolveNullableValue" : "ResolveNullable";
            }

            return new ServiceResolution(typeName, method, keyTypeName);
        }

        private static void GenerateInjector(SourceProductionContext context, ClassData data)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("""
            // <auto-generated/>
            #nullable enable
            #pragma warning disable
            using System.Runtime.CompilerServices;

            namespace ManualDi.Sync
            {
            """);

            if (data.HasUnityPreserve)
            {
                stringBuilder.AppendLine("    [UnityEngine.Scripting.Preserve]");
            }

            stringBuilder.AppendLine($$"""
                public static class {{data.InjectorClassName}}
                {
            """);

            var closedTypeParameters = data.TypeParameters is not null ? "<" + data.TypeParameters + ">" : "";

            stringBuilder.Append($$"""
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    {{data.ObsoleteText}}{{data.Accessibility}} static void Inject{{closedTypeParameters}}({{data.ClassName}} target, IDiContainer container){{data.TypeParameterConstraints}}
                    {

            """);

            if (data.BaseTypeCall is not null)
            {
                var typeArguments = !string.IsNullOrEmpty(data.BaseTypeCall.TypeArguments)
                    ? $"<{data.BaseTypeCall.TypeArguments}>"
                    : "";

                stringBuilder.AppendLine($"            {data.BaseTypeCall.BaseInjectorClassName}.Inject{typeArguments}(target, container);");
            }

            if (data.InjectMethodParameters.HasValue)
            {
                stringBuilder.Append("            target.Inject(");

                bool isFirst = true;
                foreach (var param in data.InjectMethodParameters)
                {
                    if (!isFirst) stringBuilder.AppendLine(",");
                    else { stringBuilder.AppendLine(); isFirst = false; }
                    stringBuilder.Append("                ");
                    AppendResolution(stringBuilder, param);
                }

                stringBuilder.AppendLine("""
            );
            """);
            }

            stringBuilder.AppendLine("""
                    }
                }
            }
            """);

            context.AddSource($"{data.FileName}.g.cs", SourceText.From(stringBuilder.ToString(), Encoding.UTF8));
        }

        private static void GenerateRegistration(SourceProductionContext context, AssemblyFlags flags, ImmutableArray<ClassData> classes)
        {
            var eligibleClasses = classes.Where(x => !x.IsAbstract && !x.IsOpenGeneric).ToList();
            if (eligibleClasses.Count == 0)
            {
                return;
            }

            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("""
            // <auto-generated/>
            #nullable enable
            #pragma warning disable
            using System.Runtime.CompilerServices;

            """);

            if (!flags.HasModuleInitializer)
            {
                stringBuilder.AppendLine("""
                namespace System.Runtime.CompilerServices
                {
                    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
                    internal sealed class ModuleInitializerAttribute : Attribute
                    {
                    }
                }

                """);
            }

            stringBuilder.AppendLine("namespace ManualDi.Sync");
            stringBuilder.AppendLine("{");

            if (flags.HasUnityPreserve)
            {
                stringBuilder.AppendLine("    [UnityEngine.Scripting.Preserve]");
            }

            stringBuilder.AppendLine("""
                internal static class ManualDi_Injection_Registration
                {
                    [System.Runtime.CompilerServices.ModuleInitializer]
            """);

            if (flags.HasUnityRuntimeInitialize)
            {
                stringBuilder.AppendLine("        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]");
            }

            if (flags.HasUnityPreserve)
            {
                stringBuilder.AppendLine("        [UnityEngine.Scripting.Preserve]");
            }

            stringBuilder.AppendLine("""
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    public static void Register()
                    {
            """);

            foreach (var data in eligibleClasses)
            {
                stringBuilder.AppendLine($"            ManualDiInjector.Register(typeof({data.ClassName}), static (o, c) => {data.InjectorClassName}.Inject(({data.ClassName})o, c));");
            }

            stringBuilder.AppendLine("""
                    }
                }
            }
            """);

            context.AddSource("ManualDi_Injection_Registration.g.cs", SourceText.From(stringBuilder.ToString(), Encoding.UTF8));
        }

        private static void AppendResolution(StringBuilder sb, Resolution resolution)
        {
            switch (resolution)
            {
                case OutResolution:
                    sb.Append("out _");
                    return;
                case CancellationTokenResolution:
                    sb.Append("container.CancellationToken");
                    return;
                case ContainerResolution:
                    sb.Append("container");
                    return;
                case EnumerableResolution enumRes:
                    var info = enumRes.EnumerableInfo;
                    if (info.IsListNullable)
                    {
                        sb.Append("container.WouldResolve<");
                        sb.Append(enumRes.TypeName);
                        sb.Append(">() ? ");
                    }

                    sb.Append("container.ResolveAll<");
                    sb.Append(enumRes.TypeName);
                    sb.Append(">()");

                    if (info.IsElementNullable)
                    {
                        sb.Append(".ConvertAll<");
                        sb.Append(info.ElementTypeWithNullability);
                        sb.Append(">(x => x)");
                    }

                    if (info.IsArray)
                    {
                        sb.Append(".ToArray()");
                    }

                    if (info.IsListNullable)
                    {
                        sb.Append(" : null");
                    }
                    return;
                case ServiceResolution serviceRes:
                    sb.Append("container.");
                    sb.Append(serviceRes.ResolutionMethod);
                    sb.Append("<");
                    sb.Append(serviceRes.TypeName);
                    if (serviceRes.KeyTypeName is not null)
                    {
                        sb.Append(", ");
                        sb.Append(serviceRes.KeyTypeName);
                    }
                    sb.Append(">()");
                    return;
            }
        }

        private static string FullyQualifyTypeWithoutNullable(ITypeSymbol? typeSymbol)
        {
            if (typeSymbol is null)
            {
                return string.Empty;
            }

            var nonNullableType = GetNonNullableType(typeSymbol);
            if (nonNullableType is not null)
            {
                return nonNullableType.ToDisplayString();
            }

            return typeSymbol.ToDisplayString();
        }

        private static bool IsNullableTypeSymbol(ITypeSymbol? typeSymbol)
        {
            if (typeSymbol is null)
            {
                return false;
            }

            return typeSymbol.NullableAnnotation is NullableAnnotation.Annotated ||
                   typeSymbol.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T;
        }

        private static ITypeSymbol? GetNonNullableType(ITypeSymbol? typeSymbol)
        {
            if (typeSymbol is null)
            {
                return null;
            }

            if (typeSymbol.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T &&
                typeSymbol is INamedTypeSymbol namedTypeSymbol)
            {
                return namedTypeSymbol.TypeArguments[0];
            }

            if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
            {
                return typeSymbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
            }

            return null;
        }

        private static string FullyQualifyTypeWithNullable(ITypeSymbol typeSymbol)
        {
            return typeSymbol.ToDisplayString();
        }

        private static string GetAccessibilityString(Accessibility accessibility)
        {
            return accessibility switch
            {
                Accessibility.Internal => "internal",
                Accessibility.Protected => "protected",
                Accessibility.Private => "private",
                Accessibility.Public => "public",
                _ => throw new ArgumentOutOfRangeException(nameof(accessibility), accessibility, null),
            };
        }

        private static Accessibility GetSymbolAccessibility(ISymbol symbol)
        {
            var visibility = symbol.DeclaredAccessibility;
            var currentSymbol = symbol.ContainingType;

            while (currentSymbol is not null)
            {
                var parentVisibility = currentSymbol.DeclaredAccessibility;
                if (parentVisibility < symbol.DeclaredAccessibility)
                {
                    visibility = parentVisibility;
                }
                currentSymbol = currentSymbol.ContainingType;
            }

            return visibility;
        }

        internal record AssemblyFlags(
            bool HasModuleInitializer,
            bool HasUnityPreserve,
            bool HasUnityRuntimeInitialize
        );

        internal record ClassData(
            string FileName,
            string ClassName,
            string InjectorClassName,
            string? Namespace,
            string Accessibility,
            string? TypeParameters,
            string? TypeParameterConstraints,
            string ObsoleteText,
            EquatableArray<Resolution> InjectMethodParameters,
            BaseTypeCall? BaseTypeCall,
            bool IsAbstract,
            bool IsOpenGeneric,
            bool HasUnityPreserve
        );

        internal record BaseTypeCall(string BaseInjectorClassName, string TypeArguments);

        internal abstract record Resolution;

        internal sealed record ServiceResolution(string TypeName, string ResolutionMethod, string? KeyTypeName = null) : Resolution;

        internal sealed record EnumerableResolution(string TypeName, EnumerableInfo EnumerableInfo) : Resolution;

        internal sealed record OutResolution : Resolution
        {
            public static readonly OutResolution Instance = new();
        }

        internal sealed record CancellationTokenResolution : Resolution
        {
            public static readonly CancellationTokenResolution Instance = new();
        }

        internal sealed record ContainerResolution : Resolution
        {
            public static readonly ContainerResolution Instance = new();
        }

        internal record EnumerableInfo(bool IsListNullable, bool IsElementNullable, string ElementTypeWithNullability, bool IsArray);

        private readonly struct WellKnownTypes(Compilation compilation)
        {
            public readonly INamedTypeSymbol? List = compilation.GetTypeByMetadataName("System.Collections.Generic.List`1");
            public readonly INamedTypeSymbol? IList = compilation.GetTypeByMetadataName("System.Collections.Generic.IList`1");
            public readonly INamedTypeSymbol? IReadOnlyList = compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlyList`1");
            public readonly INamedTypeSymbol? IEnumerable = compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1");
            public readonly INamedTypeSymbol? IReadOnlyCollection = compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlyCollection`1");
            public readonly INamedTypeSymbol? ICollection = compilation.GetTypeByMetadataName("System.Collections.Generic.ICollection`1");
            public readonly INamedTypeSymbol? ManualDiInjectableAttribute = compilation.GetTypeByMetadataName("ManualDi.Sync.ManualDiInjectableAttribute");
            public readonly INamedTypeSymbol? KeyedAttribute = compilation.GetTypeByMetadataName("ManualDi.Sync.KeyedAttribute");
            public readonly INamedTypeSymbol? ObsoleteAttribute = compilation.GetTypeByMetadataName("System.ObsoleteAttribute");
            public readonly INamedTypeSymbol? DiContainer = compilation.GetTypeByMetadataName("ManualDi.Sync.IDiContainer");
            public readonly INamedTypeSymbol? CancellationToken = compilation.GetTypeByMetadataName("System.Threading.CancellationToken");

            public ITypeSymbol? TryGetEnumerableType(ITypeSymbol typeSymbol)
            {
                if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
                {
                    if (SymbolEqualityComparer.Default.Equals(namedTypeSymbol.OriginalDefinition, List) ||
                        SymbolEqualityComparer.Default.Equals(namedTypeSymbol.OriginalDefinition, IReadOnlyList) ||
                        SymbolEqualityComparer.Default.Equals(namedTypeSymbol.OriginalDefinition, IList) ||
                        SymbolEqualityComparer.Default.Equals(namedTypeSymbol.OriginalDefinition, ICollection) ||
                        SymbolEqualityComparer.Default.Equals(namedTypeSymbol.OriginalDefinition, IReadOnlyCollection) ||
                        SymbolEqualityComparer.Default.Equals(namedTypeSymbol.OriginalDefinition, IEnumerable))
                    {
                        return namedTypeSymbol.TypeArguments[0];
                    }
                }
                return null;
            }

            public bool IsSymbolObsolete(ISymbol typeSymbol)
            {
                foreach (var attribute in typeSymbol.GetAttributes())
                {
                    if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, ObsoleteAttribute))
                    {
                        return true;
                    }
                }
                return false;
            }

            public bool HasManualDiInjectableAttribute(INamedTypeSymbol symbol)
            {
                foreach (var attribute in symbol.GetAttributes())
                {
                    if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, ManualDiInjectableAttribute))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;

namespace ManualDi.Sync.Generators
{
    [Generator]
    public class ManualDiSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var classData = context.SyntaxProvider
                .CreateSyntaxProvider(IsSyntaxNodeValid, GetClassData)
                .Where(x => x is not null);

            context.RegisterSourceOutput(classData, Generate!);
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
            
            if (classDeclarationSyntax.TypeParameterList is not null) //TODO: Patch out for now
            {
                return false;
            }

            // this searches for any property with using SyntaxKind.RequiredKeyword, the keyword is not available in CodeAnalysis 4.1.0
            if (classDeclarationSyntax.Members
                .OfType<PropertyDeclarationSyntax>()
                .Any(p => p.Modifiers.Any(m => m.IsKind((SyntaxKind)8447))))
            {
                return false;
            }

            return true;
        }

        private static ClassData? GetClassData(GeneratorSyntaxContext context, CancellationToken ct)
        {
            var classDeclaration = (ClassDeclarationSyntax)context.Node;
            var symbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration, ct);
            if (symbol is null || symbol.Locations.Length > 1)
            {
                return null;
            }

            var accessibility = GetSymbolAccessibility(symbol);
            if (accessibility is not (Accessibility.Public or Accessibility.Internal))
            {
                return null;
            }

            var className = FullyQualifyTypeWithoutNullable(symbol);
            var fileName = ExtensionFileName(className);

            var typeParameters = symbol.TypeParameters.Length > 0
                ? string.Join(", ", symbol.TypeParameters.Select(x => x.Name))
                : null;

            var wellKnownTypes = new WellKnownTypes(context.SemanticModel.Compilation);
            var obsoleteText = wellKnownTypes.IsSymbolObsolete(symbol) ? "[System.Obsolete]\r\n" : "";
            var constructorParameters = GetConstructorParameters(symbol, wellKnownTypes);
            var injectParameters = GetInjectMethodParameters(symbol, wellKnownTypes);
            var hasInitializeMethod = HasInitializeMethod(symbol);
            var isDisposable = wellKnownTypes.IsIDisposable(symbol);
            var baseTypeCall = GetBaseTypeCall(symbol, wellKnownTypes, context.SemanticModel.Compilation, ct);

            return new ClassData(
                FileName: fileName,
                ClassName: className,
                Namespace: symbol.ContainingNamespace.IsGlobalNamespace ? null : symbol.ContainingNamespace.ToDisplayString(),
                Accessibility: GetAccessibilityString(accessibility),
                TypeParameters: typeParameters,
                ObsoleteText: obsoleteText,
                ConstructorParameters: constructorParameters,
                InjectMethodParameters: injectParameters,
                HasInitializeMethod: hasInitializeMethod,
                IsDisposable: isDisposable,
                BaseTypeCall: baseTypeCall,
                IsSealed: symbol.IsSealed
            );
        }

        private static string ExtensionFileName(string className)
        {
            var name = className
                .Replace(".", "_")
                .Replace("<", "_")
                .Replace(">", "")
                .Replace(",", "_")
                .Replace(" ", "");

            return $"ManualDi_{name}_Extensions";
        }

        private static List<Resolution>? GetConstructorParameters(INamedTypeSymbol classSymbol, WellKnownTypes types)
        {
            if (classSymbol.IsAbstract)
            {
                return null;
            }

            var constructors = classSymbol
                .Constructors
                .Where(c => c.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal)
                .OrderByDescending(x => x.DeclaredAccessibility)
                .ToArray();

            if (constructors.Length == 0)
            {
                return null;
            }

            var constructor = constructors[0];
            var parameters = new List<Resolution>();

            foreach (var parameter in constructor.Parameters)
            {
                parameters.Add(CreateResolution(parameter, types));
            }

            return parameters;
        }

        private static List<Resolution>? GetInjectMethodParameters(INamedTypeSymbol classSymbol, WellKnownTypes types)
        {
            var injectMethod = classSymbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .Where(x => x is { Name: "Inject", DeclaredAccessibility: Accessibility.Public or Accessibility.Internal, IsStatic: false })
                .OrderByDescending(x => x.DeclaredAccessibility)
                .FirstOrDefault();

            if (injectMethod is null)
            {
                return null;
            }

            var parameters = new List<Resolution>(injectMethod.Parameters.Length);
            foreach (var parameter in injectMethod.Parameters)
            {
                parameters.Add(CreateResolution(parameter, types));
            }

            return parameters;
        }

        private static bool HasInitializeMethod(INamedTypeSymbol classSymbol)
        {
            return classSymbol
               .GetMembers()
               .OfType<IMethodSymbol>()
               .Any(x => x is
               {
                   Name: "Initialize",
                   DeclaredAccessibility: Accessibility.Public or Accessibility.Internal,
                   ReturnsVoid: true,
                   IsStatic: false,
                   Parameters.Length: 0
               });
        }

        private static BaseTypeCall? GetBaseTypeCall(INamedTypeSymbol classSymbol, WellKnownTypes types, Compilation compilation, CancellationToken ct)
        {
            var baseType = classSymbol.BaseType;
            if (baseType is null) return null;

            if (baseType.SpecialType is SpecialType.System_Object)
            {
                return null;
            }

            var accessibility = GetSymbolAccessibility(baseType);
            if (accessibility is not (Accessibility.Public or Accessibility.Internal))
            {
                return null;
            }

            bool shouldCallBase = false;
            if (baseType.Locations.Any(static x => x.IsInSource))
            {
                shouldCallBase = baseType.DeclaringSyntaxReferences
                    .Any(x => IsSyntaxNodeValid(x.GetSyntax(), ct));
            }
            else
            {
                var className = FullyQualifyTypeWithoutNullable(baseType.OriginalDefinition);
                var expectedTypeName = ExtensionFileName(className);
                shouldCallBase = compilation.GetTypeByMetadataName(expectedTypeName) is not null;
            }

            if (!shouldCallBase) return null;

            var baseClassName = FullyQualifyTypeWithoutNullable(baseType.OriginalDefinition);
            var baseExtensionsClassName = ExtensionFileName(baseClassName);
            var typeArguments = new List<string>(baseType.TypeArguments.Length);
            foreach (var typeArgument in baseType.TypeArguments)
            {
                typeArguments.Add(FullyQualifyTypeWithoutNullable(typeArgument));
            }

            return new BaseTypeCall(baseExtensionsClassName, string.Join(", ", typeArguments));
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

            var injectAttribute = parameter.GetAttributes()
                .FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, types.IdAttribute));

            string? injectId = null;
            if (injectAttribute is not null && injectAttribute.ConstructorArguments.Length > 0 && injectAttribute.ConstructorArguments[0].Value is object val)
            {
                injectId = $"\"{val}\"";
            }

            // Enumerable check
            var arraySymbol = (typeSymbol as IArrayTypeSymbol)?.ElementType;
            var listGenericType = arraySymbol ?? types.TryGetEnumerableType(typeSymbol);

            if (listGenericType is not null)
            {
                var isListNullable = IsNullableTypeSymbol(typeSymbol); // Is the LIST itself nullable
                var elementTypeWithNullability = FullyQualifyTypeWithNullable(listGenericType);
                var elementTypeNoNullable = FullyQualifyTypeWithoutNullable(listGenericType);
                var isElementNullable = IsNullableTypeSymbol(listGenericType);

                return new EnumerableResolution(
                    elementTypeNoNullable,
                    injectId,
                    new EnumerableInfo(isListNullable, isElementNullable, elementTypeWithNullability, arraySymbol is not null)
                );
            }

            // Standard resolution
            var typeName = FullyQualifyTypeWithoutNullable(typeSymbol);
            var method = "Resolve";
            if (IsNullableTypeSymbol(typeSymbol))
            {
                method = typeSymbol.IsValueType ? "ResolveNullableValue" : "ResolveNullable";
            }

            return new ServiceResolution(typeName, injectId, method);
        }

        private void Generate(SourceProductionContext context, ClassData data)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($$"""
            #nullable enable
            using System.Runtime.CompilerServices;

            namespace ManualDi.Sync
            {
                public static class {{data.FileName}}
                {
            """);

            var closedTypeParameters = (data.TypeParameters is not null ? "<" + data.TypeParameters + ">" : "");

            // FromConstructor
            if (data.ConstructorParameters is not null)
            {
                stringBuilder.Append($$"""
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    {{data.ObsoleteText}}{{data.Accessibility}} static Binding<{{data.ClassName}}> FromConstructor{{closedTypeParameters}}(this Binding<{{data.ClassName}}> binding)
                    {
                        return binding.FromMethod(static c => new {{data.ClassName}}(
            """);

                bool isFirst = true;
                foreach (var param in data.ConstructorParameters)
                {
                    if (!isFirst) stringBuilder.AppendLine(",");
                    else { stringBuilder.AppendLine(); isFirst = false; }
                    stringBuilder.Append("                ");
                    AppendResolution(stringBuilder, param);
                }

                stringBuilder.AppendLine("""
            ));
                    }

            """);
            }

            // Default
            stringBuilder.Append($$"""
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    {{data.ObsoleteText}}{{data.Accessibility}} static Binding<{{data.ClassName}}> Default{{closedTypeParameters}}(this Binding<{{data.ClassName}}> binding)
                    {
            
            """);

            if (data.IsSealed)
            {
                AppendDefaultImplBody(stringBuilder, data);
                stringBuilder.AppendLine("""
                        }
                """);
            }
            else
            {
                var defaultImplTypeParameters = data.TypeParameters is not null
                    ? $"<TDefaultImpl, {data.TypeParameters}>"
                    : "<TDefaultImpl>";

                stringBuilder.Append($$"""
                            return DefaultImpl{{(data.TypeParameters is not null ? $"<{data.ClassName}, {data.TypeParameters}>" : "")}}(binding);
                        }

                        [MethodImpl(MethodImplOptions.AggressiveInlining)]
                        {{data.ObsoleteText}}{{data.Accessibility}} static Binding<TDefaultImpl> DefaultImpl{{defaultImplTypeParameters}}(Binding<TDefaultImpl> binding) where TDefaultImpl : {{data.ClassName}}
                        {

                """);

                AppendDefaultImplBody(stringBuilder, data);

                stringBuilder.AppendLine("""
                        }
                """);
            }

            stringBuilder.AppendLine("""
                }
            }
            """);

            context.AddSource($"{data.FileName}.g.cs", SourceText.From(stringBuilder.ToString(), Encoding.UTF8));
        }

        private static void AppendDefaultImplBody(StringBuilder stringBuilder, ClassData data)
        {
            if (data.BaseTypeCall is not null)
            {
                var typeArguments = data.IsSealed
                    ? $"{data.ClassName}"
                    : "TDefaultImpl";

                if (!string.IsNullOrEmpty(data.BaseTypeCall.TypeArguments))
                {
                    typeArguments += $", {data.BaseTypeCall.TypeArguments}";
                }

                stringBuilder.AppendLine($"            {data.BaseTypeCall.BaseExtensionsClassName}.DefaultImpl<{typeArguments}>(binding);");
            }

            stringBuilder.Append("            return binding");

            if (data.HasInitializeMethod)
            {
                stringBuilder.Append("""

                                .Initialize(static (o, c) => ((
                """);
                stringBuilder.Append(data.ClassName);
                stringBuilder.Append(")o).Initialize())");
            }

            if (data.InjectMethodParameters is not null)
            {
                stringBuilder.Append("""

                                .Inject(static (o, c) =>
                                {
                                    var to = (
                """);
                stringBuilder.Append(data.ClassName);
                stringBuilder.Append("""
                )o;
                                    to.Inject(
                """);

                bool isFirst = true;
                foreach (var param in data.InjectMethodParameters)
                {
                    if (!isFirst) stringBuilder.AppendLine(",");
                    else { stringBuilder.AppendLine(); isFirst = false; }
                    stringBuilder.Append("                        ");
                    AppendResolution(stringBuilder, param);
                }

                stringBuilder.Append("""
                );
                                })
                """);
            }

            if (!data.IsDisposable)
            {
                stringBuilder.Append("""

                                .SkipDisposable()
                """);
            }

            stringBuilder.AppendLine(";");
        }

        private static void AppendResolution(StringBuilder sb, Resolution resolution)
        {
            switch (resolution)
            {
                case OutResolution:
                    sb.Append("out _");
                    return;
                case CancellationTokenResolution:
                    sb.Append("c.CancellationToken");
                    return;
                case ContainerResolution:
                    sb.Append("c");
                    return;
                case EnumerableResolution enumRes:
                    var idCode = enumRes.InjectId is null ? "" : $"static x => x.Id({enumRes.InjectId})";
                    var info = enumRes.EnumerableInfo;
                    if (info.IsListNullable)
                    {
                        sb.Append("c.WouldResolve<");
                        sb.Append(enumRes.TypeName);
                        sb.Append(">(");
                        sb.Append(idCode);
                        sb.Append(") ? ");
                    }

                    sb.Append("c.ResolveAll<");
                    sb.Append(enumRes.TypeName);
                    sb.Append(">(");
                    sb.Append(idCode);
                    sb.Append(")");

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
                    var idCodeSvc = serviceRes.InjectId is null ? "" : $"static x => x.Id({serviceRes.InjectId})";
                    sb.Append("c.");
                    sb.Append(serviceRes.ResolutionMethod); // e.g. ResolveNullable
                    sb.Append("<");
                    sb.Append(serviceRes.TypeName);
                    sb.Append(">(");
                    sb.Append(idCodeSvc);
                    sb.Append(")");
                    return;
            }
        }

        private static string FullyQualifyTypeWithoutNullable(ITypeSymbol typeSymbol)
        {
            var nonNullableType = GetNonNullableType(typeSymbol);
            if (nonNullableType is not null)
            {
                return nonNullableType.ToDisplayString();
            }

            return typeSymbol.ToDisplayString();
        }

        private static bool IsNullableTypeSymbol(ITypeSymbol typeSymbol)
        {
            return typeSymbol.NullableAnnotation is NullableAnnotation.Annotated ||
                   typeSymbol.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T;
        }

        private static ITypeSymbol? GetNonNullableType(ITypeSymbol typeSymbol)
        {
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

        internal record ClassData(
            string FileName,
            string ClassName,
            string? Namespace,
            string Accessibility,
            string? TypeParameters,
            string ObsoleteText,
            List<Resolution>? ConstructorParameters,
            List<Resolution>? InjectMethodParameters,
            bool HasInitializeMethod,
            bool IsDisposable,
            BaseTypeCall? BaseTypeCall,
            bool IsSealed
        );

        internal record BaseTypeCall(string BaseExtensionsClassName, string TypeArguments);

        internal abstract record Resolution;

        internal sealed record ServiceResolution(string TypeName, string? InjectId, string ResolutionMethod) : Resolution;

        internal sealed record EnumerableResolution(string TypeName, string? InjectId, EnumerableInfo EnumerableInfo) : Resolution;

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
            public readonly INamedTypeSymbol? IdAttribute = compilation.GetTypeByMetadataName("ManualDi.Sync.IdAttribute");
            public readonly INamedTypeSymbol? ObsoleteAttribute = compilation.GetTypeByMetadataName("System.ObsoleteAttribute");
            public readonly INamedTypeSymbol? IDisposable = compilation.GetTypeByMetadataName("System.IDisposable");
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

            public bool IsIDisposable(INamedTypeSymbol namedTypeSymbol)
            {
                foreach (var implementedInterface in namedTypeSymbol.AllInterfaces)
                {
                    if (SymbolEqualityComparer.Default.Equals(implementedInterface, IDisposable))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}

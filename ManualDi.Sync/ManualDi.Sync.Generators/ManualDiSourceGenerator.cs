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
    public record struct GenerationContext(StringBuilder StringBuilder, string ClassName, string TypeParameters, INamedTypeSymbol ClassSymbol, string ObsoleteText, TypeReferences TypeReferences, CancellationToken CancellationToken);

    [Generator]
    public class ManualDiSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var typeReferencesProvider = context.CompilationProvider
                .Select(TypeReferences.Create);

            //https://www.thinktecture.com/en/net/roslyn-source-generators-code-according-to-dependencies/
            // Create a provider for metadata references
            var classProvider = context.SyntaxProvider
                .CreateSyntaxProvider(IsSyntaxNodeValid, GetClassDeclaration)
                .Where(x => x is not null);

            var combined = classProvider.Combine(typeReferencesProvider);

            context.RegisterSourceOutput(combined, Generate!);
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

            // this searches for any property with using SyntaxKind.RequiredKeyword, the keyword is not available in CodeAnalysis 4.1.0
            if (classDeclarationSyntax.Members
                .OfType<PropertyDeclarationSyntax>()
                .Any(p => p.Modifiers.Any(m => m.IsKind((SyntaxKind)8447))))
            {
                return false;
            }

            return true;
        }

        private static INamedTypeSymbol? GetClassDeclaration(GeneratorSyntaxContext context, CancellationToken ct)
        {
            var symbol = context.SemanticModel.GetDeclaredSymbol((ClassDeclarationSyntax)context.Node, ct);
            if (symbol is null || symbol.Locations.Length > 1)
            {
                return null;
            }

            return symbol;
        }

        private void Generate(SourceProductionContext context, (INamedTypeSymbol, TypeReferences?) arg)
        {
            var (classSymbol, typeReferences) = arg;
            if (typeReferences is null)
            {
                return;
            }

            var accessibility = GetSymbolAccessibility(classSymbol);
            if (accessibility is not (Accessibility.Public or Accessibility.Internal))
            {
                return;
            }

            var className = FullyQualifyTypeWithoutNullable(classSymbol);
            var fileName = className
                .Replace(".", "_")
                .Replace("<", "_")
                .Replace(">", "_");

            var typeParameters = classSymbol.TypeParameters.Length > 0
                ? $"<{string.Join(", ", classSymbol.TypeParameters.Select(x => x.Name))}>"
                : "";

            var obsoleteText = typeReferences.IsSymbolObsolete(classSymbol)
                ? "[System.Obsolete]\r\n"
                : "";

            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($$"""
            #nullable enable
            using System.Runtime.CompilerServices;

            namespace ManualDi.Sync
            {
                public static class ManualDiGenerated{{fileName}}Extensions
                {
            """);

            var generationContext = new GenerationContext(stringBuilder, className, typeParameters, classSymbol, obsoleteText, typeReferences, context.CancellationToken);

            if (!typeReferences.IsUnityEngineObject(classSymbol))
            {
                AddFromConstructor(generationContext);
            }

            AddDefault(generationContext, accessibility);

            stringBuilder.AppendLine("""
                }
            }
            """);
            
            context.AddSource($"ManualDiGeneratedExtensions.{fileName}.g.cs", SourceText.From(stringBuilder.ToString(), Encoding.UTF8));
        }

        private static void ReportTypeNotFound(string typeName, SourceProductionContext context)
        {
            var descriptor = new DiagnosticDescriptor(
                id: "MDI001",
                title: "Type Not Found",
                messageFormat: "The type '{0}' could not be found. Ensure the necessary assemblies are referenced.",
                category: "ManualDiGenerator",
                DiagnosticSeverity.Error,
                isEnabledByDefault: true);

            var diagnostic = Diagnostic.Create(descriptor, Location.None, typeName);
            context.ReportDiagnostic(diagnostic);
        }

        private static string? GetInjectId(AttributeData attributeData)
        {
            var idArgument = attributeData.ConstructorArguments[0];
            if (idArgument.Value is null)
            {
                return null;
            }

            return $"\"{idArgument.Value}\"";
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
            // Recursively determine the effective accessibility of the type
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

        private static void AddFromConstructor(GenerationContext context)
        {
            if (context.ClassSymbol.IsAbstract)
            {
                return;
            }

            var constructors = context.ClassSymbol
                .Constructors
                .Where(c => c.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal)
                .OrderByDescending(x => x.DeclaredAccessibility)
                .ToArray();

            if (constructors.Length == 0)
            {
                return;
            }
            var constructor = constructors[0];

            var accessibility = GetSymbolAccessibility(constructor);
            var accessibilityString = GetAccessibilityString(accessibility);

            context.StringBuilder.Append($$"""
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    {{context.ObsoleteText}}{{accessibilityString}} static Binding<{{context.ClassName}}> FromConstructor{{context.TypeParameters}}(this Binding<{{context.ClassName}}> binding)
                    {
                        return binding.FromMethod(static c => new {{context.ClassName}}(
            """);

            CreateMethodResolution(constructor, "                ", context.TypeReferences, context.StringBuilder);

            context.StringBuilder.AppendLine($$"""
            ));
                    }

            """);
        }

        private static void CreateMethodResolution(IMethodSymbol constructor, string tabs, TypeReferences typeReferences, StringBuilder stringBuilder)
        {
            bool isFirst = true;
            foreach (var parameter in constructor.Parameters)
            {
                if (!isFirst)
                {
                    stringBuilder.AppendLine(",");
                }
                else
                {
                    stringBuilder.AppendLine();
                    isFirst = false;
                }

                stringBuilder.Append(tabs);
                CreateTypeResolution(parameter, typeReferences, stringBuilder);
            }
        }

        private static void CreateIdResolution(string? id, StringBuilder stringBuilder)
        {
            if (id is not null)
            {
                stringBuilder.Append("static x => x.Id(");
                stringBuilder.Append(id);
                stringBuilder.Append(")");
            }
        }

        private static void CreateTypeResolution(IParameterSymbol parameterSymbol, TypeReferences typeReferences, StringBuilder stringBuilder)
        {
            var attribute = typeReferences.GetIdAttribute(parameterSymbol);
            var id = attribute is null ? null : GetInjectId(attribute);
            var typeSymbol = parameterSymbol.Type;
            var isOutParam = parameterSymbol.RefKind == RefKind.Out;

            if (isOutParam)
            {
                stringBuilder.Append("out _");
                return;
            }

            if (typeReferences.IsCancellationToken(typeSymbol))
            {
                stringBuilder.Append("c.CancellationToken");
                return;
            }

            if (typeReferences.IsSymbolDiContainer(typeSymbol))
            {
                stringBuilder.Append("c");
                return;
            }

            // Updated code below
            var arraySymbol = (typeSymbol as IArrayTypeSymbol)?.ElementType;
            var listGenericType = arraySymbol ?? typeReferences.TryGetEnumerableType(typeSymbol);
            if (listGenericType is not null)
            {
                var isListNullable = IsNullableTypeSymbol(typeSymbol);
                if (isListNullable)
                {
                    stringBuilder.Append("c.WouldResolve<");
                    stringBuilder.Append(FullyQualifyTypeWithoutNullable(listGenericType));
                    stringBuilder.Append(">(");
                    CreateIdResolution(id, stringBuilder);
                    stringBuilder.Append(") ? ");
                }

                stringBuilder.Append("c.ResolveAll<");
                stringBuilder.Append(FullyQualifyTypeWithoutNullable(listGenericType));
                stringBuilder.Append(">(");
                CreateIdResolution(id, stringBuilder);
                stringBuilder.Append(")");

                if (IsNullableTypeSymbol(listGenericType))
                {
                    stringBuilder.Append(".ConvertAll<");
                    stringBuilder.Append(FullyQualifyTypeWithNullable(listGenericType));
                    stringBuilder.Append(">(x => x)");
                }

                if (arraySymbol is not null)
                {
                    stringBuilder.Append(".ToArray()");
                }

                if (isListNullable)
                {
                    stringBuilder.Append(" : null");
                }

                return;
            }

            stringBuilder.Append("c.");
            stringBuilder.Append(CreateContainerResolutionMethod(typeSymbol));
            stringBuilder.Append("<");
            stringBuilder.Append(FullyQualifyTypeWithoutNullable(typeSymbol));
            stringBuilder.Append(">(");
            CreateIdResolution(id, stringBuilder);
            stringBuilder.Append(")");
        }

        private static string CreateContainerResolutionMethod(ITypeSymbol typeSymbol)
        {
            if (!IsNullableTypeSymbol(typeSymbol))
            {
                return "Resolve";
            }

            if (typeSymbol.IsValueType)
            {
                return "ResolveNullableValue";
            }

            return "ResolveNullable";
        }

        private static bool AddInitialize(GenerationContext context, bool isOnNewLine)
        {
            var initializeMethod = context.ClassSymbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .FirstOrDefault(x => x is
                {
                    Name: "Initialize",
                    DeclaredAccessibility: Accessibility.Public or Accessibility.Internal,
                    ReturnsVoid: true,
                    IsStatic: false,
                    Parameters.Length: 0
                });

            if (initializeMethod is null)
            {
                return isOnNewLine;
            }

            if (isOnNewLine)
            {
                context.StringBuilder.AppendLine();
                context.StringBuilder.Append("                    ");
            }

            context.StringBuilder.Append(".Initialize(static (o, c) => ((");
            context.StringBuilder.Append(context.ClassName);
            context.StringBuilder.Append(")o).Initialize())");
            return true;
        }

        private static bool AddInject(GenerationContext context, bool isOnNewLine)
        {
            var injectMethod = context.ClassSymbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .Where(x => x is { Name: "Inject", DeclaredAccessibility: Accessibility.Public or Accessibility.Internal, IsStatic: false })
                .OrderByDescending(x => x.DeclaredAccessibility)
                .FirstOrDefault();

            if (injectMethod is null)
            {
                return isOnNewLine;
            }

            if (isOnNewLine)
            {
                context.StringBuilder.AppendLine();
                context.StringBuilder.Append("                ");
            }

            context.StringBuilder.Append($$"""
            .Inject(static (o, c) =>
                            {
                                var to = (
            """);
            context.StringBuilder.Append(context.ClassName);
            context.StringBuilder.AppendLine(")o;");
            context.StringBuilder.Append("                    to.Inject(");
            CreateMethodResolution(injectMethod, "                        ", context.TypeReferences, context.StringBuilder);
            context.StringBuilder.AppendLine(");");
            context.StringBuilder.Append("                })");
            return true;
        }

        private static void AddDefault(GenerationContext generationContext, Accessibility accessibility)
        {
            var accessibilityString = GetAccessibilityString(accessibility);

            generationContext.StringBuilder.Append($$"""
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    {{generationContext.ObsoleteText}}{{accessibilityString}} static Binding<{{generationContext.ClassName}}> Default{{generationContext.TypeParameters}}(this Binding<{{generationContext.ClassName}}> binding)
                    {
                        return DefaultImpl{{(generationContext.ClassSymbol.TypeParameters.Length > 0 ? $"<{generationContext.ClassName}, {string.Join(", ", generationContext.ClassSymbol.TypeParameters.Select(x => x.Name))}>" : "")}}(binding);
                    }
                    
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
            """);
            generationContext.StringBuilder.AppendLine();

            var defaultImplTypeParameters = generationContext.ClassSymbol.TypeParameters.Length > 0
                ? $"<TDefaultImpl, {string.Join(", ", generationContext.ClassSymbol.TypeParameters.Select(x => x.Name))}>"
                : "<TDefaultImpl>";

            generationContext.StringBuilder.Append($$"""
                    {{generationContext.ObsoleteText}}{{accessibilityString}} static Binding<TDefaultImpl> DefaultImpl{{defaultImplTypeParameters}}(Binding<TDefaultImpl> binding) where TDefaultImpl : {{generationContext.ClassName}}
                    {

            """);

            var baseType = generationContext.ClassSymbol.BaseType;
            if (baseType is not null && ShouldGenerateCallToBase(baseType, generationContext))
            {
                var baseClassName = FullyQualifyTypeWithoutNullable(baseType.OriginalDefinition);
                var baseExtensionsClassName = $"ManualDiGenerated{baseClassName.Replace(".", "_").Replace("<", "_").Replace(">", "_")}Extensions";

                var typeArguments = new List<string>();
                typeArguments.Add("TDefaultImpl");
                foreach (var typeArgument in baseType.TypeArguments)
                {
                    typeArguments.Add(FullyQualifyTypeWithoutNullable(typeArgument));
                }

                generationContext.StringBuilder.AppendLine($"            {baseExtensionsClassName}.DefaultImpl<{string.Join(", ", typeArguments)}>(binding);");
            }

            generationContext.StringBuilder.Append("            return binding");

            var isOnNewLine = AddInitialize(generationContext, false);
            isOnNewLine = AddInject(generationContext, isOnNewLine);
            _ = AddSkipDisposable(generationContext, isOnNewLine);

            generationContext.StringBuilder.AppendLine("""
            ;
                    }
            """);
        }

        private static bool AddSkipDisposable(GenerationContext context, bool isOnNewLine)
        {
            if (context.TypeReferences.IsIDisposable(context.ClassSymbol))
            {
                return isOnNewLine;
            }

            if (isOnNewLine)
            {
                context.StringBuilder.AppendLine();
                context.StringBuilder.Append("                ");
            }
            context.StringBuilder.Append(".SkipDisposable()");
            return true;
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

        private static bool ShouldGenerateCallToBase(INamedTypeSymbol typeSymbol, GenerationContext context)
        {
            if (typeSymbol.SpecialType is SpecialType.System_Object)
            {
                return false;
            }

            var accessibility = GetSymbolAccessibility(typeSymbol);
            if (accessibility is not (Accessibility.Public or Accessibility.Internal))
            {
                return false;
            }

            if (typeSymbol.Locations.Any(static x => x.IsInSource))
            {
                return typeSymbol.DeclaringSyntaxReferences
                    .Any(x => IsSyntaxNodeValid(x.GetSyntax(context.CancellationToken), context.CancellationToken));
            }

            //This only happens when the source generator runs on a precompiled DLL
            var fileName = FullyQualifyTypeWithoutNullable(typeSymbol.OriginalDefinition)
                .Replace(".", "_")
                .Replace("<", "_")
                .Replace(">", "_");

            var expectedTypeName = $"ManualDi.Sync.ManualDiGenerated{fileName}Extensions";
            return context.TypeReferences.TypeExists(expectedTypeName);
        }
    }
}

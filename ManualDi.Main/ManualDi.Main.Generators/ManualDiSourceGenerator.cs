using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;


namespace ManualDi.Main.Generators
{
    public record struct GenerationClassContext(StringBuilder StringBuilder, string ClassName, INamedTypeSymbol ClassSymbol, string ObsoleteText, TypeReferences TypeReferences);

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
            
            if (classDeclarationSyntax.TypeParameterList is not null)
            {
                return false;
            }

            if (classDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword))
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
            
            if (classSymbol.IsAbstract)
            {
                return;
            }
            
            var accessibility = GetSymbolAccessibility(classSymbol);
            if (accessibility is not (Accessibility.Public or Accessibility.Internal))
            {
                return;
            }

            var className = FullyQualifyTypeWithoutNullable(classSymbol);
            var obsoleteText = typeReferences.IsSymbolObsolete(classSymbol)
                ? "[System.Obsolete]\r\n" 
                : "";
            
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($$"""
            #nullable enable
            using System.Runtime.CompilerServices;

            namespace ManualDi.Main
            {
                public static class ManualDiGenerated{{className.Replace(".","")}}Extensions
                {
            """);

            var generationContext = new GenerationClassContext(stringBuilder, className, classSymbol, obsoleteText, typeReferences);

            if (!typeReferences.IsUnityEngineObject(classSymbol))
            {
                AddFromConstructor(generationContext);
            }
            
            AddDefault(generationContext, accessibility);

            stringBuilder.AppendLine("""
                }
            }
            """);
            
            context.AddSource($"ManualDiGeneratedExtensions.{className}.g.cs", SourceText.From(stringBuilder.ToString(), Encoding.UTF8));
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
            if (attributeData.ConstructorArguments.Length != 1)
            {
                return null;
            }

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

        private static void AddFromConstructor(GenerationClassContext context)
        {
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
                    {{context.ObsoleteText}}{{accessibilityString}} static TypeBinding<T, {{context.ClassName}}> FromConstructor<T>(this TypeBinding<T, {{context.ClassName}}> typeBinding)
                    {
                        return typeBinding.FromMethod(static c => new {{context.ClassName}}(
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
                
                var attribute = typeReferences.GetInjectAttribute(parameter);
                var id = attribute is null ? null : GetInjectId(attribute);
                stringBuilder.Append(tabs);
                CreteTypeResolution(parameter.Type, id, typeReferences, stringBuilder);
            }
        }

        private static void CreateIdResolution(string? id, StringBuilder stringBuilder)
        {
            if (id is not null)
            {
                stringBuilder.Append("x => x.Id(");
                stringBuilder.Append(id);
                stringBuilder.Append(")");
            }
        }
        
        private static void CreteTypeResolution(ITypeSymbol typeSymbol, string? id, TypeReferences typeReferences,StringBuilder stringBuilder)
        {
            var lazyGenericType = typeReferences.TryGenericLazyType(typeSymbol);
            if (lazyGenericType is not null)
            {
                if (IsNullableTypeSymbol(typeSymbol))
                {
                    stringBuilder.Append("c.WouldResolve<");
                    stringBuilder.Append(FullyQualifyTypeWithoutNullable(lazyGenericType));
                    stringBuilder.Append(">(");
                    CreateIdResolution(id, stringBuilder);
                    stringBuilder.Append(") ? new System.Lazy<");
                    stringBuilder.Append(FullyQualifyTypeWithNullable(lazyGenericType));
                    stringBuilder.Append(">(() => ");
                    CreteTypeResolution(lazyGenericType, id, typeReferences, stringBuilder);
                    stringBuilder.Append(") : null");
                    return;
                }

                stringBuilder.Append("new System.Lazy<");
                stringBuilder.Append(FullyQualifyTypeWithNullable(lazyGenericType));
                stringBuilder.Append(">(() => ");
                CreteTypeResolution(lazyGenericType, id, typeReferences, stringBuilder);
                stringBuilder.Append(")");
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

                if (isListNullable)
                {
                    if (IsNullableTypeSymbol(listGenericType))
                    {
                        stringBuilder.Append(".ConvertAll<");
                        stringBuilder.Append(FullyQualifyTypeWithNullable(listGenericType));
                        stringBuilder.Append(">(x => x)");
                    }
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

        private static bool AddInitialize(GenerationClassContext context, bool isOnNewLine)
        {
            var initializeMethod = context.ClassSymbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .FirstOrDefault(x => x is
                {
                    Name: "Initialize", 
                    DeclaredAccessibility: Accessibility.Public or Accessibility.Internal,
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
            
            context.StringBuilder.Append(".Initialize(static (o, c) => o.Initialize())");
            return true;
        }

        private static bool AddInject(GenerationClassContext context, bool isOnNewLine)
        {
            var injectMethods = context.ClassSymbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .Where(x => x is { Name: "Inject", DeclaredAccessibility: Accessibility.Public or Accessibility.Internal, IsStatic: false })
                .OrderByDescending(x => x.DeclaredAccessibility)
                .ToArray();
            
            var injectProperties = context.ClassSymbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Select(x => (propertySymbol: x, attribute: context.TypeReferences.GetInjectAttribute(x)))
                .Where(x => x.attribute is not null)
                .ToArray();

            if (injectMethods.Length == 0 && injectProperties.Length == 0)
            {
                return isOnNewLine;
            }

            var injectMethod = injectMethods.FirstOrDefault();

            if (isOnNewLine)
            {
                context.StringBuilder.AppendLine();
                context.StringBuilder.Append("                ");
            }
            
            context.StringBuilder.AppendLine($$"""
            .Inject(static (o, c) => 
                            {
            """);

            foreach (var (injectProperty, attribute) in injectProperties)
            {
                var id = attribute is null ? null : GetInjectId(attribute);
                context.StringBuilder.Append($"                    o.{injectProperty.Name} = ");
                CreteTypeResolution(injectProperty.Type, id, context.TypeReferences, context.StringBuilder);
                context.StringBuilder.AppendLine(";");
            }
                        
            if (injectMethod is not null)
            {
                context.StringBuilder.Append("                    o.Inject(");
                CreateMethodResolution(injectMethod, "                        ", context.TypeReferences, context.StringBuilder);
                context.StringBuilder.AppendLine(");");
            }

            context.StringBuilder.Append("                })");
            return true;
        }

        private static void AddDefault(GenerationClassContext generationClassContext, Accessibility typeAccessibility)
        {
            var accessibility = typeAccessibility;
            var accessibilityString = GetAccessibilityString(accessibility);
            
            generationClassContext.StringBuilder.Append($$"""
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    {{generationClassContext.ObsoleteText}}{{accessibilityString}} static TypeBinding<T, {{generationClassContext.ClassName}}> Default<T>(this TypeBinding<T, {{generationClassContext.ClassName}}> typeBinding)
                    {
                        return typeBinding
            """);

            var isOnNewLine = AddInitialize(generationClassContext, false);
            isOnNewLine = AddInject(generationClassContext, isOnNewLine);
            _ = AddDontDispose(generationClassContext, isOnNewLine);

            generationClassContext.StringBuilder.AppendLine("""
            ;
                    }
            """);
        }

        private static bool AddDontDispose(GenerationClassContext context, bool isOnNewLine)
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
            context.StringBuilder.Append(".DontDispose()");
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
            if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated || typeSymbol.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
            {
                return true;
            }
            
            return false;
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
    }
}

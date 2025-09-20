using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;


namespace ManualDi.Async.Generators
{
    public record struct GenerationClassContext(
        StringBuilder StringBuilder,
        string ClassName,
        INamedTypeSymbol ClassSymbol,
        string ObsoleteText,
        TypeReferences TypeReferences);

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

            namespace ManualDi.Async
            {
                public static class ManualDiGenerated{{className.Replace(".","_")}}Extensions
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
                    {{context.ObsoleteText}}{{accessibilityString}} static Binding<T, {{context.ClassName}}> FromConstructor<T>(this Binding<T, {{context.ClassName}}> binding)
                    {
                        return binding
                            .FromMethod(static c => new {{context.ClassName}}(
            """);
            
            CreateMethodResolution(constructor, "                    ", context.TypeReferences, context.StringBuilder);

            context.StringBuilder.Append("))");
            
            CreateMethodDependencies(constructor, "                ", context.TypeReferences, context.StringBuilder);
                
            context.StringBuilder.Append("""
            ;
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
                
                var attribute = typeReferences.GetIdAttribute(parameter);
                var id = attribute is null ? null : GetInjectId(attribute);
                var isOutParam = parameter.RefKind == RefKind.Out;
                stringBuilder.Append(tabs);
                CreteTypeResolution(parameter.Type, id, isOutParam, typeReferences, stringBuilder);
            }
        }
        
        private static void CreateMethodDependencies(IMethodSymbol methodSymbol, string tabs, TypeReferences typeReferences, StringBuilder stringBuilder)
        {
            if (methodSymbol.Parameters.Length == 0)
            {
                return;
            }

            stringBuilder.AppendLine();
            stringBuilder.Append(tabs);
            stringBuilder.AppendLine(".DependsOn(static d => {");
            foreach (var parameter in methodSymbol.Parameters)
            {
                var isOutParam = parameter.RefKind == RefKind.Out;
                if (isOutParam)
                {
                    continue;
                }
                stringBuilder.Append(tabs);
                var attribute = typeReferences.GetIdAttribute(parameter);
                var id = attribute is null ? null : GetInjectId(attribute);
                CreteTypeDependency(true, parameter.Type, id, typeReferences, stringBuilder);
            }
            stringBuilder.Append(tabs);
            stringBuilder.Append("})");
        }
        
        private static void CreateMethodPropertyDependencies(IMethodSymbol? injectMethodSymbol, string tabs, TypeReferences typeReferences, StringBuilder stringBuilder)
        {
            if (injectMethodSymbol?.Parameters.Length == 0)
            {
                return;
            }

            stringBuilder.AppendLine();
            stringBuilder.Append(tabs);
            stringBuilder.AppendLine(".DependsOn(static d => {");
            
            if (injectMethodSymbol is not null)
            {
                foreach (var parameter in injectMethodSymbol.Parameters)
                {
                    var isOutParam = parameter.RefKind == RefKind.Out;
                    if (isOutParam)
                    {
                        continue;
                    }
                    stringBuilder.Append(tabs);
                    var attribute = typeReferences.GetIdAttribute(parameter);
                    var id = attribute is null ? null : GetInjectId(attribute);
                    var isConstructor = !typeReferences.HasCyclicDependencyAttribute(parameter);
                    CreteTypeDependency(isConstructor, parameter.Type, id, typeReferences, stringBuilder);
                }
            }
            
            stringBuilder.Append(tabs);
            stringBuilder.Append("})");
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
        
        private static void CreteTypeResolution(ITypeSymbol typeSymbol, string? id, bool isOutParam, TypeReferences typeReferences,StringBuilder stringBuilder)
        {
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
        
        private static void CreteTypeDependency(bool isConstructor, ITypeSymbol typeSymbol, string? id, TypeReferences typeReferences, StringBuilder stringBuilder)
        {
            if (typeReferences.IsSymbolDiContainer(typeSymbol))
            {
                stringBuilder.AppendLine("    // Injected DiContainer");
                return;
            }

            if (typeReferences.IsCancellationToken(typeSymbol))
            {
                stringBuilder.AppendLine("    // Injected CancellationToken");
                return;
            }

            // Updated code below
            var listGenericType = typeReferences.TryGetEnumerableType(typeSymbol);
            if (listGenericType is not null)
            {
                if (isConstructor)
                {
                    stringBuilder.Append("    d.NullableConstructorDependency<");
                }
                else
                {
                    stringBuilder.Append("    d.NullableInjectionDependency<");
                }
                stringBuilder.Append(FullyQualifyTypeWithoutNullable(listGenericType));
                stringBuilder.Append(">(");
                CreateIdResolution(id, stringBuilder);
                stringBuilder.AppendLine(");");
                return;
            }

            if (IsNullableTypeSymbol(typeSymbol))
            {
                if (isConstructor)
                {
                    stringBuilder.Append("    d.NullableConstructorDependency<");
                }
                else
                {
                    stringBuilder.Append("    d.NullableInjectionDependency<");
                }
                stringBuilder.Append(FullyQualifyTypeWithoutNullable(typeSymbol));
                stringBuilder.Append(">(");
                CreateIdResolution(id, stringBuilder);
                stringBuilder.AppendLine(");");
                return;
            }
            
            if (isConstructor)
            {
                stringBuilder.Append("    d.ConstructorDependency<");
            }
            else
            {
                stringBuilder.Append("    d.InjectionDependency<");
            }
            stringBuilder.Append(FullyQualifyTypeWithoutNullable(typeSymbol));
            stringBuilder.Append(">(");
            CreateIdResolution(id, stringBuilder);
            stringBuilder.AppendLine(");");
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

            context.StringBuilder.Append(".Initialize(static o => ((");
            context.StringBuilder.Append(context.ClassName);
            context.StringBuilder.Append(")o).Initialize())");
            return true;
        }
        
        private static bool AddInitializeAsync(GenerationClassContext context, bool isOnNewLine)
        {
            var initializeMethod = context.ClassSymbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .FirstOrDefault(x => x is
                {
                    Name: "InitializeAsync", 
                    DeclaredAccessibility: Accessibility.Public or Accessibility.Internal,
                    IsStatic: false,
                    Parameters.Length: 1
                } && context.TypeReferences.IsCancellationToken(x.Parameters[0].Type) && context.TypeReferences.IsTask(x.ReturnType));
                
            if (initializeMethod is null)
            {
                return isOnNewLine;
            }
            
            if (isOnNewLine)
            {
                context.StringBuilder.AppendLine();
                context.StringBuilder.Append("                    ");
            }

            context.StringBuilder.Append(".InitializeAsync(static (o, ct) => ((");
            context.StringBuilder.Append(context.ClassName);
            context.StringBuilder.Append(")o).InitializeAsync(ct))");
            return true;
        }

        private static bool AddInject(GenerationClassContext context, bool isOnNewLine)
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
            
            if (injectMethod is not null)
            {
                context.StringBuilder.Append("                    to.Inject(");
                CreateMethodResolution(injectMethod, "                        ", context.TypeReferences, context.StringBuilder);
                context.StringBuilder.AppendLine(");");
            }

            context.StringBuilder.Append("                })");
            
            CreateMethodPropertyDependencies(injectMethod, "                ", context.TypeReferences, context.StringBuilder);
            return true;
        }

        private static void AddDefault(GenerationClassContext generationClassContext, Accessibility typeAccessibility)
        {
            var accessibility = typeAccessibility;
            var accessibilityString = GetAccessibilityString(accessibility);
            
            generationClassContext.StringBuilder.Append($$"""
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    {{generationClassContext.ObsoleteText}}{{accessibilityString}} static Binding<T, {{generationClassContext.ClassName}}> Default<T>(this Binding<T, {{generationClassContext.ClassName}}> binding)
                    {
                        return binding
            """);

            var isOnNewLine = AddInitialize(generationClassContext, false);
            isOnNewLine = AddInitializeAsync(generationClassContext, isOnNewLine);
            isOnNewLine = AddInject(generationClassContext, isOnNewLine);
            _ = AddSkipDisposable(generationClassContext, isOnNewLine);

            generationClassContext.StringBuilder.AppendLine("""
            ;
                    }
            """);
        }

        private static bool AddSkipDisposable(GenerationClassContext context, bool isOnNewLine)
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

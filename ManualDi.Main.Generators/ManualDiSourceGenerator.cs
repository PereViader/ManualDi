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
    [Generator]
    public class ManualDiSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            //https://www.thinktecture.com/en/net/roslyn-source-generators-code-according-to-dependencies/
            // Create a provider for metadata references
            var manualDiMainReferenceModule = context.GetMetadataReferencesProvider()
                .SelectMany((references, _) => references.GetModules())
                .Where(x => x.Name is "ManualDi.Main.dll")
                .Collect();
            
            var classDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(IsSyntaxNodeValid, GetClassDeclaration)
                .Collect();

            var providers = context.CompilationProvider
                .Combine(manualDiMainReferenceModule)
                .Combine(classDeclarations);

            context.RegisterSourceOutput(providers, Generate);
        }

        public readonly struct GenerationClassContext
        {
            public StringBuilder StringBuilder { get; }
            public string ClassName { get; }
            public INamedTypeSymbol ClassSymbol { get; }
            public string AccessibilityString { get; }
            public string ObsoleteText { get; }

            public GenerationClassContext(StringBuilder stringBuilder, string className, INamedTypeSymbol classSymbol, string accessibilityString, string obsoleteText)
            {
                StringBuilder = stringBuilder;
                ClassName = className;
                ClassSymbol = classSymbol;
                AccessibilityString = accessibilityString;
                ObsoleteText = obsoleteText;
            }
        }
        
        private void Generate(SourceProductionContext context, ((Compilation, ImmutableArray<ModuleInfo>), ImmutableArray<ClassDeclarationSyntax>) arg)
        {
            var ((compilation, moduleInfos), classDeclarations) = arg;

            //We don't generate if the ManualDi reference is not there
            if (moduleInfos.Length == 0)
            {
                return;
            }
            
            var unityEngineObjectSymbol = compilation.GetTypeByMetadataName("UnityEngine.Object");

            foreach (var classDeclaration in classDeclarations)
            {
                var stringBuilder = new StringBuilder();

                stringBuilder.Append(@"
using ManualDi.Main;

namespace ManualDi.Main
{
    public static partial class ManualDiGeneratedExtensions
    {
");
                
                var model = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                var classSymbol = ModelExtensions.GetDeclaredSymbol(model, classDeclaration) as INamedTypeSymbol;
                
                if (classSymbol is null)
                {
                    continue;
                }
                
                var accessibility = GetTypeVisibility(classSymbol);
                if (accessibility is not (Accessibility.Public or Accessibility.Internal))
                {
                    continue;
                }

                var accessibilityString = GetAccessibilityString(accessibility);
                bool inheritsUnityObject = InheritsFromSymbol(classSymbol, unityEngineObjectSymbol);
                var className = FullyQualifyType(classSymbol);
                var obsoleteText = IsSymbolObsolete(classSymbol)
                    ? "[System.Obsolete]\r\n        " 
                    : "";

                var generationContext = new GenerationClassContext(stringBuilder, className, classSymbol, accessibilityString,
                    obsoleteText);
                
                bool hasConstructor = !inheritsUnityObject && AddFromConstructor(generationContext);
                bool hasInitialize = AddInitialize(generationContext);
                bool hasInject = AddInject(generationContext);
                if (hasConstructor)
                {
                    AddDefaultConstructor(generationContext, hasConstructor, hasInitialize, hasInject);
                }
                AddDefaultInstance(generationContext, hasConstructor, hasInitialize, hasInject);
                AddDefaultMethod(generationContext, hasConstructor, hasInitialize, hasInject);

                stringBuilder.Append(@"
    }
}
");
                context.AddSource($"ManualDiGeneratedExtensions.{className}.cs", SourceText.From(stringBuilder.ToString(), Encoding.UTF8));
            }
        }
        
        private static bool InheritsFromSymbol(INamedTypeSymbol namedTypeSymbol, INamedTypeSymbol? inheritedNameTypeSymbol)
        {
            if (inheritedNameTypeSymbol == null)
            {
                return false;
            }

            var baseType = namedTypeSymbol;
            while (baseType != null)
            {
                if (SymbolEqualityComparer.Default.Equals(baseType, inheritedNameTypeSymbol))
                {
                    return true;
                }
                baseType = baseType.BaseType;
            }
            return false;
        }
        
        private static bool IsSymbolObsolete(ISymbol typeSymbol)
        {
            return typeSymbol.GetAttributes()
                .Any(x => x.AttributeClass?.ToDisplayString() == "System.ObsoleteAttribute");
        }
        
        private static bool IsSymbolInjectValid(IPropertySymbol propertySymbol)
        {
            if (propertySymbol.IsStatic)
            {
                return false;
            }
            
            bool isPropertyAccessible = propertySymbol.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal;
            if (!isPropertyAccessible)
            {
                return false;
            }

            bool isSetterAccessible = propertySymbol.SetMethod?.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal;
            if (!isSetterAccessible)
            {
                return false;
            }
            
            bool hasInjectAttribute = propertySymbol
                .GetAttributes()
                .Any(x => x.AttributeClass?.ToDisplayString() == "ManualDi.Main.InjectAttribute");
            if (!hasInjectAttribute)
            {
                return false;
            }
            
            return true;
        }

        private string GetAccessibilityString(Accessibility accessibility)
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

            if (classDeclarationSyntax.TypeParameterList?.Parameters.Count > 0)
            {
                return false;
            }

            return true;
        }
        
        private Accessibility GetTypeVisibility(INamedTypeSymbol typeSymbol)
        {
            // Recursively determine the effective accessibility of the type
            var visibility = typeSymbol.DeclaredAccessibility;
            var containingType = typeSymbol.ContainingType;

            while (containingType != null)
            {
                var parentVisibility = containingType.DeclaredAccessibility;
                if (parentVisibility < typeSymbol.DeclaredAccessibility)
                {
                    visibility = parentVisibility;
                }
                containingType = containingType.ContainingType;
            }

            return visibility;
        }

        private static ClassDeclarationSyntax GetClassDeclaration(GeneratorSyntaxContext context, CancellationToken ct)
        {
            return (ClassDeclarationSyntax)context.Node;
        }

        private static bool AddFromConstructor(GenerationClassContext context)
        {
            var constructor = context.ClassSymbol
                .Constructors
                .SingleOrDefault(c => c.DeclaredAccessibility == Accessibility.Public);

            if (constructor is null)
            {
                return false;
            }

            var arguments = string.Join(",\r\n                ", constructor.Parameters.Select(p => $"c.Resolve<{FullyQualifyType(p.Type)}>()"));

            context.StringBuilder.Append($@"
        {context.ObsoleteText}{context.AccessibilityString} static TypeBinding<T, {context.ClassName}> FromConstructor<T>(this TypeBinding<T, {context.ClassName}> typeBinding)
        {{
            typeBinding.FromMethod(static c => new {context.ClassName}({arguments}));
            return typeBinding;
        }}
");
            return true;
        }

        private static bool AddInitialize(GenerationClassContext context)
        {
            var initializeMethod = context.ClassSymbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .SingleOrDefault(m => m is { Name: "Initialize", DeclaredAccessibility: Accessibility.Public, IsStatic: false });

            if (initializeMethod is null)
            {
                return false;
            }

            var arguments = string.Join(",\r\n                ", initializeMethod.Parameters.Select(p => $"c.Resolve<{FullyQualifyType(p.Type)}>()"));

            context.StringBuilder.Append($@"
        {context.ObsoleteText}{context.AccessibilityString} static TypeBinding<T, {context.ClassName}> Initialize<T>(this TypeBinding<T, {context.ClassName}> typeBinding)
        {{
            typeBinding.Initialize(static (o, c) => o.Initialize({arguments}));
            return typeBinding;
        }}
");
            return true;
        }

        private static bool AddInject(GenerationClassContext generationClassContext)
        {
            var injectMethod = generationClassContext.ClassSymbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .SingleOrDefault(m => m is { Name: "Inject", DeclaredAccessibility: Accessibility.Public, IsStatic: false });

            var injectProperties = generationClassContext.ClassSymbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(IsSymbolInjectValid)
                .ToArray();

            if (injectMethod is null && injectProperties.Length == 0)
            {
                return false;
            }
            
            generationClassContext.StringBuilder.AppendLine($@"
        {generationClassContext.ObsoleteText}{generationClassContext.AccessibilityString} static TypeBinding<T, {generationClassContext.ClassName}> Inject<T>(this TypeBinding<T, {generationClassContext.ClassName}> typeBinding)
        {{
            typeBinding.Inject(static (o, c) => 
            {{");

            foreach (var injectProperty in injectProperties)
            {
                generationClassContext.StringBuilder.AppendLine($"                o.{injectProperty.Name} = c.Resolve<{FullyQualifyType(injectProperty.Type)}>();");
            }
            
            if (injectMethod is not null)
            {
                var arguments = string.Join(",\r\n                ", injectMethod.Parameters.Select(p => $"c.Resolve<{FullyQualifyType(p.Type)}>()"));
                generationClassContext.StringBuilder.AppendLine($"                o.Inject({arguments});");
            }
            
            generationClassContext.StringBuilder.Append(@"            });
            return typeBinding;
        }
");
            return true;
        }

        private static void AddDefaultConstructor(GenerationClassContext generationClassContext, bool hasConstructor, bool hasInitialize,
            bool hasInject)
        {
            generationClassContext.StringBuilder.AppendLine($@"
        {generationClassContext.ObsoleteText}{generationClassContext.AccessibilityString} static TypeBinding<T, {generationClassContext.ClassName}> Default<T>(this TypeBinding<T, {generationClassContext.ClassName}> typeBinding)
        {{");

            if (hasConstructor)
            {
                generationClassContext.StringBuilder.AppendLine("            typeBinding.FromConstructor();");
            }

            if (hasInitialize)
            {
                generationClassContext.StringBuilder.AppendLine("            typeBinding.Initialize();");
            }

            if (hasInject)
            {
                generationClassContext.StringBuilder.AppendLine("            typeBinding.Inject();");
            }

            generationClassContext.StringBuilder.AppendLine("            return typeBinding;\r\n        }");
        }
        
        private static void AddDefaultInstance(GenerationClassContext generationClassContext, bool hasConstructor, bool hasInitialize,
            bool hasInject)
        {
            generationClassContext.StringBuilder.AppendLine($@"
        {generationClassContext.ObsoleteText}{generationClassContext.AccessibilityString} static TypeBinding<T, {generationClassContext.ClassName}> Default<T>(this TypeBinding<T, {generationClassContext.ClassName}> typeBinding, {generationClassContext.ClassName} instance)
        {{");

            if (hasConstructor)
            {
                generationClassContext.StringBuilder.AppendLine("            typeBinding.FromInstance(instance);");
            }

            if (hasInitialize)
            {
                generationClassContext.StringBuilder.AppendLine("            typeBinding.Initialize();");
            }

            if (hasInject)
            {
                generationClassContext.StringBuilder.AppendLine("            typeBinding.Inject();");
            }

            generationClassContext.StringBuilder.AppendLine("            return typeBinding;\r\n        }");
        }
        
        private static void AddDefaultMethod(GenerationClassContext generationClassContext, bool hasConstructor, bool hasInitialize,
            bool hasInject)
        {
            generationClassContext.StringBuilder.AppendLine($@"
        {generationClassContext.ObsoleteText}{generationClassContext.AccessibilityString} static TypeBinding<T, {generationClassContext.ClassName}> Default<T>(this TypeBinding<T, {generationClassContext.ClassName}> typeBinding, CreateDelegate<{generationClassContext.ClassName}> func)
        {{");

            if (hasConstructor)
            {
                generationClassContext.StringBuilder.AppendLine("            typeBinding.FromMethod(func);");
            }

            if (hasInitialize)
            {
                generationClassContext.StringBuilder.AppendLine("            typeBinding.Initialize();");
            }

            if (hasInject)
            {
                generationClassContext.StringBuilder.AppendLine("            typeBinding.Inject();");
            }

            generationClassContext.StringBuilder.AppendLine("            return typeBinding;\r\n        }");
        }

        private static readonly SymbolDisplayFormat FullyQualifyTypeSymbolDisplayFormat = new(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
        );
        
        private static string FullyQualifyType(ITypeSymbol typeSymbol)
        {
            return typeSymbol.ToDisplayString(FullyQualifyTypeSymbolDisplayFormat);
        }
    }
}

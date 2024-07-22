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

        private void Generate(SourceProductionContext context, ((Compilation, ImmutableArray<ModuleInfo>), ImmutableArray<ClassDeclarationSyntax>) arg)
        {
            var ((compilation, moduleInfos), classDeclarations) = arg;

            //We don't generate if the ManualDi reference is not there
            if (moduleInfos.Length == 0)
            {
                return;
            }

            var stringBuilder = new StringBuilder();

            stringBuilder.Append(@"
using ManualDi.Main;

namespace ManualDi.Main
{
    public static partial class ManualDiGeneratedExtensions
    {
");

            foreach (var classDeclaration in classDeclarations)
            {
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
                var className = FullyQualifyType(classSymbol);

                bool hasConstructor = AddFromConstructor(stringBuilder, className, classSymbol, accessibilityString);
                bool hasInitialize = AddInitialize(stringBuilder, className, classSymbol, accessibilityString);
                bool hasInject = AddInject(stringBuilder, className, classSymbol, accessibilityString);
                AddDefault(stringBuilder, hasConstructor, hasInitialize, hasInject, className, accessibilityString);
            }

            stringBuilder.Append(@"
    }
}
");

            context.AddSource($"ManualDiGeneratedExtensions.g.cs", SourceText.From(stringBuilder.ToString(), Encoding.UTF8));
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

        private static bool AddFromConstructor(StringBuilder stringBuilder, string className,
            INamedTypeSymbol classSymbol, string accessibilityString)
        {
            var constructor = classSymbol
                .Constructors
                .SingleOrDefault(c => c.DeclaredAccessibility == Accessibility.Public);

            if (constructor is null)
            {
                return false;
            }

            var arguments = string.Join(",\r\n                ", constructor.Parameters.Select(p => $"c.Resolve<{FullyQualifyType(p.Type)}>()"));

            stringBuilder.Append($@"
        {accessibilityString} static TypeBinding<T, {className}> FromConstructor<T>(this TypeBinding<T, {className}> typeBinding)
        {{
            typeBinding.FromMethod(static c => new {className}({arguments}));
            return typeBinding;
        }}
");
            return true;
        }

        private static bool AddInitialize(StringBuilder stringBuilder, string className, INamedTypeSymbol classSymbol,
            string accessibilityString)
        {
            var initializeMethod = classSymbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .SingleOrDefault(m => m is { Name: "Initialize", DeclaredAccessibility: Accessibility.Public, IsStatic: false });

            if (initializeMethod is null)
            {
                return false;
            }

            var arguments = string.Join(",\r\n                ", initializeMethod.Parameters.Select(p => $"c.Resolve<{FullyQualifyType(p.Type)}>()"));

            stringBuilder.Append($@"
        {accessibilityString} static TypeBinding<T, {className}> Initialize<T>(this TypeBinding<T, {className}> typeBinding)
        {{
            typeBinding.Initialize(static (o, c) => o.Initialize({arguments}));
            return typeBinding;
        }}
");
            return true;
        }

        private static bool AddInject(StringBuilder stringBuilder, string className, INamedTypeSymbol classSymbol,
            string accessibilityString)
        {
            var injectMethod = classSymbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .SingleOrDefault(m => m is { Name: "Inject", DeclaredAccessibility: Accessibility.Public, IsStatic: false });

            if (injectMethod is null)
            {
                return false;
            }

            var arguments = string.Join(",\r\n                ", injectMethod.Parameters.Select(p => $"c.Resolve<{FullyQualifyType(p.Type)}>()"));

            stringBuilder.Append($@"
        {accessibilityString} static TypeBinding<T, {className}> Inject<T>(this TypeBinding<T, {className}> typeBinding)
        {{
            typeBinding.Inject(static (o, c) => o.Inject({arguments}));
            return typeBinding;
        }}
");
            return true;
        }

        private static void AddDefault(StringBuilder stringBuilder, bool hasConstructor, bool hasInitialize,
            bool hasInject, string className, string accessibilityString)
        {
            stringBuilder.AppendLine($@"
        {accessibilityString} static TypeBinding<T, {className}> Default<T>(this TypeBinding<T, {className}> typeBinding)
        {{");

            if (hasConstructor)
            {
                stringBuilder.AppendLine("            typeBinding.FromConstructor();");
            }

            if (hasInitialize)
            {
                stringBuilder.AppendLine("            typeBinding.Initialize();");
            }

            if (hasInject)
            {
                stringBuilder.AppendLine("            typeBinding.Inject();");
            }

            stringBuilder.AppendLine("            return typeBinding;\r\n        }");
        }

        private static string FullyQualifyType(ITypeSymbol typeSymbol)
        {
            return typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }
    }
}

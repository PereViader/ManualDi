using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManualDi.Main.Generators
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not SyntaxReceiver receiver)
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
            
            foreach (var classDeclaration in receiver.CandidateClasses)
            {
                var model = context.Compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                var classSymbol = model.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;

                if (classSymbol is null || classSymbol.DeclaredAccessibility != Accessibility.Public)
                    continue;
                
                var className = FullyQualifyType(classSymbol);
                
                bool hasConstructor = AddFromConstructor(stringBuilder, className, classSymbol);
                bool hasInitialize = AddInitialize(stringBuilder, className, classSymbol);
                bool hasInject = AddInject(stringBuilder, className, classSymbol);
                AddDefault(stringBuilder, hasConstructor, hasInitialize, hasInject, className);
            }
            
            stringBuilder.Append(@"
    }
}
");
            
            context.AddSource($"ManualDiGeneratedExtensions.g.cs", SourceText.From(stringBuilder.ToString(), Encoding.UTF8));
        }

        private static bool AddFromConstructor(StringBuilder stringBuilder, string className, INamedTypeSymbol classSymbol)
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
        public static TypeBinding<T, {className}> FromConstructor<T>(this TypeBinding<T, {className}> typeBinding)
        {{
            typeBinding.FromMethod(static c => new {className}({arguments}));
            return typeBinding;
        }}
");
            return true;
        }
        
        private bool AddInitialize(StringBuilder stringBuilder, string className, INamedTypeSymbol classSymbol)
        {
            // Check if the class contains a public 'Initialize' method
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
        public static TypeBinding<T, {className}> Initialize<T>(this TypeBinding<T, {className}> typeBinding)
        {{
            typeBinding.Initialize(static (o, c) => o.Initialize({arguments}));
            return typeBinding;
        }}
");
            return true;
        }
        
        private bool AddInject(StringBuilder stringBuilder, string className, INamedTypeSymbol classSymbol)
        {
            // Check if the class contains a public 'Initialize' method
            var initializeMethod = classSymbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .SingleOrDefault(m => m is { Name: "Inject", DeclaredAccessibility: Accessibility.Public, IsStatic: false });

            if (initializeMethod is null)
            {
                return false;
            }

            var arguments = string.Join(",\r\n                ", initializeMethod.Parameters.Select(p => $"c.Resolve<{FullyQualifyType(p.Type)}>()"));

            stringBuilder.Append($@"
        public static TypeBinding<T, {className}> Inject<T>(this TypeBinding<T, {className}> typeBinding)
        {{
            typeBinding.Inject(static (o, c) => o.Inject({arguments}));
            return typeBinding;
        }}
");
            return true;
        }
        
        private void AddDefault(StringBuilder stringBuilder, bool hasConstructor, bool hasInitialize, bool hasInject, string className)
        {
            stringBuilder.AppendLine($@"
        public static TypeBinding<T, {className}> Default<T>(this TypeBinding<T, {className}> typeBinding)
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
            
            stringBuilder.AppendLine($@"
            return typeBinding;
        }}");
        }

        private static string FullyQualifyType(ITypeSymbol typeSymbol)
        {
            return typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        private class SyntaxReceiver : ISyntaxReceiver
        {
            public List<ClassDeclarationSyntax> CandidateClasses { get; } = new();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is ClassDeclarationSyntax classDeclaration)
                {
                    CandidateClasses.Add(classDeclaration);
                }
            }
        }
    }
}

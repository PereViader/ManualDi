using Microsoft.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace ManualDi.Async.Generators;

public record TypeReferences
{
    private readonly INamedTypeSymbol? UnityEngineObjectTypeSymbol;
    private readonly INamedTypeSymbol LazyTypeSymbol;
    private readonly INamedTypeSymbol ListTypeSymbol;
    private readonly INamedTypeSymbol IListTypeSymbol;
    private readonly INamedTypeSymbol IReadOnlyListTypeSymbol;
    private readonly INamedTypeSymbol IEnumerableTypeSymbol;
    private readonly INamedTypeSymbol IReadOnlyCollectionTypeSymbol;
    private readonly INamedTypeSymbol ICollectionTypeSymbol;
    private readonly INamedTypeSymbol InjectAttributeTypeSymbol;
    private readonly INamedTypeSymbol ObsoleteAttributeTypeSymbol;
    private readonly INamedTypeSymbol IDisposableTypeSymbol;
    private readonly INamedTypeSymbol IDiContainerTypeSymbol;
    private readonly INamedTypeSymbol CancellationTokenTypeSymbol;
    private readonly INamedTypeSymbol TaskTypeSymbol;

    public TypeReferences(INamedTypeSymbol? unityEngineObjectTypeSymbol,
        INamedTypeSymbol lazyTypeSymbol, INamedTypeSymbol listTypeSymbol, INamedTypeSymbol iListTypeSymbol,
        INamedTypeSymbol iReadOnlyListTypeSymbol, INamedTypeSymbol iEnumerableTypeSymbol,
        INamedTypeSymbol iReadOnlyCollectionTypeSymbol, INamedTypeSymbol iCollectionTypeSymbol,
        INamedTypeSymbol injectAttributeTypeSymbol, INamedTypeSymbol obsoleteAttributeTypeSymbol,
        INamedTypeSymbol iDisposableTypeSymbol, INamedTypeSymbol iDiContainerTypeSymbol, 
        INamedTypeSymbol cancellationTokenTypeSymbol, INamedTypeSymbol taskTypeSymbol)
    {
        UnityEngineObjectTypeSymbol = unityEngineObjectTypeSymbol;
        LazyTypeSymbol = lazyTypeSymbol;
        ListTypeSymbol = listTypeSymbol;
        IListTypeSymbol = iListTypeSymbol;
        IReadOnlyListTypeSymbol = iReadOnlyListTypeSymbol;
        IEnumerableTypeSymbol = iEnumerableTypeSymbol;
        IReadOnlyCollectionTypeSymbol = iReadOnlyCollectionTypeSymbol;
        ICollectionTypeSymbol = iCollectionTypeSymbol;
        InjectAttributeTypeSymbol = injectAttributeTypeSymbol;
        ObsoleteAttributeTypeSymbol = obsoleteAttributeTypeSymbol;
        IDisposableTypeSymbol = iDisposableTypeSymbol;
        IDiContainerTypeSymbol = iDiContainerTypeSymbol;
        CancellationTokenTypeSymbol = cancellationTokenTypeSymbol;
        TaskTypeSymbol = taskTypeSymbol;
    }
    
    public static TypeReferences? Create(Compilation compilation, CancellationToken ct)
    {
        var diContainerTypeSymbol = compilation.GetTypeByMetadataName("ManualDi.Async.IDiContainer");
        if (diContainerTypeSymbol is null)
        {
            return null;
        }
        
        var unityEngineObjectTypeSymbol = compilation.GetTypeByMetadataName("UnityEngine.Object");
        
        var lazyTypeSymbol = compilation.GetTypeByMetadataName("System.Lazy`1");
        if (lazyTypeSymbol is null)
        {
            return null;
        }

        var listTypeSymbol = compilation.GetTypeByMetadataName("System.Collections.Generic.List`1");
        if (listTypeSymbol is null)
        {
            return null;
        }
        var iListTypeSymbol = compilation.GetTypeByMetadataName("System.Collections.Generic.IList`1");
        if (iListTypeSymbol is null)
        {
            return null;
        }

        var iReadOnlyListTypeSymbol = compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlyList`1");
        if (iReadOnlyListTypeSymbol is null)
        {
            return null;
        }

        var iReadOnlyCollectionTypeSymbol = compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlyCollection`1");
        if (iReadOnlyCollectionTypeSymbol is null)
        {
            return null;
        }

        var iCollectionTypeSymbol = compilation.GetTypeByMetadataName("System.Collections.Generic.ICollection`1");
        if (iCollectionTypeSymbol is null)
        {
            return null;
        }

        var iEnumerableTypeSymbol = compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1");
        if (iEnumerableTypeSymbol is null)
        {
            return null;
        }

        var injectAttributeTypeSymbol = compilation.GetTypeByMetadataName("ManualDi.Async.InjectAttribute");
        if (injectAttributeTypeSymbol is null)
        {
            return null;
        }

        var obsoleteAttributeTypeSymbol = compilation.GetTypeByMetadataName("System.ObsoleteAttribute");
        if (obsoleteAttributeTypeSymbol is null)
        {
            return null;
        }

        var iDisposableTypeSymbol = compilation.GetTypeByMetadataName("System.IDisposable");
        if (iDisposableTypeSymbol is null)
        {
            return null;
        }
        
        var cancellationTokenTypeSymbol = compilation.GetTypeByMetadataName("System.Threading.CancellationToken");
        if (cancellationTokenTypeSymbol is null)
        {
            return null;
        }
        
        var taskTypeSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        if (taskTypeSymbol is null)
        {
            return null;
        }
        
        return new TypeReferences(unityEngineObjectTypeSymbol, lazyTypeSymbol, listTypeSymbol, iListTypeSymbol, iReadOnlyListTypeSymbol, iEnumerableTypeSymbol, iReadOnlyCollectionTypeSymbol, iCollectionTypeSymbol, injectAttributeTypeSymbol, obsoleteAttributeTypeSymbol, iDisposableTypeSymbol, diContainerTypeSymbol, cancellationTokenTypeSymbol, taskTypeSymbol);
    }
    
    public ITypeSymbol? TryGenericLazyType(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
        {
            if (SymbolEqualityComparer.Default.Equals(namedTypeSymbol.OriginalDefinition, LazyTypeSymbol))
            {
                return namedTypeSymbol.TypeArguments[0];
            }
        }

        return null;
    }

    public ITypeSymbol? TryGetEnumerableType(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
        {
            if (SymbolEqualityComparer.Default.Equals(namedTypeSymbol.OriginalDefinition, ListTypeSymbol) ||
                SymbolEqualityComparer.Default.Equals(namedTypeSymbol.OriginalDefinition, IReadOnlyListTypeSymbol) ||
                SymbolEqualityComparer.Default.Equals(namedTypeSymbol.OriginalDefinition, IListTypeSymbol) || 
                SymbolEqualityComparer.Default.Equals(namedTypeSymbol.OriginalDefinition, ICollectionTypeSymbol) || 
                SymbolEqualityComparer.Default.Equals(namedTypeSymbol.OriginalDefinition, IReadOnlyCollectionTypeSymbol) || 
                SymbolEqualityComparer.Default.Equals(namedTypeSymbol.OriginalDefinition, IEnumerableTypeSymbol))
            {
                return namedTypeSymbol.TypeArguments[0];
            }
        }

        return null;
    }
    
    public bool IsSymbolObsolete(ISymbol typeSymbol)
    {
        return typeSymbol.GetAttributes()
            .Any(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, ObsoleteAttributeTypeSymbol));
    }
    
    public AttributeData? GetInjectAttribute(IPropertySymbol propertySymbol)
    {
        if (propertySymbol.IsStatic)
        {
            return null;
        }
            
        bool isSetterAccessible = propertySymbol.SetMethod?.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal;
        if (!isSetterAccessible)
        {
            return null;
        }
            
        bool isPropertyAccessible = propertySymbol.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal;
        if (!isPropertyAccessible)
        {
            return null;
        }

        return propertySymbol
            .GetAttributes()
            .FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, InjectAttributeTypeSymbol));
    }
        
    public AttributeData? GetInjectAttribute(ISymbol parameterSymbol)
    {
        return parameterSymbol
            .GetAttributes()
            .FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, InjectAttributeTypeSymbol));
    }
    
    public bool IsSymbolDiContainer(ITypeSymbol typeSymbol)
    {
        return SymbolEqualityComparer.Default.Equals(IDiContainerTypeSymbol, typeSymbol);
    }
    
    public bool IsIDisposable(INamedTypeSymbol namedTypeSymbol)
    {
        foreach (var implementedInterface in namedTypeSymbol.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(implementedInterface, IDisposableTypeSymbol))
            {
                return true;
            }
        }

        return false;
    }
    
    public bool IsUnityEngineObject(INamedTypeSymbol namedTypeSymbol)
    {
        if (UnityEngineObjectTypeSymbol is null)
        {
            return false;
        }
        
        var baseType = namedTypeSymbol;
        while (baseType != null)
        {
            if (SymbolEqualityComparer.Default.Equals(baseType, UnityEngineObjectTypeSymbol))
            {
                return true;
            }
            baseType = baseType.BaseType;
        }
        return false;
    }

    public bool IsCancellationToken(ITypeSymbol namedTypeSymbol)
    {
        return SymbolEqualityComparer.Default.Equals(namedTypeSymbol, CancellationTokenTypeSymbol);
    }

    public bool IsTask(ITypeSymbol namedTypeSymbol)
    {
        return SymbolEqualityComparer.Default.Equals(namedTypeSymbol, TaskTypeSymbol);
    }
}
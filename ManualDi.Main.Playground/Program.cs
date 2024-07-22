using ManualDi.Main;

Console.WriteLine("ManualDi Playground");

var container = new DiContainerBuilder().Install(b =>
{
    
    b.Bind<PublicClass>().Default();
    b.Bind<InternalClass>().Default();
    b.Bind<InternalClass2>().Default();
    b.Bind<StaticClass.PublicNestedClass>().Default();
    b.Bind<StaticClass.InternalNestedClass>().Default();
    b.Bind<PublicClass>().Default();
}).Build();

public class PublicClass { }
internal class InternalClass { }
class InternalClass2 { }
class GenericClass<T> { }

class ConstructorWithGenericArgument
{
    public ConstructorWithGenericArgument(Func<int> func) { }
}

[Obsolete]
class ObsoleteClass
{
    public void Inject() { }
    public void Initialize() { }
}

static class StaticClass 
{
    public class PublicNestedClass { }
    internal class InternalNestedClass { }
    private class PrivateNestedClass { }
    static class StaticNestedClass { }
}
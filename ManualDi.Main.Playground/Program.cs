using ManualDi.Main;

Console.WriteLine("ManualDi Playground");

var container = new DiContainerBuilder().Install(b =>
{
    b.Bind<Public>().Default();
    b.Bind<Internal>().Default();
    b.Bind<Internal2>().Default();
    b.Bind<Static.PublicNested>().Default();
    b.Bind<Static.InternalNested>().Default();
    b.Bind<Public>().Default(new Public());
#pragma warning disable CS0612 // Type or member is obsolete
    b.Bind<Obsolete>().Default();
#pragma warning restore CS0612 // Type or member is obsolete
}).Build();

public class Public { }
internal class Internal { }
class Internal2 { }
class Generic<T> { }

class ConstructorWithGenericArgument
{
    public ConstructorWithGenericArgument(Func<int> func) { }
}

class InjectPropertyAndMethod
{
    [Inject] public object Object { get; set; }

    public void Inject(object obj)
    {
    }
}

class InjectPropertyClass
{
    [Inject] public object Object { get; set; }
    [Inject] internal object Object2 { get; set; }
    [Inject] private object Object3 { get; set; }
    [Inject] protected object Object4 { get; set; }
    [Inject] public object Object5 { get; internal set; }
    [Inject] public object Object6 { get; private set; }
    [Inject] public object Object7 { get; protected set; }
}

[Obsolete]
class Obsolete
{
    public void Inject() { }
    public void Initialize() { }
}

static class Static 
{
    public class PublicNested { }
    internal class InternalNested { }
    private class PrivateNested { }
    static class StaticNested { }
}
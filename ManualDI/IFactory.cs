namespace ManualDI
{
    public interface IFactory<T>
    {
        T Create();
    }
}

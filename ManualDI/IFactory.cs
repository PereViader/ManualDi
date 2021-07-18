namespace ManualDi
{
    public interface IFactory<T>
    {
        T Create();
    }
}

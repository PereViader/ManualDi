namespace ManualDI
{
    public class CarFactory : IFactory<Car>
    {
        public IContainer Container { get; }

        public CarFactory(IContainer container)
        {
            Container = container;
        }

        public Car Create()
        {
            return new Car();
        }
    }

    public class Car
    {
        public void Init(Person person)
        {
            this.Person = person;
        }

        public Person Person { get; set; }
    }

    public class Person
    {
        public void Init(Car car)
        {
            this.Car = car;
        }

        public Car Car { get; set; }
    }
}

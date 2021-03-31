using ManualDI;

namespace Program
{
    public class Fruit
    {
        public string Name { get; }

        public Fruit(string name)
        {
            Name = name;
        }
    }

    public class CarFactory : IFactory<Car>
    {
        public IDiContainer Container { get; }

        public CarFactory(IDiContainer container)
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
        public Person(Car car)
        {
            this.Car = car;
        }

        public Car Car { get; set; }
    }
}

using ManualDI;
using System;

namespace Program
{
    class Program
    {
        static void Main()
        {
            var container = new ContainerBuilder().Build();
            
            container.Bind<Car>(x => x
                .Single()
                .FromFactory<CarFactory, Car>()
                .Inject((o,x) => o.Init(x.Resolve<Person>()))
                );

            container.Bind<CarFactory>(x => x
                .Single()
                .FromFunc(x => new CarFactory(container))
                );

            container.Bind<Person>(x => x
                .Single()
                .FromFunc(x => new Person(x.Resolve<Car>()))
                );

            var car = container.Resolve<Car>();
            var person = container.Resolve<Person>();

            Console.WriteLine(car.Person == person);
            Console.WriteLine(person.Car == car);
        }
    }
}

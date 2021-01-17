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
                .Id("Potato")
                .Single()
                .FromFunc(x => new Car())
                .Inject((o,x) => o.Init(x.Resolve<Person>()))
                );

            container.Bind<Person>(x => x
                .Single()
                .FromFunc(x => new Person(x.Resolve<Car>(x => 
                    x.Id("Potato")
                    )))
                );

            var car = container.Resolve<Car>(x => x.Id("Potato"));

            var person = container.Resolve<Person>();

            Console.WriteLine(car.Person == person);
            Console.WriteLine(person.Car == car);
        }
    }
}

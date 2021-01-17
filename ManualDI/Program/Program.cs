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
                .Metadata("UNITY_EDITOR")
                .Metadata("Speed", 5)
                .FromMethod(x => new Car())
                .Inject((o,x) => 
                    o.Init(x.Resolve<Person>()
                    ))
                );

            container.Bind<Person>(x => x
                .Single()
                .FromMethod(x => new Person(x.Resolve<Car>(x => x
                    .Id("Potato")
                    .Metadata("UNITY_EDITOR")
                    .Metadata("Speed",5)
                    )))
                );

            var car = container.Resolve<Car>();
            var person = container.Resolve<Person>();

            Console.WriteLine(car.Person == person);
            Console.WriteLine(person.Car == car);
        }
    }
}

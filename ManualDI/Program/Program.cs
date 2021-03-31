using ManualDI;
using System;
using System.Collections.Generic;

namespace Program
{
    class Program
    {
        static void Main()
        {
            var container = new ContainerBuilder().Build();

            container.Bind<Fruit>(x => x.FromInstance(new Fruit("Apple")).Metadata("Fruit1"));
            container.Bind<Fruit>(x => x.FromInstance(new Fruit("Orange")).Metadata("Fruit2"));
            container.Bind<Fruit>(x => x.FromInstance(new Fruit("Banana")).Metadata("Fruit3"));
            container.Bind<List<Fruit>>(x => x.FromContainerAll(x => x.Metadata("Fruit3")).Metadata("OnlySomeSpecificFruits"));
            container.Bind<List<Fruit>>(x => x.FromContainerAll().Metadata("AllFruit"));

            var boundFilteredFruit = container.Resolve<List<Fruit>>(x => x.Metadata("OnlySomeSpecificFruits"));
            var allFruitLists = container.ResolveAll<List<Fruit>>();
            var allFruit = container.ResolveAll<Fruit>();
            var fruit1And3 = container.ResolveAll<Fruit>(x => x.Metadata(x => x.Has("Fruit1") || x.Has("Fruit3")));

            container.Bind<Car>(x => x
                .Metadata("Potato")
                .Single()
                .Metadata("UNITY_EDITOR")
                .Metadata("Speed", 5)
                .FromMethod(x => new Car())
                .Inject((o, x) =>
                    o.Init(x.Resolve<Person>()
                    ))
                );

            container.Bind<Person>(x => x
                .Single()
                .FromMethod(x => new Person(x.Resolve<Car>(x => x
                    .Metadata("Potato")
                    .Metadata("UNITY_EDITOR")
                    .Metadata("Speed", 5)
                    )))
                );

            var car = container.Resolve<Car>();
            var person = container.Resolve<Person>();

            Console.WriteLine(car.Person == person);
            Console.WriteLine(person.Car == car);
        }
    }
}

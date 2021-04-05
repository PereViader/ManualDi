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

            container.Bind<Fruit>(x => x.FromInstance(new Fruit("Apple")).WithMetadata("Fruit1"));
            container.Bind<Fruit>(x => x.FromInstance(new Fruit("Orange")).WithMetadata("Fruit2"));
            container.Bind<Fruit>(x => x.FromInstance(new Fruit("Banana")).WithMetadata("Fruit3"));
            container.Bind<List<Fruit>>(x => x.FromContainerAll(x => x.WhereMetadata("Fruit3")).WithMetadata("OnlySomeSpecificFruits"));
            container.Bind<List<Fruit>>(x => x.FromContainerAll().WithMetadata("AllFruit"));

            var boundFilteredFruit = container.Resolve<List<Fruit>>(x => x.WhereMetadata("OnlySomeSpecificFruits"));
            var allFruitLists = container.ResolveAll<List<Fruit>>();
            var allFruit = container.ResolveAll<Fruit>();
            var fruit1And3 = container.ResolveAll<Fruit>(x => x.WhereMetadata(x => x.Has("Fruit1") || x.Has("Fruit3")));

            container.Bind<Car>(x => x
                .WithMetadata("Potato")
                .Single()
                .WithMetadata("UNITY_EDITOR")
                .WithMetadata("Speed", 5)
                .FromMethod(x => new Car())
                .Inject((o, x) =>
                    o.Init(x.Resolve<Person>()
                    ))
                );

            container.Bind<Person>(x => x
                .Single()
                .FromMethod(x => new Person(x.Resolve<Car>(x => x
                    .WhereMetadata("Potato")
                    .WhereMetadata("UNITY_EDITOR")
                    .WhereMetadata("Speed", 5)
                    )))
                );

            var car = container.Resolve<Car>();
            var person = container.Resolve<Person>();

            Console.WriteLine(car.Person == person);
            Console.WriteLine(person.Car == car);
        }
    }
}

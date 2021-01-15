using System;
using System.Net.Http.Headers;
using System.Reflection.Metadata;

namespace ManualDI
{
    class Program
    {
        static void Main()
        {
            var container = new Container();
            
            container.Bind<Car>(new TypeSetup<Car>(TypeScope.Single, new FactoryTypeFactory<CarFactory, Car>(), new ActionTypeInjection<Car>((c,x) => x.Init(c.Resolve<Person>()))));
            container.Bind<CarFactory>(new TypeSetup<CarFactory>(TypeScope.Single, new FuncTypeFactory<CarFactory>(x => new CarFactory(x)), NopTypeInjection<CarFactory>.Instance));
            container.Bind<Person>(new TypeSetup<Person>(TypeScope.Single, new FuncTypeFactory<Person>(x => new Person()), new ActionTypeInjection<Person>((c,x) => x.Init(c.Resolve<Car>()))));

            var car = container.Resolve<Car>();
            var person = container.Resolve<Person>();
            Console.WriteLine(car.Person == person);
            Console.WriteLine(person.Car == car);
        }
    }
}

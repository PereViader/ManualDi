using System;

namespace ManualDI
{
    class Program
    {
        static void Main()
        {
            var container = new ContainerBuilder().Build();
            
            container.Bind<Car>(new TypeBinding<Car>(SingleTypeScope.Instance, new FactoryTypeFactory<CarFactory, Car>(), new ActionTypeInjection<Car>((c,x) => x.Init(c.Resolve<Person>()))));
            container.Bind<CarFactory>(new TypeBinding<CarFactory>(SingleTypeScope.Instance, new FuncTypeFactory<CarFactory>(x => new CarFactory(x)), NopTypeInjection<CarFactory>.Instance));
            container.Bind<Person>(new TypeBinding<Person>(SingleTypeScope.Instance, new FuncTypeFactory<Person>(x => new Person(x.Resolve<Car>())), NopTypeInjection<Person>.Instance));

            var car = container.Resolve<Car>();
            var person = container.Resolve<Person>();
            Console.WriteLine(car.Person == person);
            Console.WriteLine(person.Car == car);
        }
    }
}

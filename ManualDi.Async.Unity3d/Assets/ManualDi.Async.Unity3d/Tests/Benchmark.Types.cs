using System;

namespace ManualDi.Async.Unity3d.Tests
{
    public interface IFirstService { }
    [ManualDi]
    public class FirstService : IFirstService { }
    
    public interface ISecondService { }
    [ManualDi]
    public class SecondService : ISecondService { }
    
    public interface IThirdService { }
    [ManualDi]
    public class ThirdService : IThirdService { }
    
    public interface IServiceA { }
    [ManualDi]
    public class ServiceA : IServiceA
    {
        public ServiceA() { }
    }
    
    public interface ISubObjectA { }
    [ManualDi]
    public class SubObjectA : ISubObjectA
    {
        [Zenject.Inject]
        [VContainer.Inject]
        public IServiceA ServiceA { get; set; }

        public void Inject(IServiceA serviceA)
        {
            ServiceA = serviceA;
        }
    }
    
    public interface IServiceB { }
    [ManualDi]
    public class ServiceB : IServiceB
    {
        public ServiceB() { }
    }
    
    public interface ISubObjectB { }
    [ManualDi]
    public class SubObjectB : ISubObjectB
    {
        [Zenject.Inject]
        [VContainer.Inject]
        public IServiceB ServiceB { get; set; }
        
        public void Inject(IServiceB serviceB)
        {
            ServiceB = serviceB;
        }
    }
    
    public interface IServiceC { }
    [ManualDi]
    public class ServiceC : IServiceC
    {
        public ServiceC() { }
    }
    
    public interface ISubObjectC { }
    [ManualDi]
    public class SubObjectC : ISubObjectC
    {
        [Zenject.Inject]
        [VContainer.Inject]
        public IServiceC ServiceC { get; set; }

        public void Inject(IServiceC serviceC)
        {
            ServiceC = serviceC;
        }
    }
    
    public interface IComplex1 { }
    [ManualDi]
    public class Complex1 : IComplex1
    {
        public Complex1(
            IFirstService firstService,
            ISecondService secondService,
            IThirdService thirdService,
            ISubObjectOne subObjectOne,
            ISubObjectTwo subObjectTwo,
            ISubObjectThree subObjectThree)
        {
            _ = firstService ?? throw new ArgumentNullException(nameof(firstService));
            _ = secondService ?? throw new ArgumentNullException(nameof(secondService));
            _ = thirdService ?? throw new ArgumentNullException(nameof(thirdService));
            _ = subObjectOne ?? throw new ArgumentNullException(nameof(subObjectOne));
            _ = subObjectTwo ?? throw new ArgumentNullException(nameof(subObjectTwo));
            _ = subObjectThree ?? throw new ArgumentNullException(nameof(subObjectThree));
        }
    }

    public interface IComplex2 { }
    [ManualDi]
    public class Complex2 : IComplex2
    {
        public Complex2(
            IFirstService firstService,
            ISecondService secondService,
            IThirdService thirdService,
            ISubObjectOne subObjectOne,
            ISubObjectTwo subObjectTwo,
            ISubObjectThree subObjectThree)
        {
            _ = firstService ?? throw new ArgumentNullException(nameof(firstService));
            _ = secondService ?? throw new ArgumentNullException(nameof(secondService));
            _ = thirdService ?? throw new ArgumentNullException(nameof(thirdService));
            _ = subObjectOne ?? throw new ArgumentNullException(nameof(subObjectOne));
            _ = subObjectTwo ?? throw new ArgumentNullException(nameof(subObjectTwo));
            _ = subObjectThree ?? throw new ArgumentNullException(nameof(subObjectThree));
        }
    }

    public interface IComplex3 { }
    [ManualDi]
    public class Complex3 : IComplex3
    {
        public Complex3(
            IFirstService firstService,
            ISecondService secondService,
            IThirdService thirdService,
            ISubObjectOne subObjectOne,
            ISubObjectTwo subObjectTwo,
            ISubObjectThree subObjectThree)
        {
            _ = firstService ?? throw new ArgumentNullException(nameof(firstService));
            _ = secondService ?? throw new ArgumentNullException(nameof(secondService));
            _ = thirdService ?? throw new ArgumentNullException(nameof(thirdService));
            _ = subObjectOne ?? throw new ArgumentNullException(nameof(subObjectOne));
            _ = subObjectTwo ?? throw new ArgumentNullException(nameof(subObjectTwo));
            _ = subObjectThree ?? throw new ArgumentNullException(nameof(subObjectThree));
        }
    }
    
    public interface ISubObjectOne { }
    [ManualDi]
    public class SubObjectOne : ISubObjectOne
    {
        public SubObjectOne(IFirstService firstService)
        {
            _ = firstService ?? throw new ArgumentNullException(nameof(firstService));
        }
    }
    
    public interface ISubObjectTwo { }
    [ManualDi]
    public class SubObjectTwo : ISubObjectTwo
    {
        public SubObjectTwo(ISecondService secondService)
        {
            _ = secondService ?? throw new ArgumentNullException(nameof(secondService));
        }
    }
    
    public interface ISubObjectThree { }
    [ManualDi]
    public class SubObjectThree : ISubObjectThree
    {
        public SubObjectThree(IThirdService thirdService)
        {
            _ = thirdService ?? throw new ArgumentNullException(nameof(thirdService));
        }
    }
}
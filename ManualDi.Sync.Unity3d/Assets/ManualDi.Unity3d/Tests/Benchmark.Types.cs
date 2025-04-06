using System;

namespace ManualDi.Unity3d.Tests
{
    public interface IFirstService { }
    public class FirstService : IFirstService { }
    
    public interface ISecondService { }
    public class SecondService : ISecondService { }
    
    public interface IThirdService { }
    public class ThirdService : IThirdService { }
    
    public interface IServiceA { }
    public class ServiceA : IServiceA
    {
        public ServiceA() { }
    }
    
    public interface ISubObjectA { }
    public class SubObjectA : ISubObjectA
    {
        [Zenject.Inject]
        [VContainer.Inject]
        [ManualDi.Main.Inject]
        public IServiceA ServiceA { get; set; }
    }
    
    public interface IServiceB { }
    public class ServiceB : IServiceB
    {
        public ServiceB() { }
    }
    
    public interface ISubObjectB { }
    public class SubObjectB : ISubObjectB
    {
        [Zenject.Inject]
        [VContainer.Inject]
        [ManualDi.Main.Inject]
        public IServiceB ServiceB { get; set; }
    }
    
    public interface IServiceC { }
    public class ServiceC : IServiceC
    {
        public ServiceC() { }
    }
    
    public interface ISubObjectC { }
    public class SubObjectC : ISubObjectC
    {
        [Zenject.Inject]
        [VContainer.Inject]
        [ManualDi.Main.Inject]
        public IServiceC ServiceC { get; set; }
    }
    
    public interface IComplex1 { }
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
    public class SubObjectOne : ISubObjectOne
    {
        public SubObjectOne(IFirstService firstService)
        {
            _ = firstService ?? throw new ArgumentNullException(nameof(firstService));
        }
    }
    
    public interface ISubObjectTwo { }
    public class SubObjectTwo : ISubObjectTwo
    {
        public SubObjectTwo(ISecondService secondService)
        {
            _ = secondService ?? throw new ArgumentNullException(nameof(secondService));
        }
    }
    
    public interface ISubObjectThree { }
    public class SubObjectThree : ISubObjectThree
    {
        public SubObjectThree(IThirdService thirdService)
        {
            _ = thirdService ?? throw new ArgumentNullException(nameof(thirdService));
        }
    }
}
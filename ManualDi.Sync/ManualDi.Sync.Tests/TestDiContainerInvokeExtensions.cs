using NUnit.Framework;
using NSubstitute;
using System;

namespace ManualDi.Sync.Tests
{
    public class TestDiContainerInvokeExtensions
    {
        [Test]
        public void TestInvoke_Int_Resolved()
        {
            var container = new DiContainerBindings().Install(b =>
            {
                b.Bind<int>().FromInstance(5);
            }).Build();
            
            int? result = null;
            container.InvokeDelegateUsingReflexion((int value)  => result = value);
            
            Assert.That(result, Is.EqualTo(5));
        }

        [Test]
        public void TestInvoke_Int_NotResolved_Throws()
        {
            var container = new DiContainerBindings().Install(b =>
            {
            }).Build();
            
            Assert.Throws<InvalidOperationException>(() =>
            {
                container.InvokeDelegateUsingReflexion((int a) => { });
            });
        }

        [Test]
        public void TestInvoke_NullableInt_Resolved()
        {
            var container = new DiContainerBindings().Install(b =>
            {
                b.Bind<int>().FromInstance(5);
            }).Build();
            
            int? result = null;
            container.InvokeDelegateUsingReflexion((int? a) => result = a);
            
            Assert.That(result, Is.EqualTo(5));
        }

        [Test]
        public void TestInvoke_NullableInt_NotResolved_ReturnsNull()
        {
            var container = new DiContainerBindings().Install(b =>
            {
            }).Build();
            
            int? result = 5; // Initialize with non-null
            container.InvokeDelegateUsingReflexion((int? a) => result = a);
            
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TestInvoke_String_Resolved()
        {
            var container = new DiContainerBindings().Install(b =>
            {
                b.Bind<string>().FromInstance("hello");
            }).Build();

            string? result = null;
            container.InvokeDelegateUsingReflexion((string s) => result = s);
            
            Assert.That(result, Is.EqualTo("hello"));
        }

        [Test]
        public void TestInvoke_String_NotResolved_Throws()
        {
            var container = new DiContainerBindings().Install(b =>
            {
            }).Build();
            
            Assert.Throws<InvalidOperationException>(() =>
            {
                container.InvokeDelegateUsingReflexion((string s) => { });
            });
        }

        [Test]
        public void TestInvoke_NullableString_NotResolved_ReturnsNull()
        {
            var container = new DiContainerBindings().Install(b =>
            {
            }).Build();
            
            string? result = "not null";
            container.InvokeDelegateUsingReflexion((string? s) => result = s);
            
            Assert.That(result, Is.Null);
        }
        
        [Test]
        public void TestInvoke_Mixed()
        {
            var container = new DiContainerBindings().Install(b =>
            {
                b.Bind<int>().FromInstance(42);
            }).Build();
            
            int resInt = 0;
            string? resString = "initial";
            container.InvokeDelegateUsingReflexion((int a, string? s) => 
            {
                resInt = a;
                resString = s;
            });
            
            Assert.That(resInt, Is.EqualTo(42));
            Assert.That(resString, Is.Null);
        }
        
        [Test]
        public void TestInvoke_WithIdAttribute_ResolvesCorrectInstance()
        {
            var container = new DiContainerBindings().Install(b =>
            {
                b.Bind<int>().FromInstance(1).WithId("A");
                b.Bind<int>().FromInstance(2).WithId("B");
            }).Build();

            int resA = 0;
            int resB = 0;
            container.InvokeDelegateUsingReflexion(([Id("A")] int a, [Id("B")] int b) => 
            {
                resA = a;
                resB = b;
            });
            
            Assert.That(resA, Is.EqualTo(1));
            Assert.That(resB, Is.EqualTo(2));
        }
    }
}

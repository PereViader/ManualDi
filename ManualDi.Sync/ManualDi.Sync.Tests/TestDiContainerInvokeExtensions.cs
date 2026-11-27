using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ManualDi.Sync.Tests
{
    public class TestDiContainerInvokeExtensions
    {
        [Test]
        public void TestInvoke_Int_Resolved()
        {
            using var container = new DiContainerBindings().Install(b =>
            {
                b.Bind<int>().FromInstance(5);
            }).Build();
            
            int? result = null;
            container.InvokeDelegateUsingReflection((int value)  => result = value);
            
            Assert.That(result, Is.EqualTo(5));
        }

        [Test]
        public void TestInvoke_Int_NotResolved_Throws()
        {
            using var container = new DiContainerBindings().Install(b =>
            {
            }).Build();
            
            Assert.Throws<InvalidOperationException>(() =>
            {
                container.InvokeDelegateUsingReflection((int a) => { });
            });
        }

        [Test]
        public void TestInvoke_NullableInt_Resolved()
        {
            using var container = new DiContainerBindings().Install(b =>
            {
                b.Bind<int>().FromInstance(5);
            }).Build();
            
            int? result = null;
            container.InvokeDelegateUsingReflection((int? a) => result = a);
            
            Assert.That(result, Is.EqualTo(5));
        }

        [Test]
        public void TestInvoke_NullableInt_NotResolved_ReturnsNull()
        {
            using var container = new DiContainerBindings().Install(b =>
            {
            }).Build();
            
            int? result = 5; // Initialize with non-null
            container.InvokeDelegateUsingReflection((int? a) => result = a);
            
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TestInvoke_String_Resolved()
        {
            using var container = new DiContainerBindings().Install(b =>
            {
                b.Bind<string>().FromInstance("hello");
            }).Build();

            string? result = null;
            container.InvokeDelegateUsingReflection((string s) => result = s);
            
            Assert.That(result, Is.EqualTo("hello"));
        }

        [Test]
        public void TestInvoke_String_NotResolved_Throws()
        {
            using var container = new DiContainerBindings().Install(b =>
            {
            }).Build();
            
            Assert.Throws<InvalidOperationException>(() =>
            {
                container.InvokeDelegateUsingReflection((string s) => { });
            });
        }

        [Test]
        public void TestInvoke_NullableString_NotResolved_ReturnsNull()
        {
            using var container = new DiContainerBindings().Install(b =>
            {
            }).Build();
            
            string? result = "not null";
            container.InvokeDelegateUsingReflection((string? s) => result = s);
            
            Assert.That(result, Is.Null);
        }
        
        [Test]
        public void TestInvoke_Mixed()
        {
            using var container = new DiContainerBindings().Install(b =>
            {
                b.Bind<int>().FromInstance(42);
            }).Build();
            
            int resInt = 0;
            string? resString = "initial";
            container.InvokeDelegateUsingReflection((int a, string? s) => 
            {
                resInt = a;
                resString = s;
            });
            
            Assert.That(resInt, Is.EqualTo(42));
            Assert.That(resString, Is.Null);
        }

        [Test]
        public async Task TestInvokeAsync_VoidReturn()
        {
            using var container = new DiContainerBindings().Install(b =>
            {
                b.Bind<int>().FromInstance(5);
            }).Build();
            
            int result = 0;
            await container.InvokeDelegateUsingReflectionAsync(async (int a) => 
            {
                await Task.CompletedTask;
                result = a;
            });
            
            Assert.That(result, Is.EqualTo(5));
        }

        [Test]
        public async Task TestInvokeAsync_ValueReturn()
        {
            using var container = new DiContainerBindings().Install(b =>
            {
                b.Bind<int>().FromInstance(10);
            }).Build();
            
            var result = await container.InvokeDelegateUsingReflectionAsync(async (int a) => 
            {
                await Task.CompletedTask;
                return a * 2;
            });
            
            Assert.That(result, Is.EqualTo(20));
        }

        [Test]
        public async Task TestInvokeAsync_SyncDelegate_ValueReturn()
        {
            using var container = new DiContainerBindings().Install(b =>
            {
                b.Bind<int>().FromInstance(5);
            }).Build();
            
            var result = await container.InvokeDelegateUsingReflectionAsync((int a) => a * 2);
            
            Assert.That(result, Is.EqualTo(10));
        }
        
        [Test]
        public async Task TestInvokeAsync_SyncDelegate_VoidReturn()
        {
            using var container = new DiContainerBindings().Install(b =>
            {
                b.Bind<int>().FromInstance(5);
            }).Build();

            int result = 0;
            await container.InvokeDelegateUsingReflectionAsync((int a) => result = a);
            
            Assert.That(result, Is.EqualTo(5));
        }
        
        [Test]
        public async Task TestInvokeAsync_CancellationToken_DisposesProperly()
        {
            var container = new DiContainerBindings().Install(b =>
            {
                b.Bind<int>().FromInstance(5);
            }).Build();

            CancellationToken cancellationToken = CancellationToken.None;
            await container.InvokeDelegateUsingReflectionAsync((CancellationToken ct) => cancellationToken = ct);
            
            Assert.That(cancellationToken.IsCancellationRequested, Is.False);

            container.Dispose();
            
            Assert.That(cancellationToken.IsCancellationRequested, Is.True);
        }
    }
}

using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ManualDi.Async.Tests
{
    public class TestDiContainerInvokeExtensions
    {
        [Test]
        public async Task TestInvoke_Int_Resolved()
        {
            await using var container = await new DiContainerBindings().Install(b =>
            {
                b.Bind<int>().FromInstance(5);
            }).Build(CancellationToken.None);
            
            int? result = null;
            container.InvokeDelegateUsingReflexion((int value)  => result = value);
            
            Assert.That(result, Is.EqualTo(5));
        }

        [Test]
        public async Task TestInvoke_Int_NotResolved_Throws()
        {
            await using var container = await new DiContainerBindings().Install(b =>
            {
            }).Build(CancellationToken.None);
            
            Assert.Throws<InvalidOperationException>(() =>
            {
                container.InvokeDelegateUsingReflexion((int a) => { });
            });
        }

        [Test]
        public async Task TestInvoke_NullableInt_Resolved()
        {
            await using var container = await new DiContainerBindings().Install(b =>
            {
                b.Bind<int>().FromInstance(5);
            }).Build(CancellationToken.None);
            
            int? result = null;
            container.InvokeDelegateUsingReflexion((int? a) => result = a);
            
            Assert.That(result, Is.EqualTo(5));
        }

        [Test]
        public async Task TestInvoke_NullableInt_NotResolved_ReturnsNull()
        {
            await using var container = await new DiContainerBindings().Install(b =>
            {
            }).Build(CancellationToken.None);
            
            int? result = 5; // Initialize with non-null
            container.InvokeDelegateUsingReflexion((int? a) => result = a);
            
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task TestInvoke_String_Resolved()
        {
            await using var container = await new DiContainerBindings().Install(b =>
            {
                b.Bind<string>().FromInstance("hello");
            }).Build(CancellationToken.None);

            string? result = null;
            container.InvokeDelegateUsingReflexion((string s) => result = s);
            
            Assert.That(result, Is.EqualTo("hello"));
        }

        [Test]
        public async Task TestInvoke_String_NotResolved_Throws()
        {
            await using var container = await new DiContainerBindings().Install(b =>
            {
            }).Build(CancellationToken.None);
            
            Assert.Throws<InvalidOperationException>(() =>
            {
                container.InvokeDelegateUsingReflexion((string s) => { });
            });
        }

        [Test]
        public async Task TestInvoke_NullableString_NotResolved_ReturnsNull()
        {
            await using var container = await new DiContainerBindings().Install(b =>
            {
            }).Build(CancellationToken.None);
            
            string? result = "not null";
            container.InvokeDelegateUsingReflexion((string? s) => result = s);
            
            Assert.That(result, Is.Null);
        }
        
        [Test]
        public async Task TestInvoke_Mixed()
        {
            await using var container = await new DiContainerBindings().Install(b =>
            {
                b.Bind<int>().FromInstance(42);
            }).Build(CancellationToken.None);
            
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
        public async Task TestInvoke_WithIdAttribute_ResolvesCorrectInstance()
        {
            await using var container = await new DiContainerBindings().Install(b =>
            {
                b.Bind<int>().FromInstance(1).WithId("A");
                b.Bind<int>().FromInstance(2).WithId("B");
            }).Build(CancellationToken.None);

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

        [Test]
        public async Task TestInvokeAsync_VoidReturn()
        {
            await using var container = await new DiContainerBindings().Install(b =>
            {
                b.Bind<int>().FromInstance(5);
            }).Build(CancellationToken.None);
            
            int result = 0;
            await container.InvokeDelegateUsingReflexionAsync(async (int a) => 
            {
                await Task.CompletedTask;
                result = a;
            });
            
            Assert.That(result, Is.EqualTo(5));
        }

        [Test]
        public async Task TestInvokeAsync_ValueReturn()
        {
            await using var container = await new DiContainerBindings().Install(b =>
            {
                b.Bind<int>().FromInstance(10);
            }).Build(CancellationToken.None);
            
            var result = await container.InvokeDelegateUsingReflexionAsync(async (int a) => 
            {
                await Task.CompletedTask;
                return a * 2;
            });
            
            Assert.That(result, Is.EqualTo(20));
        }

        [Test]
        public async Task TestInvokeAsync_SyncDelegate_ValueReturn()
        {
            await using var container = await new DiContainerBindings().Install(b =>
            {
                b.Bind<int>().FromInstance(5);
            }).Build(CancellationToken.None);
            
            var result = await container.InvokeDelegateUsingReflexionAsync((int a) => a * 2);
            
            Assert.That(result, Is.EqualTo(10));
        }
        
        [Test]
        public async Task TestInvokeAsync_SyncDelegate_VoidReturn()
        {
            await using var container = await new DiContainerBindings().Install(b =>
            {
                b.Bind<int>().FromInstance(5);
            }).Build(CancellationToken.None);

            int result = 0;
            await container.InvokeDelegateUsingReflexionAsync((int a) => result = a);
            
            Assert.That(result, Is.EqualTo(5));
        }
        
        [Test]
        public async Task TestInvoke_CancellationToken_DisposesProperly()
        {
            var container = await new DiContainerBindings().Install(b =>
            {
                b.Bind<int>().FromInstance(5);
            }).Build(CancellationToken.None);

            CancellationToken cancellationToken = CancellationToken.None;
            await container.InvokeDelegateUsingReflexionAsync((CancellationToken ct) => cancellationToken = ct);
            
            Assert.That(cancellationToken.IsCancellationRequested, Is.False);

            await container.DisposeAsync();
            
            Assert.That(cancellationToken.IsCancellationRequested, Is.True);
        }
    }
}

using NUnit.Framework;
using System.Collections.Generic;

namespace ManualDi.Main.Tests
{
    public class TestDiContainerNonGenericResolveAll
    {
        public class Parent
        {
        }
        public class Child : Parent
        {
        }

        [Test]
        public void TestResolveAllEmptyReturnsEmpty()
        {
            var container = new DiContainerBuilder().Build();

            List<object> resolved = container.ResolveAll(typeof(int));

            Assert.That(resolved, Is.Empty);
        }

        [Test]
        public void TestResolveAllDifferentTypeList()
        {
            var child = new Child();
            var container = new DiContainerBuilder().Install(b =>
            {
                b.Bind<Child>().FromInstance(child);
            }).Build();

            List<Parent> resolved = container.ResolveAll<Parent>(typeof(Child));

            Assert.That(resolved, Is.EquivalentTo(new Parent[] { child }));
        }
    }
}

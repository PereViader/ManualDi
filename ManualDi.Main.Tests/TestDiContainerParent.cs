using NUnit.Framework;

namespace ManualDi.Main.Tests
{
    public class TestDiContainerParent
    {
        [Test]
        public void TestResolve()
        {
            var parentContainer = new ContainerBuilder().Build();
            var childContainer = new ContainerBuilder().WithParentContainer(parentContainer).Build();

            var instance = new object();
            parentContainer.Bind<object>().FromInstance(instance);

            var resolution = childContainer.Resolve<object>();

            Assert.That(resolution, Is.EqualTo(instance));
        }

        [Test]
        public void TestResolveAll()
        {
            var parentContainer = new ContainerBuilder().Build();
            var childContainer = new ContainerBuilder().WithParentContainer(parentContainer).Build();

            var instanceParent = new object();
            parentContainer.Bind<object>().FromInstance(instanceParent);


            var instanceChild = new object();
            childContainer.Bind<object>().FromInstance(instanceChild);

            var resolution = childContainer.ResolveAll<object>();

            Assert.That(resolution, Is.EquivalentTo(new object[] { instanceParent, instanceChild }));
        }
    }
}

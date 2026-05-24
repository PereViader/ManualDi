using NUnit.Framework;
using ManualDi.Sync.Generators;

namespace ManualDi.Sync.Tests;

public class TestGeneratorModelEquality
{
    [Test]
    public void Test_ClassData_Equality()
    {
        var classData1 = CreateDefaultClassData();
        var classData2 = CreateDefaultClassData();

        Assert.That(classData1, Is.EqualTo(classData2));
        Assert.That(classData1.GetHashCode(), Is.EqualTo(classData2.GetHashCode()));
        Assert.That(classData1 == classData2, Is.True);
    }

    [Test]
    public void Test_ClassData_Inequality_DifferentFields()
    {
        var baseClassData = CreateDefaultClassData();

        // Let's verify each field modification changes equality and hash code
        Assert.That(baseClassData, Is.Not.EqualTo(baseClassData with { FileName = "Diff" }));
        Assert.That(baseClassData, Is.Not.EqualTo(baseClassData with { ClassName = "Diff" }));
        Assert.That(baseClassData, Is.Not.EqualTo(baseClassData with { Namespace = "Diff" }));
        Assert.That(baseClassData, Is.Not.EqualTo(baseClassData with { Accessibility = "Diff" }));
        Assert.That(baseClassData, Is.Not.EqualTo(baseClassData with { TypeParameters = "Diff" }));
        Assert.That(baseClassData, Is.Not.EqualTo(baseClassData with { TypeParameterConstraints = "Diff" }));
        Assert.That(baseClassData, Is.Not.EqualTo(baseClassData with { ObsoleteText = "Diff" }));
        Assert.That(baseClassData, Is.Not.EqualTo(baseClassData with { HasInitializeMethod = !baseClassData.HasInitializeMethod }));
        Assert.That(baseClassData, Is.Not.EqualTo(baseClassData with { IsDisposable = !baseClassData.IsDisposable }));
        Assert.That(baseClassData, Is.Not.EqualTo(baseClassData with { IsSealed = !baseClassData.IsSealed }));
        Assert.That(baseClassData, Is.Not.EqualTo(baseClassData with { HasBaseDi = !baseClassData.HasBaseDi }));
        
        // BaseTypeCall modification
        Assert.That(baseClassData, Is.Not.EqualTo(baseClassData with { BaseTypeCall = new ManualDiSourceGenerator.BaseTypeCall("Base2", "T") }));
        
        // ConstructorParameters modification
        var diffConstructorParams = new EquatableArray<ManualDiSourceGenerator.Resolution>(new ManualDiSourceGenerator.Resolution[]
        {
            new ManualDiSourceGenerator.ServiceResolution("Int32", null, "Resolve")
        });
        Assert.That(baseClassData, Is.Not.EqualTo(baseClassData with { ConstructorParameters = diffConstructorParams }));

        // InjectMethodParameters modification
        var diffInjectParams = new EquatableArray<ManualDiSourceGenerator.Resolution>(new ManualDiSourceGenerator.Resolution[]
        {
            new ManualDiSourceGenerator.ServiceResolution("Int32", null, "Resolve")
        });
        Assert.That(baseClassData, Is.Not.EqualTo(baseClassData with { InjectMethodParameters = diffInjectParams }));
    }

    [Test]
    public void Test_BaseTypeCall_Equality()
    {
        var call1 = new ManualDiSourceGenerator.BaseTypeCall("Base", "T");
        var call2 = new ManualDiSourceGenerator.BaseTypeCall("Base", "T");
        var call3 = new ManualDiSourceGenerator.BaseTypeCall("Base2", "T");

        Assert.That(call1, Is.EqualTo(call2));
        Assert.That(call1.GetHashCode(), Is.EqualTo(call2.GetHashCode()));
        Assert.That(call1, Is.Not.EqualTo(call3));
    }

    [Test]
    public void Test_Resolution_ServiceResolution_Equality()
    {
        var res1 = new ManualDiSourceGenerator.ServiceResolution("Int32", "Id", "Resolve");
        var res2 = new ManualDiSourceGenerator.ServiceResolution("Int32", "Id", "Resolve");
        var res3 = new ManualDiSourceGenerator.ServiceResolution("Int32", null, "Resolve");

        Assert.That(res1, Is.EqualTo(res2));
        Assert.That(res1.GetHashCode(), Is.EqualTo(res2.GetHashCode()));
        Assert.That(res1, Is.Not.EqualTo(res3));
    }

    [Test]
    public void Test_Resolution_EnumerableResolution_Equality()
    {
        var info1 = new ManualDiSourceGenerator.EnumerableInfo(true, false, "Int32?", false);
        var info2 = new ManualDiSourceGenerator.EnumerableInfo(true, false, "Int32?", false);
        var info3 = new ManualDiSourceGenerator.EnumerableInfo(false, false, "Int32", false);

        var res1 = new ManualDiSourceGenerator.EnumerableResolution("List", "Id", info1);
        var res2 = new ManualDiSourceGenerator.EnumerableResolution("List", "Id", info2);
        var res3 = new ManualDiSourceGenerator.EnumerableResolution("List", "Id", info3);

        Assert.That(res1, Is.EqualTo(res2));
        Assert.That(res1.GetHashCode(), Is.EqualTo(res2.GetHashCode()));
        Assert.That(res1, Is.Not.EqualTo(res3));
    }

    [Test]
    public void Test_Singleton_Resolutions_Equality()
    {
        Assert.That(ManualDiSourceGenerator.OutResolution.Instance, Is.EqualTo(new ManualDiSourceGenerator.OutResolution()));
        Assert.That(ManualDiSourceGenerator.CancellationTokenResolution.Instance, Is.EqualTo(new ManualDiSourceGenerator.CancellationTokenResolution()));
        Assert.That(ManualDiSourceGenerator.ContainerResolution.Instance, Is.EqualTo(new ManualDiSourceGenerator.ContainerResolution()));

        Assert.That(ManualDiSourceGenerator.OutResolution.Instance, Is.Not.EqualTo(ManualDiSourceGenerator.CancellationTokenResolution.Instance));
        Assert.That(ManualDiSourceGenerator.CancellationTokenResolution.Instance, Is.Not.EqualTo(ManualDiSourceGenerator.ContainerResolution.Instance));
    }

    [Test]
    public void Test_EquatableArray_Equality()
    {
        var res1 = new ManualDiSourceGenerator.ServiceResolution("Int32", null, "Resolve");
        var res2 = new ManualDiSourceGenerator.ServiceResolution("String", null, "Resolve");

        var arr1 = new EquatableArray<ManualDiSourceGenerator.Resolution>(new ManualDiSourceGenerator.Resolution[] { res1, res2 });
        var arr2 = new EquatableArray<ManualDiSourceGenerator.Resolution>(new ManualDiSourceGenerator.Resolution[] { res1, res2 });
        var arrDifferentOrder = new EquatableArray<ManualDiSourceGenerator.Resolution>(new ManualDiSourceGenerator.Resolution[] { res2, res1 });
        var arrDifferentElement = new EquatableArray<ManualDiSourceGenerator.Resolution>(new ManualDiSourceGenerator.Resolution[] { res1 });

        Assert.That(arr1, Is.EqualTo(arr2));
        Assert.That(arr1.GetHashCode(), Is.EqualTo(arr2.GetHashCode()));
        Assert.That(arr1, Is.Not.EqualTo(arrDifferentOrder));
        Assert.That(arr1, Is.Not.EqualTo(arrDifferentElement));
    }

    private ManualDiSourceGenerator.ClassData CreateDefaultClassData()
    {
        var constructorParams = new EquatableArray<ManualDiSourceGenerator.Resolution>(new ManualDiSourceGenerator.Resolution[]
        {
            new ManualDiSourceGenerator.ServiceResolution("String", "\"test\"", "Resolve"),
            ManualDiSourceGenerator.ContainerResolution.Instance
        });

        var injectParams = new EquatableArray<ManualDiSourceGenerator.Resolution>(new ManualDiSourceGenerator.Resolution[]
        {
            ManualDiSourceGenerator.CancellationTokenResolution.Instance
        });

        return new ManualDiSourceGenerator.ClassData(
            FileName: "TestFile",
            ClassName: "TestClass",
            Namespace: "TestNamespace",
            Accessibility: "public",
            TypeParameters: "T",
            TypeParameterConstraints: "where T : class",
            ObsoleteText: "",
            ConstructorParameters: constructorParams,
            InjectMethodParameters: injectParams,
            HasInitializeMethod: true,
            IsDisposable: false,
            BaseTypeCall: new ManualDiSourceGenerator.BaseTypeCall("BaseExtensions", "T"),
            IsSealed: true,
            BaseTypesAndInterfaces: default,
            BaseDiTypesAndInterfaces: default,
            HasBaseDi: false
        );
    }
}

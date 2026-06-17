using NSubstitute;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ManualDi.Async.Tests;

public class TestDiContainerBindingsError
{
    private class BuildException : Exception { }
    private class DisposeException : Exception { }

    [Test]
    public async Task TestBuildExceptionAndDisposeException()
    {
        var buildException = new BuildException();
        var disposeException = new DisposeException();

        var bindings = new DiContainerBindings()
            .Install(b =>
            {
                b.QueueInitialize((_) => throw buildException);
                b.QueueDispose(() => throw disposeException);
            });

        try
        {
            await bindings.Build(CancellationToken.None);
            Assert.Fail("Expected an AggregateException to be thrown.");
        }
        catch (AggregateException aggregateException)
        {
            Assert.That(aggregateException, Is.Not.Null);
            Assert.That(aggregateException!.InnerExceptions, Has.Count.EqualTo(2));
            Assert.That(aggregateException.InnerExceptions[0], Is.SameAs(buildException));
            Assert.That(aggregateException.InnerExceptions[1], Is.SameAs(disposeException));
        }
    }

    [Test]
    public async Task TestBuildExceptionNoDisposeException()
    {
        var buildException = new BuildException();
        var disposeAction = Substitute.For<Action>();

        var bindings = new DiContainerBindings()
            .Install(b =>
            {
                b.QueueInitialize((_) => throw buildException);
                b.QueueDispose(disposeAction);
            });

        try
        {
            await bindings.Build(CancellationToken.None);
            Assert.Fail("Expected a BuildException to be thrown.");
        }
        catch (BuildException thrownException)
        {
            Assert.That(thrownException, Is.SameAs(buildException));
            disposeAction.Received(1).Invoke();
        }
    }

    [Test]
    public async Task TestFailureDebugReportAttached()
    {
        var buildException = new BuildException();

        var bindings = new DiContainerBindings()
            .Install(b =>
            {
                b.QueueInitialize((_) => throw buildException);
            })
            .WithFailureDebugReport();

        try
        {
            await bindings.Build(CancellationToken.None);
            Assert.Fail("Expected a BuildException to be thrown.");
        }
        catch (BuildException thrownException)
        {
            Assert.That(thrownException, Is.SameAs(buildException));
            Assert.That(thrownException.Data.Contains(DiContainer.FailureDebugReportKey), Is.True);
            var report = thrownException.Data[DiContainer.FailureDebugReportKey] as string;
            Assert.That(report, Is.Not.Null);
        }
    }
}

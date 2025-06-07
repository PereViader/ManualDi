using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using VerifyNUnit;
using VerifyTests;

namespace ManualDi.Async.Tests;

public class TestDiContainerDebugReport
{
    [Test]
    public async Task TestDebugReport()
    {
        try
        {
            await using var container = await new DiContainerBindings()
                .Install(b =>
                {
                    b.Bind<object>()
                        .DependsOn(x => x.ConstructorDependency<int>())
                        .FromMethod(x => throw new Exception());
                    b.Bind<int>();
                })
                .WithFailureDebugReport()
                .Build(CancellationToken.None);
        }
        catch (Exception e)
        {
            var report = (string)e.Data[DiContainer.FailureDebugReportKey]!;
            await Verifier.Verify(report);
            return;
        }
        
        throw new Exception("Could not verify debug report");
    }
}
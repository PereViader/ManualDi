//dotnet run 
//dotnet run --configuration MANUALDI_SYNC
//dotnet run --configuration MANUALDI_ASYNC

#if MANUALDI_SYNC
AssertAssemblyDoesNotExists("ManualDi.Async");
var name = typeof(ManualDi.Sync.DiContainer).Assembly.FullName;
System.Console.WriteLine(name);
#elif MANUALDI_ASYNC
AssertAssemblyDoesNotExists("ManualDi.Sync");
var name = typeof(ManualDi.Async.DiContainer).Assembly.FullName;
System.Console.WriteLine(name);
#else
AssertAssemblyDoesNotExists("ManualDi.Sync");
AssertAssemblyDoesNotExists("ManualDi.Async");
System.Console.WriteLine("STANDARD");
#endif

void AssertAssemblyDoesNotExists(string assemblyName)
{
    var assembly = AppDomain.CurrentDomain.GetAssemblies()
        .FirstOrDefault(x => x.FullName is not null && x.FullName.Contains(assemblyName));
    
    if (assembly is not null)
    {
        throw new InvalidOperationException($"Assembly {assemblyName} should not exist");
    }
}

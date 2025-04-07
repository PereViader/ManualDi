#!/bin/bash
echo Copy Main folder
cp -r "ManualDi.Async/ManualDi.Async/." "ManualDi.Async.Unity3d.Package/ManualDi.Async/"

echo Delete unnecesary files from common
rm "ManualDi.Async.Unity3d.Package/ManualDi.Async/ManualDi.Async.csproj"
rm -rf "ManualDi.Async.Unity3d.Package/ManualDi.Async/bin"
rm -rf "ManualDi.Async.Unity3d.Package/ManualDi.Async/obj"
rm -rf "ManualDi.Async.Unity3d.Package/ManualDi.Async/Properties"

echo Build generator
dotnet build ManualDi.Async --configuration Release

echo Copy generator dll to package
cp "ManualDi.Async/ManualDi.Async.Generators/bin/Release/netstandard2.0/ManualDi.Async.Generators.dll" "ManualDi.Async.Unity3d.Package/ManualDi.Async.Generators.dll"
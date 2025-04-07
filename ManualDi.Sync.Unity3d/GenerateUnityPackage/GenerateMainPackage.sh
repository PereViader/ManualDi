#!/bin/bash
echo Copy Main folder
cp -r "ManualDi.Sync/ManualDi.Sync/." "ManualDi.Sync.Unity3d.Package/ManualDi.Sync/"

echo Delete unnecesary files from common
rm "ManualDi.Sync.Unity3d.Package/ManualDi.Sync/ManualDi.Sync.csproj"
rm -rf "ManualDi.Sync.Unity3d.Package/ManualDi.Sync/bin"
rm -rf "ManualDi.Sync.Unity3d.Package/ManualDi.Sync/obj"
rm -rf "ManualDi.Sync.Unity3d.Package/ManualDi.Sync/Properties"

echo Build generator
dotnet build ManualDi.Sync --configuration Release

echo Copy generator dll to package
cp "ManualDi.Sync/ManualDi.Sync.Generators/bin/Release/netstandard2.0/ManualDi.Sync.Generators.dll" "ManualDi.Sync.Unity3d.Package/ManualDi.Sync.Generators.dll"
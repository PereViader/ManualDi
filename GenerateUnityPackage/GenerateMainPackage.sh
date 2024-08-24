#!/bin/bash
echo Copy Main folder
cp -r "ManualDi.Main/ManualDi.Main/." "UnityPackageRelease/ManualDi.Main/"

echo Delete unnecesary files from common
rm "UnityPackageRelease/ManualDi.Main/ManualDi.Main.csproj"
rm -rf "UnityPackageRelease/ManualDi.Main/bin"
rm -rf "UnityPackageRelease/ManualDi.Main/obj"
rm -rf "UnityPackageRelease/ManualDi.Main/Properties"

echo Build generator
dotnet build ManualDi.Main --configuration Release

echo Copy generator dll to package
cp "ManualDi.Main/ManualDi.Main.Generators/bin/Release/netstandard2.0/ManualDi.Main.Generators.dll" "UnityPackageRelease/ManualDi.Main.Generators.dll"
cp "GenerateUnityPackage/ManualDi.Main.Generators.dll.meta" "UnityPackageRelease/ManualDi.Main.Generators.dll.meta"
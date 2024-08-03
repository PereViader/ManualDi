#!/bin/bash

echo Remove previous package
rm -rf UnityPackageRelease

echo Ensure the destination directories exist
mkdir -p "UnityPackageRelease/ManualDi.Main/"

echo Copy Main folder
cp -r "ManualDi.Main/." "UnityPackageRelease/ManualDi.Main/"

echo Copy License
cp "LICENSE.md" "UnityPackageRelease/LICENSE.md"

echo Delete unnecesary files from common
rm "UnityPackageRelease/ManualDi.Main/ManualDi.Main.csproj"
rm -rf "UnityPackageRelease/ManualDi.Main/bin"
rm -rf "UnityPackageRelease/ManualDi.Main/obj"
rm -rf "UnityPackageRelease/ManualDi.Main/Properties"

# Path to the .csproj file and package.json
CSPROJ_FILE="ManualDi.Main/ManualDi.Main.csproj"

# Extract version from the .csproj file
version=$(grep -oP '(?<=<Version>)[^<]+' "$CSPROJ_FILE")

# Check if we extracted a version
if [ -z "$version" ]; then
    echo "Version not found in $CSPROJ_FILE"
    exit 1
fi

echo "Found version $version"

echo "Copy package files to the package root"
cp "GenerateUnityPackage/package.json" "UnityPackageRelease/package.json"
cp "GenerateUnityPackage/package.json.meta" "UnityPackageRelease/package.json.meta"
sed -i "s/\"version\": \"\$version\"/\"version\": \"$version\"/g" "UnityPackageRelease/package.json"
echo "Version in package.json updated to $version"

echo Copy assambly definition
cp "GenerateUnityPackage/ManualDi.Main.asmdef" "UnityPackageRelease/ManualDi.Main/ManualDi.Main.asmdef"
cp "GenerateUnityPackage/ManualDi.Main.asmdef.meta" "UnityPackageRelease/ManualDi.Main/ManualDi.Main.asmdef.meta"

echo Build generator
dotnet build --configuration Release

echo Copy generator dll to package
cp "ManualDi.Main.Generators/bin/Release/netstandard2.0/ManualDi.Main.Generators.dll" "UnityPackageRelease/ManualDi.Main.Generators.dll"
cp "GenerateUnityPackage/ManualDi.Main.Generators.dll.meta" "UnityPackageRelease/ManualDi.Main.Generators.dll.meta"
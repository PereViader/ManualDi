#!/bin/bash

# Default values for the flags
skip_unity3d=false

# Parse command-line arguments
while [[ "$#" -gt 0 ]]; do
  case $1 in
    --skip-unity3d)
      skip_unity3d=true
      ;;
    *)
      echo "Unknown option: $1"
      exit 1
      ;;
  esac
  shift
done

# Path to the .csproj file
CSPROJ_FILE="ManualDi.Main/ManualDi.Main/ManualDi.Main.csproj"

# Extract version from the .csproj file
version=$(sed -n 's|.*<Version>\(.*\)</Version>.*|\1|p' "$CSPROJ_FILE")

# Check if we extracted a version
if [ -z "$version" ]; then
    echo "Version not found in $CSPROJ_FILE"
    exit 1
fi

echo "Remove previous package"
rm -rf UnityPackageRelease
mkdir -p "UnityPackageRelease/"

echo Copy License
cp "LICENSE.md" "UnityPackageRelease/LICENSE.md"

cp "ManualDi.Unity3d/GenerateUnityPackage/package.json" "UnityPackageRelease/package.json"
echo "Copy package files to the package root"
sed -i "s/\"version\": \"\$version\"/\"version\": \"$version\"/g" "UnityPackageRelease/package.json"
echo "Version in package.json updated to $version"

echo Copy readme
cp "ManualDi.Unity3d/GenerateUnityPackage/README.md" "UnityPackageRelease/README.md"

echo Copy assambly definition
cp "ManualDi.Unity3d/GenerateUnityPackage/ManualDi.asmdef" "UnityPackageRelease/ManualDi.asmdef"
cp "ManualDi.Unity3d/GenerateUnityPackage/ManualDi.asmdef.meta" "UnityPackageRelease/ManualDi.asmdef.meta"
cp "ManualDi.Unity3d/GenerateUnityPackage/csc.rsp" "UnityPackageRelease/csc.rsp"

sh ./ManualDi.Unity3d/GenerateUnityPackage/GenerateMainPackage.sh
if $skip_unity3d; then
    echo "Skipping unity3d package"
else
    sh ./ManualDi.Unity3d/GenerateUnityPackage/GenerateUnity3dPackage.sh
fi
sh ./ManualDi.Unity3d/GenerateUnityPackage/GenerateUnity3dMetas.sh
cd UnityPackageRelease
npm pack
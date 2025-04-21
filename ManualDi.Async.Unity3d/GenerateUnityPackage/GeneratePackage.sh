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

# Extract version from the .env file
version=$(source .env && echo $VERSION)

# Check if we extracted a version
if [ -z "$version" ]; then
    echo "Version not found in .env"
    exit 1
fi

echo "Remove previous package"
rm -rf ManualDi.Async.Unity3d.Package
mkdir -p "ManualDi.Async.Unity3d.Package/"

echo Copy License
cp "LICENSE.md" "ManualDi.Async.Unity3d.Package/LICENSE.md"

cp "ManualDi.Async.Unity3d/GenerateUnityPackage/package.json" "ManualDi.Async.Unity3d.Package/package.json"
echo "Copy package files to the package root"
sed -i "s/\"version\": \"\$version\"/\"version\": \"$version\"/g" "ManualDi.Async.Unity3d.Package/package.json"
echo "Version in package.json updated to $version"

echo Copy readme
cp "ManualDi.Async.Unity3d/GenerateUnityPackage/README.md" "ManualDi.Async.Unity3d.Package/README.md"

echo Copy assambly definition
cp "ManualDi.Async.Unity3d/GenerateUnityPackage/ManualDi.Async.asmdef" "ManualDi.Async.Unity3d.Package/ManualDi.Async.asmdef"
cp "ManualDi.Async.Unity3d/GenerateUnityPackage/ManualDi.Async.asmdef.meta" "ManualDi.Async.Unity3d.Package/ManualDi.Async.asmdef.meta"
cp "ManualDi.Async.Unity3d/GenerateUnityPackage/csc.rsp" "ManualDi.Async.Unity3d.Package/csc.rsp"
cp "ManualDi.Async.Unity3d/GenerateUnityPackage/ManualDi.Async.Generators.dll.meta" "ManualDi.Async.Unity3d.Package/ManualDi.Async.Generators.dll.meta"

sh ./ManualDi.Async.Unity3d/GenerateUnityPackage/GenerateMainPackage.sh
if $skip_unity3d; then
    echo "Skipping unity3d package"
else
    sh ./ManualDi.Async.Unity3d/GenerateUnityPackage/GenerateUnity3dPackage.sh
fi
sh ./ManualDi.Async.Unity3d/GenerateUnityPackage/GenerateUnity3dMetas.sh
cd ManualDi.Async.Unity3d.Package
npm pack
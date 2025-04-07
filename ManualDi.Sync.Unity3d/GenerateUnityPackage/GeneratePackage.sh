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
rm -rf ManualDi.Sync.Unity3d.Package
mkdir -p "ManualDi.Sync.Unity3d.Package/"

echo Copy License
cp "LICENSE.md" "ManualDi.Sync.Unity3d.Package/LICENSE.md"

cp "ManualDi.Sync.Unity3d/GenerateUnityPackage/package.json" "ManualDi.Sync.Unity3d.Package/package.json"
echo "Copy package files to the package root"
sed -i "s/\"version\": \"\$version\"/\"version\": \"$version\"/g" "ManualDi.Sync.Unity3d.Package/package.json"
echo "Version in package.json updated to $version"

echo Copy readme
cp "ManualDi.Sync.Unity3d/GenerateUnityPackage/README.md" "ManualDi.Sync.Unity3d.Package/README.md"

echo Copy assambly definition
cp "ManualDi.Sync.Unity3d/GenerateUnityPackage/ManualDi.Sync.asmdef" "ManualDi.Sync.Unity3d.Package/ManualDi.Sync.asmdef"
cp "ManualDi.Sync.Unity3d/GenerateUnityPackage/ManualDi.Sync.asmdef.meta" "ManualDi.Sync.Unity3d.Package/ManualDi.Sync.asmdef.meta"
cp "ManualDi.Sync.Unity3d/GenerateUnityPackage/csc.rsp" "ManualDi.Sync.Unity3d.Package/csc.rsp"
cp "ManualDi.Sync.Unity3d/GenerateUnityPackage/ManualDi.Sync.Generators.dll.meta" "ManualDi.Sync.Unity3d.Package/ManualDi.Sync.Generators.dll.meta"

sh ./ManualDi.Sync.Unity3d/GenerateUnityPackage/GenerateMainPackage.sh
if $skip_unity3d; then
    echo "Skipping unity3d package"
else
    sh ./ManualDi.Sync.Unity3d/GenerateUnityPackage/GenerateUnity3dPackage.sh
fi
sh ./ManualDi.Sync.Unity3d/GenerateUnityPackage/GenerateUnity3dMetas.sh
cd ManualDi.Sync.Unity3d.Package
npm pack
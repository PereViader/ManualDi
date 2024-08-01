#!/bin/bash

sh ./GenerateUnityPackage/GenerateUnityPackageReleaseFolder.sh
sh ./GenerateUnityPackage/GenerateUnity3dMetas.sh
cd UnityPackageRelease
npm pack
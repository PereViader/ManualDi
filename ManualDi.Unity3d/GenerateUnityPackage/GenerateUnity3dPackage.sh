#!/bin/bash
cp -r "ManualDi.Unity3d/Assets/ManualDi.Unity3d/." "UnityPackageRelease/ManualDi.Unity3d/"
mv "UnityPackageRelease/ManualDi.Unity3d/Samples" "UnityPackageRelease/ManualDi.Unity3d/Samples~"
rm "UnityPackageRelease/ManualDi.Unity3d/Samples.meta"
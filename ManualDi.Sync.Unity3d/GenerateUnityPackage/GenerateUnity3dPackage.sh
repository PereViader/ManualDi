#!/bin/bash
cp -r "ManualDi.Sync.Unity3d/Assets/ManualDi.Sync.Unity3d/." "ManualDi.Sync.Unity3d.Package/ManualDi.Sync.Unity3d/"
mv "ManualDi.Sync.Unity3d.Package/ManualDi.Sync.Unity3d/Samples" "ManualDi.Sync.Unity3d.Package/ManualDi.Sync.Unity3d/Samples~"
rm "ManualDi.Sync.Unity3d.Package/ManualDi.Sync.Unity3d/Samples.meta"
rm -rf "ManualDi.Sync.Unity3d.Package/ManualDi.Sync.Unity3d/Tests"
rm "ManualDi.Sync.Unity3d.Package/ManualDi.Sync.Unity3d/Tests.meta"
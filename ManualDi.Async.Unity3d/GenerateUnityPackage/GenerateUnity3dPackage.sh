#!/bin/bash
cp -r "ManualDi.Async.Unity3d/Assets/ManualDi.Async.Unity3d/." "ManualDi.Async.Unity3d.Package/ManualDi.Async.Unity3d/"
mv "ManualDi.Async.Unity3d.Package/ManualDi.Async.Unity3d/Samples" "ManualDi.Async.Unity3d.Package/ManualDi.Async.Unity3d/Samples~"
rm "ManualDi.Async.Unity3d.Package/ManualDi.Async.Unity3d/Samples.meta"
rm -rf "ManualDi.Async.Unity3d.Package/ManualDi.Async.Unity3d/Tests"
rm "ManualDi.Async.Unity3d.Package/ManualDi.Async.Unity3d/Tests.meta"
#!/bin/bash
set -euo pipefail

bash ./ManualDi.Sync.Unity3d/GenerateUnityPackage/GeneratePackage.sh --skip-unity3d
bash ./ManualDi.Async.Unity3d/GenerateUnityPackage/GeneratePackage.sh --skip-unity3d

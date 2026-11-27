#!/bin/bash
set -euo pipefail

cd "$(dirname "$0")"
mv TestsSourceGenerator.Generated.received.txt TestsSourceGenerator.Generated.verified.txt

#!/bin/bash
set -e
cd "$(dirname "$0")"
mv TestsSourceGenerator.Generated.received.txt TestsSourceGenerator.Generated.verified.txt
rm -f TestsSourceGenerator.Generated.received

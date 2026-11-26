#!/bin/bash
set -euo pipefail

mv TestsSourceGenerator.Generated.received.txt TestsSourceGenerator.Generated.verified.txt
rm TestsSourceGenerator.Generated.received

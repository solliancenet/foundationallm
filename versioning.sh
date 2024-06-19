#!/bin/bash

# Determine the branch name
BRANCH_NAME=$(git rev-parse --abbrev-ref HEAD)

# Extract version from branch name (format: release/1.0.0)
if [[ "$BRANCH_NAME" =~ ^release/([0-9]+\.[0-9]+\.[0-9]+)$ ]]; then
  VERSION="${BASH_REMATCH[1]}"
else
  VERSION="0.0.0"
fi

echo "##[set-output name=version;]$VERSION"
echo "Version: $VERSION"

# Replace the version placeholder in the .csproj files
for csproj in $(find . -name '*.csproj'); do
  sed -i "s/<Version>0.0.0<\/Version>/<Version>$VERSION<\/Version>/" "$csproj"
done

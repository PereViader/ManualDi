#!/bin/bash

# Generate a lowercase GUID in the Unity format
generate_guid() {
    # Try using uuidgen, if it fails use PowerShell
    (uuidgen 2>/dev/null || powershell -Command "[guid]::NewGuid().ToString()") | tr '[:upper:]' '[:lower:]' | sed 's/-//g'
}

# Function to generate a .meta file for .cs files
generate_cs_meta() {
    local path="$1"
    local guid=$(generate_guid)
    cat <<EOF >"$path.meta"
fileFormatVersion: 2
guid: $guid
MonoImporter:
  externalObjects: {}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {instanceID: 0}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
EOF
}

# Function to generate a .meta file for other files (non-.cs)
generate_text_meta() {
    local path="$1"
    local guid=$(generate_guid)
    cat <<EOF >"$path.meta"
fileFormatVersion: 2
guid: $guid
TextScriptImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
EOF
}

# Function to generate a .meta file for folders
generate_folder_meta() {
    local path="$1"
    local guid=$(generate_guid)
    cat <<EOF >"$path.meta"
fileFormatVersion: 2
guid: $guid
folderAsset: yes
DefaultImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
EOF
}

# Traverse the directory structure and generate .meta files
traverse_and_generate() {
    local dir="$1"
    for item in "$dir"/*; do
        # Skip if the item is a .meta file
        case "$item" in
            *.meta) continue ;;
        esac

        # Check if a .meta file already exists, skip if it does
        if [ -f "$item.meta" ]; then
            continue
        fi

        # Check for directories and skip those ending with ~
        if [ -d "$item" ]; then
            case "$item" in
                *~) continue ;;
            esac
            echo "Generating .meta for directory: $item"
            generate_folder_meta "$item"
            traverse_and_generate "$item"  # Recursive call for subdirectories
            continue
        fi

        # Generate .meta for .cs files
        case "$item" in
            *.cs)
                echo "Generating .meta for script: $item"
                generate_cs_meta "$item"
                continue
                ;;
        esac

        # Generate .meta for non-.cs files
        echo "Generating .meta for other file: $item"
        generate_text_meta "$item"  
    done
}

echo "Start generating metas"
traverse_and_generate "UnityPackageRelease"
echo "Meta files generation complete."
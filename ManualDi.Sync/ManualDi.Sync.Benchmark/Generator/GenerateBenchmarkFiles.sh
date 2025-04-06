#!/bin/bash

# Original file name and class name
original_file="ClassX.cs"
original_class="ClassX"

# Check if the original file exists
if [ ! -f "$original_file" ]; then
    echo "File $original_file not found!"
    exit 1
fi

# Loop to create files from Class1 to Class5000
for i in {1..2000}
do
    # Define new file name and class name
    new_file="Class$i.cs"
    new_class="Class$i"

    # Copy the original file to the new file
    cp "$original_file" "$new_file"

    # Replace the class name inside the new file
    sed -i "s/$original_class/$new_class/g" "$new_file"

    echo "Created $new_file with class name $new_class"
done

echo "All files have been created!"

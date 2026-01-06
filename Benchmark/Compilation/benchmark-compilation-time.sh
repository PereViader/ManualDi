#!/bin/bash

# Configuration
CSV_FILE="compilation-times.csv"
PNG_FILE="compilation-times.png"
COUNTS=(500 0)
ITERATIONS=1

# Helper function to measure build time
measure_build_time() {
    local project_file="$1"
    local build_args="$2"
    local total_time=0

    for ((j=1; j<=ITERATIONS; j++)); do
        # Clean only build artifacts
        dotnet clean "$project_file" --verbosity quiet --nologo > /dev/null

        # Measure start time
        start=$(date +%s%3N 2>/dev/null)
        if [ -z "$start" ] || [ "$start" == "%s%3N" ]; then
            start=$(date +%s)000
        fi

        # Build
        dotnet build "$project_file" $build_args --no-incremental --nologo --verbosity quiet > /dev/null

        # Measure end time
        end=$(date +%s%3N 2>/dev/null)
        if [ -z "$end" ] || [ "$end" == "%s%3N" ]; then
            end=$(date +%s)000
        fi

        duration=$((end - start))
        total_time=$((total_time + duration))
    done

    # Return average
    echo $((total_time / ITERATIONS))
}

# Helper function to generate classes
generate_classes() {
    local project_type="$1"
    local count="$2"
    local generated_dir="Project/Generated"

    # Clear previous
    if [ -d "$generated_dir" ]; then
        rm -rf "$generated_dir"
    fi
    mkdir -p "$generated_dir"

    # Generate
    if [ "$count" -gt 0 ]; then
        echo "Generating $count classes for $project_type..."
        for ((i=0; i<count; i++)); do
            echo "namespace ManualDi.$project_type.Benchmark.Generated { public class Class$i {} }" > "$generated_dir/Class$i.cs"
        done
    fi
}

echo "======================================"
echo "Starting Benchmark Suite"
echo "======================================"

# Initialize CSV
echo "Classes,Baseline,ManualDi.Sync,ManualDi.Async" > "$CSV_FILE"

for count in "${COUNTS[@]}"; do
    echo "--------------------------------------------------"
    echo "Processing for $count classes..."

    PROJECT_FILE="Project/Project.csproj"
    
    generate_classes "Sync" "$count"
    
    echo "  [Sync] Measuring Standard Build..."
    std=$(measure_build_time "$PROJECT_FILE" "")
    echo "    -> Avg: ${std} ms"

    echo "  [Sync] Measuring ManualDi.Sync Build..."
    sync=$(measure_build_time "$PROJECT_FILE" "--configuration MANUALDI_SYNC")
    echo "    -> Avg: ${sync} ms"

    echo "  [Async] Measuring ManualDi.Async Build..."
    async=$(measure_build_time "$PROJECT_FILE" "--configuration MANUALDI_ASYNC")
    echo "    -> Avg: ${async} ms"

    # Log to CSV
    echo "$count,$std,$sync,$async" >> "$CSV_FILE"
done

echo "======================================"
echo "Benchmark complete. Results saved to $CSV_FILE"
echo "Generating graph..."
python "plot-compilation-times.py" -i "$CSV_FILE" -o "$PNG_FILE"
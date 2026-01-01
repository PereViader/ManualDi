import pandas as pd
import matplotlib.pyplot as plt
import argparse
import os

def generate_graph(csv_path, output_path):
    """
    Generates a line graph from compilation time data.

    Args:
        csv_path (str): The path to the input CSV file.
        output_path (str): The path to save the output PNG image.
    """
    # --- 1. Data Loading and Validation ---
    if not os.path.exists(csv_path):
        print(f"Error: Input file not found at '{csv_path}'")
        return

    try:
        df = pd.read_csv(csv_path)
        required_cols = {'Classes', 'Baseline', 'ManualDi.Sync', 'ManualDi.Async'}
        if not required_cols.issubset(df.columns):
            print(f"Error: CSV file must contain columns: {required_cols}")
            return
    except Exception as e:
        print(f"Error reading or parsing the CSV file: {e}")
        return

    # --- 2. Plotting Logic ---
    plt.figure(figsize=(10, 6))

    # Define styles for the 3 lines
    lines = [
        {'col': 'ManualDi.Sync',  'label': 'Sync ManualDi',    'color': 'tab:blue',   'marker': 'o'},
        {'col': 'ManualDi.Async', 'label': 'Async ManualDi',   'color': 'tab:red',    'marker': '^'},
        {'col': 'Baseline',       'label': 'Baseline (No DI)', 'color': 'tab:gray',   'marker': 'x'}
    ]

    for line_cfg in lines:
        plt.plot(df['Classes'], df[line_cfg['col']], 
                 marker=line_cfg['marker'], 
                 linestyle='-', 
                 color=line_cfg['color'], 
                 linewidth=2, 
                 markersize=8,
                 label=line_cfg['label'])
        
        # Add labels for each point
        for i, row in df.iterrows():
            plt.annotate(f"{int(row[line_cfg['col']])}", 
                         (row['Classes'], row[line_cfg['col']]),
                         fontsize=9, ha='left', va='bottom', 
                         textcoords="offset points", xytext=(5, 5),
                         color=line_cfg['color'])

    # --- 3. Customization ---
    plt.title('Compilation Time vs Number of Classes', fontsize=16)
    plt.xlabel('Number of Generated Classes', fontsize=12)
    plt.ylabel('Compilation Time (ms)', fontsize=12)
    plt.grid(True, linestyle='--', alpha=0.7)
    plt.legend()

    plt.tight_layout()

    # --- 4. Save the Output ---
    try:
        plt.savefig(output_path, bbox_inches='tight')
        print(f"Graph successfully generated and saved to '{output_path}'")
    except Exception as e:
        print(f"Error saving the graph: {e}")


def main():
    parser = argparse.ArgumentParser(
        description="Generate a compilation time graph from a CSV file."
    )
    parser.add_argument(
        '-i', '--input',
        type=str,
        default='compilation-times.csv',
        help="Path to the input CSV file."
    )
    parser.add_argument(
        '-o', '--output',
        type=str,
        default='compilation-times.png',
        help="Path for the output PNG image."
    )

    args = parser.parse_args()
    generate_graph(args.input, args.output)

if __name__ == "__main__":
    main()

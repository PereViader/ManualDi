import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
import argparse
import os

def generate_graph(csv_path, output_path):
    """
    Generates a grouped bar chart from DI container performance data.

    Args:
        csv_path (str): The path to the input CSV file.
        output_path (str): The path to save the output PNG image.
    """
    # --- 1. Data Loading and Validation ---
    # Check if the input file exists
    if not os.path.exists(csv_path):
        print(f"Error: Input file not found at '{csv_path}'")
        return

    # Read the data from the CSV file
    try:
        df = pd.read_csv(csv_path)
        # Basic validation for required columns
        if not {'Container', 'Time', 'GC'}.issubset(df.columns):
            print("Error: CSV file must contain 'Container', 'Time', and 'GC' columns.")
            return
    except Exception as e:
        print(f"Error reading or parsing the CSV file: {e}")
        return

    # --- 2. Plotting Logic ---
    barWidth = 0.35
    fig, ax1 = plt.subplots(figsize=(12, 8))

    # Set position of bar on X axis
    r1 = np.arange(len(df['Container']))
    r2 = [x + barWidth for x in r1]

    # Plot 'Time' bars on the primary y-axis (ax1)
    ax1.bar(r1, df['Time'], color='tab:blue', width=barWidth, edgecolor='grey', label='Time')
    ax1.set_ylabel('Time', color='tab:blue', fontsize=12)
    ax1.tick_params(axis='y', labelcolor='tab:blue')

    # Create a secondary y-axis (ax2) for 'GC' bars
    ax2 = ax1.twinx()
    ax2.bar(r2, df['GC'], color='tab:red', width=barWidth, edgecolor='grey', label='GC')
    ax2.set_ylabel('GC', color='tab:red', fontsize=12)
    ax2.tick_params(axis='y', labelcolor='tab:red')

    # --- 3. Final Chart Customization ---
    # Configure the x-axis labels and ticks
    plt.xticks([r + barWidth / 2 for r in range(len(df['Container']))], df['Container'], rotation=45, ha="right")

    # Add titles and a unified legend
    plt.title('Performance Comparison of DI Containers', fontsize=16)
    handles1, labels1 = ax1.get_legend_handles_labels()
    handles2, labels2 = ax2.get_legend_handles_labels()
    fig.legend(handles1 + handles2, labels1 + labels2, loc="upper left", bbox_to_anchor=(0.1, 0.9))

    # Adjust layout to prevent labels from being cut off
    fig.tight_layout()

    # --- 4. Save the Output ---
    try:
        plt.savefig(output_path, bbox_inches='tight')
        print(f"Graph successfully generated and saved to '{output_path}'")
    except Exception as e:
        print(f"Error saving the graph: {e}")


def main():
    """
    Main function to parse arguments and run the graph generator.
    """
    parser = argparse.ArgumentParser(
        description="Generate a performance comparison graph from a CSV file.",
        formatter_class=argparse.RawTextHelpFormatter
    )
    parser.add_argument(
        '-i', '--input',
        type=str,
        required=True,
        help="Path to the input CSV file.\nThe CSV must contain 'Container', 'Time', and 'GC' columns."
    )
    parser.add_argument(
        '-o', '--output',
        type=str,
        default='di_performance_comparison.png',
        help="Path for the output PNG image (default: di_performance_comparison.png)."
    )

    args = parser.parse_args()
    generate_graph(args.input, args.output)


if __name__ == "__main__":
    main()
# SudokuApp.Check

## Overview

This utility stress-tests and benchmarks the speed of different puzzle generation algorithms. To guarantee pure performance metrics, the tool runs entirely silent by default. It automatically validates every board layout during creation, instantly halting and throwing an error only if a generator produces a broken puzzle structure.

## How to Build, Run, and Test

### 1. Build the Solution
Run from the root solution directory:
```bash
dotnet build
```

### 2. Run the SudokuApp.Check
Execute the console application:
```bash
dotnet run --project SudokuApp.Check PuzzleGenerator 100
```

#### Arguments

* **`PuzzleGenerator`**: The name of the specific generator class to benchmark.
* **`100`**: The total number of puzzle boards to generate during the test run.

## Supported Generators

The benchmark dynamically loads implementations from the core assembly. You can swap the generator class name argument to test different algorithms:
* `PuzzleGenerator`: generates valid grids row-by-row.
* `SudokuGenerator`: generates valid grids by populating diagonal 3x3 subgrids first.

## Performance Testing

To eliminate framework overhead and average out system variance, compile the application once and execute a 5-run sample loop using the native `time` utility.

```bash
GEN="PuzzleGenerator" COUNT=10000 && \
dotnet build && \
echo "=== Testing: $GEN ($COUNT) ===" && \
seq 5 | xargs -I {} time -f "[{}] Time Elapsed: %E | CPU: %P" \
dotnet SudokuApp.Check/bin/Debug/net10.0/SudokuApp.Check.dll "$GEN" $COUNT
```

### Configuration Details
* **`GEN="PuzzleGenerator"`**: Set your target generator class name here.
* **`COUNT=10000`**: Set total number of puzzle boards generated per simulation loop.

### Benchmark Output Format
The pipeline runs silently and outputs tracking logs in the following layout:
```text
[1] Time Elapsed: 0:00.42 | CPU: 92%
[2] Time Elapsed: 0:00.39 | CPU: 95%
...
```
*If a board fails validation, the loop will break instantly with a runtime error trace.*

### Benchmark Results

```bash
=== Testing: PuzzleGenerator (10000) ===
[1] Time Elapsed: 0:02.33 | CPU: 104%
[2] Time Elapsed: 0:01.41 | CPU: 173%
[3] Time Elapsed: 0:02.38 | CPU: 106%
[4] Time Elapsed: 0:02.34 | CPU: 104%
[5] Time Elapsed: 0:02.34 | CPU: 104%

=== Testing: SudokuGenerator (10000) ===
[1] Time Elapsed: 0:09.91 | CPU: 110%
[2] Time Elapsed: 0:11.59 | CPU: 96%
[3] Time Elapsed: 0:11.48 | CPU: 96%
[4] Time Elapsed: 0:09.62 | CPU: 110%
[5] Time Elapsed: 0:11.36 | CPU: 96%
```
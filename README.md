# Sudoku Command-Line Application & Test Suite

An interactive, console-based 9x9 Sudoku game and comprehensive unit testing suite written in C# (.NET 10.0). The application enables players to generate Sudoku puzzles, interact via CLI commands, request hints, check for rule violations, undo last commands, and quit or replay upon completion.

---

## System Architecture & Design Overview

The solution (`Sudoku.slnx`) contains three projects:
1. **`SudokuApp`**: Main console application.
2. **`SudokuApp.Tests`**: Unit testing suite using **xUnit** and **Microsoft.NET.Test.Sdk**.
3. **`SudokuApp.Check`**: Puzzle generator speed benchmark utility.

### Core Application Components (`SudokuApp`)
* **`Program.cs`**: Application entry point managing the startup grid size selection, game loop, input handling, board rendering, and replay prompts.
* **`SudokuGame.cs`**: High-level game controller managing board state, processing user cell commands (`PlaceValue`, `Clear`), undo last cell commands, checking solution completion (`IsSolved`), and identifying hint cells.
* **`Board.cs` & `Cell.cs`**: Domain model for the 9x9 grid. Each `Cell` tracks row/column indices, solution digit (`Value`), pre-filled clue status (`IsFixed`), and player entry (`UserValue`). `Board` handles duplicate validation across rows, columns, and 3x3 subgrids.
* **`PuzzleGenerator.cs`**: Defines `IPuzzleGenerator` interface and `PuzzleGenerator` class, which generates full valid Sudoku grids row-by-row using row backtracking (non-deterministic) and delegates clue selection to an `IClueStrategy`.
* **`SudokuGenerator.cs`**: Implements `IPuzzleGenerator`. Generates full valid Sudoku grids by populating diagonal 3x3 subgrids first, recursively solving remaining cells via backtracking (`FillRemaining`), and delegating clue selection to an `IClueStrategy`.
* **`ClueStrategy.cs`**: Strategy interface for clue selection: 
    * `UniqueClueStrategy` *(Default)*: Performs in-place clue removal with a solution counter to guarantee unique solution.
    * `RandomClueStrategy`: Picks random clue positions without solver verification.
* **`SudokuCommandParser.cs`**: Uses regular expressions to parse CLI commands into strongly typed `SudokuCommand` records (`PlaceValue`, `Clear`, `Undo`, `Hint`, `Check`, `Quit`, `NewGame`, `Invalid`).
* **`BoardRenderer.cs`**: ASCII console renderer displaying a formatted 9x9 grid with ANSI color formatting for user-entered digits.

### Unit Test Project (`SudokuApp.Tests`)
* **`CellTests.cs`**: Validates cell state mutations (`TrySetUserValue`), fixed vs. editable logic, value clearing, and out-of-range bounds checks.
* **`BoardTests.cs`**: Tests value placement rules, violation detection for rows, columns, and subgrids (`CheckCell`), board-wide validation (`CheckBoard`), and completion detection (`IsSolved`).
* **`SudokuGameTests.cs`**: Tests hint retrieval logic (`GetHintCell`), cell command processing (`ProcessCellCommand`), undo command, and board violation reporting.
* **`SudokuGeneratorTests.cs`**: Verifies generation constraints (81 cells total, exactly 30 pre-filled fixed cells), solution validity across all rows/cols/boxes, and reproducibility via random seed initialization.
* **`PuzzleGeneratorTests.cs`**: Verifies generation of the original `PuzzleGenerator` code.
* **`SudokuCommandParserTests.cs`**: Validates parsing for `quit`, `hint`, `check`, coordinate conversion (e.g., `B3 7` -> row 1, col 2, value 7), `clear`, and malformed/invalid inputs.
* **`BoardRendererTests.cs`**: Validates ASCII board header and row output, user-entry ANSI formatting, and blank cell rendering (`_`).

### Benchmark Project (`SudokuApp.Check`)
* **`Program.cs`**: Standalone entry point for stress-testing and benchmarking puzzle generation algorithms.
* **Silent Execution**: Runs silently by default to eliminate console I/O overhead and measure algorithm speed.
* **Automatic Layout Validation**: Instantly validates every generated board during creation, throwing an error if a broken layout is produced.

---

## Alternative Puzzle generator strategy

Modify code inside `Program.cs` to switch puzzle generator.

Before:

```csharp
IPuzzleGenerator puzzleGenerator = new SudokuGenerator();
```

After:

```csharp
IPuzzleGenerator puzzleGenerator = new PuzzleGenerator();
```

---

## Design Assumptions & Key Implementation Notes

1. **Puzzle Generation & Error Handling**:
   * The generation algorithm is based on standard Sudoku subgrid filling and backtracking logic (adapted from GeeksforGeeks) with custom adjustments for blank editable cells.
   * Generation includes checks to ensure recursive backtracking (`FillRemaining`) completes successfully; if generation fails, an exception is thrown to guarantee broken grids containing zero-valued cells are never returned.
2. **Uniqueness Guarantee**:
   * Clue selection is decoupled via the `IClueStrategy` interface: 
      * `RandomClueStrategy`: Randomly selects fixed clues from a completed grid ([**does not guarantee a unique solution**](#example-of-non-unique-solution-in-4x4-board).
      * `UniqueClueStrategy` *(Default)*: Solves uniqueness by restoring a clue whenever removing it leads to multiple valid completions.

3. **Hint Rule**:
   * The `hint` command returns a random unresolved editable cell (a cell that is empty or currently holds an incorrect user digit) along with its correct solution value. When all editable cells are correctly solved, `GetHintCell()` returns `null` because the puzzle is already completed.
4. **Quit Command**:
   * A dedicated `quit` command allows players to terminate the game session at any command prompt.
5. **Check Command Behavior**:
   * Running the `check` command evaluates all user-entered cells across the grid in sequential order and reports the **first violation found** (row, column, or subgrid duplicate). It does **not** check or restrict reporting specifically to the user's most recent move.
6. **Grid Size Selection & New Game Command**:
   * **Startup Prompt**: Upon launching the application, players are prompted to select a grid size (`4` for 4x4 or `9` for 9x9, defaulting to `9` if Enter is pressed).
   * **Mid-Game Resets**: Players can type `new` at any prompt to generate a new puzzle using the current size, or `new 4` / `new 9` to dynamically switch sizes mid-session.

---

## Environment Requirements

* **Supported Operating Systems**: Windows, Linux, or macOS.
* **Runtime / SDK**: .NET 10.0 SDK (or .NET 8.0 / 9.0 by adjusting `<TargetFramework>` in `.csproj` files).
* **Terminal**: Any command-line terminal with ANSI color escape code support (e.g., Windows Terminal, PowerShell, bash, zsh).

---

## How to Build, Run, and Test

### 1. Build the Solution
Run from the root solution directory:
```bash
dotnet build
```

### 2. Run the Sudoku Game
Execute the console application:
```bash
dotnet run --project SudokuApp
```

### 3. Run the Unit Test Suite
Execute all xUnit test cases:
```bash
dotnet test
```

---

## How to Play (Command Summary)

| Command Pattern | Example | Description |
| :--- | :--- | :--- |
| `[Row][Col] [Digit]` | `A3 4` | Places digit `4` in Row `A`, Column `3`. |
| `[Row][Col] clear` | `C5 clear` | Clears user-entered digit in Row `C`, Column `5`. |
| `undo` | `undo` | Revert last place value and clear cell. |
| `hint` | `hint` | Reveals the solution value for a random unresolved cell. |
| `check` | `check` | Scans the board and reports the **first rule violation found** (evaluates all user entries in grid order, not just the most recent move). |
| `new` / `new [4/9]` | `new 4` | Starts a new game (retains current size if omitted, or sets to 4x4 / 9x9) |
| `quit` | `quit` | Exits the game. |

---

## Example Console Session

```text
Welcome to Sudoku
Select grid size — 4 (4x4) or 9 (9x9) [Default: 9]:

Here is your puzzle:
    1 2 3 4 5 6 7 8 9
  A 5 3 _ _ 7 _ _ _ _
  B 6 _ _ 1 9 5 _ _ _
  C _ 9 8 _ _ _ _ 6 _
  D 8 _ _ _ 6 _ _ _ 3
  E 4 _ _ 8 _ 3 _ _ 1
  F 7 _ _ _ 2 _ _ _ 6
  G _ 6 _ _ _ _ 2 8 _
  H _ _ _ 4 1 9 _ _ 5
  I _ _ _ _ 8 _ _ 7 9

Enter command (e.g., A3 4, C5 clear, undo, hint, check, quit):
Or start new game (e.g., new, new 4, new 9):
A3 4

Move accepted.

Current grid:
    1 2 3 4 5 6 7 8 9
  A 5 3 4 _ 7 _ _ _ _
  B 6 _ _ 1 9 5 _ _ _
  ...
```

---

## Example of Non Unique solution in 4x4 board

The generator might generate multiple valid solutions when clues are randomly selected.

```ps1
Solution:           Current board:
    𝟷  𝟸  𝟹  𝟺          𝟷  𝟸  𝟹  𝟺 
 A  4̳  1̳  𝟸  𝟹       A  4̳  1̳  𝟸  𝟹 
 B  𝟹  𝟸  1̳  𝟺       B  𝟹  𝟸  1̳  𝟺 
 C  2̳  3̳  4̳  1̳       C  2̳  4̳  3̳  1̳ 
 D  𝟷  4̳  3̳  𝟸       D  𝟷  3̳  4̳  𝟸 
```

The game only ends when it matches the solution.

```
Enter command (e.g., A3 4, C5 clear, hint, check, quit):
Or start new game (e.g., new, new 4, new 9):
check
No rule violations detected.

Current grid:
    𝟷  𝟸  𝟹  𝟺 
 A  4̳  1̳  𝟸  𝟹 
 B  𝟹  𝟸  1̳  𝟺 
 C  2̳  4̳  3̳  1̳ 
 D  𝟷  3̳  4̳  𝟸 
```
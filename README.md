# Sudoku Command-Line Application & Test Suite

An interactive, console-based 9x9 Sudoku game and comprehensive unit testing suite written in C# (.NET 10.0). The application enables players to generate Sudoku puzzles, interact via CLI commands, request hints, check for rule violations, and quit or replay upon completion.

---

## System Architecture & Design Overview

The solution (`Sudoku.slnx`) contains two projects:
1. **`SudokuApp`**: Main console application.
2. **`SudokuApp.Tests`**: Unit testing suite using **xUnit** and **Microsoft.NET.Test.Sdk**.

### Core Application Components (`SudokuApp`)
* **`Program.cs`**: Application entry point managing the game loop, input handling, board rendering, and replay prompts.
* **`SudokuGame.cs`**: High-level game controller managing board state, processing user cell commands (`PlaceValue`, `Clear`), checking solution completion (`IsSolved`), and identifying hint cells.
* **`Board.cs` & `Cell.cs`**: Domain model for the 9x9 grid. Each `Cell` tracks row/column indices, solution digit (`Value`), pre-filled clue status (`IsFixed`), and player entry (`UserValue`). `Board` handles duplicate validation across rows, columns, and 3x3 subgrids.
* **`SudokuGenerator.cs`**: Generates full valid Sudoku grids by populating diagonal 3x3 subgrids first, recursively solving remaining cells via backtracking (`FillRemaining`), and selecting pre-filled clue positions.
* **`SudokuCommandParser.cs`**: Uses regular expressions to parse CLI commands into strongly typed `SudokuCommand` records (`PlaceValue`, `Clear`, `Hint`, `Check`, `Quit`, `Invalid`).
* **`PuzzleGenerator.cs`**: Original code to generate full valid Sudoku grids row-by-row using row backtracking, but is non-deterministic.
* **`BoardRenderer.cs`**: ASCII console renderer displaying a formatted 9x9 grid with ANSI color formatting for user-entered digits.

### Unit Test Project (`SudokuApp.Tests`)
* **`CellTests.cs`**: Validates cell state mutations (`TrySetUserValue`), fixed vs. editable logic, value clearing, and out-of-range bounds checks.
* **`BoardTests.cs`**: Tests value placement rules, violation detection for rows, columns, and subgrids (`CheckCell`), board-wide validation (`CheckBoard`), and completion detection (`IsSolved`).
* **`SudokuGameTests.cs`**: Tests hint retrieval logic (`GetHintCell`), cell command processing (`ProcessCellCommand`), and board violation reporting.
* **`SudokuGeneratorTests.cs`**: Verifies generation constraints (81 cells total, exactly 30 pre-filled fixed cells), solution validity across all rows/cols/boxes, and reproducibility via random seed initialization.
* **`PuzzleGeneratorTests.cs`**: Verifies generation of the original `PuzzleGenerator` code.
* **`SudokuCommandParserTests.cs`**: Validates parsing for `quit`, `hint`, `check`, coordinate conversion (e.g., `B3 7` -> row 1, col 2, value 7), `clear`, and malformed/invalid inputs.
* **`BoardRendererTests.cs`**: Validates ASCII board header and row output, user-entry ANSI formatting, and blank cell rendering (`_`).

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
2. **Uniqueness Guarantee Limitation**:
   * Fixed clue positions are randomly selected from a valid completed solution grid. Because clue selection does not currently run a solver check for multiple valid completions, generated puzzles **do not guarantee a unique solution**.
3. **Hint Rule**:
   * The `hint` command returns a random unresolved editable cell (a cell that is empty or currently holds an incorrect user digit) along with its correct solution value. When all editable cells are correctly solved, `GetHintCell()` returns `null` because the puzzle is already completed.
4. **Quit Command**:
   * A dedicated `quit` command allows players to terminate the game session at any command prompt.
5. **Check Command Behavior**:
   * Running the `check` command evaluates all user-entered cells across the grid in sequential order and reports the **first violation found** (row, column, or subgrid duplicate). It does **not** check or restrict reporting specifically to the user's most recent move.

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
| `hint` | `hint` | Reveals the solution value for a random unresolved cell. |
| `check` | `check` | Scans the board and reports the **first rule violation found** (evaluates all user entries in grid order, not just the most recent move). |
| `quit` | `quit` | Exits the game. |

---

## Example Console Session

```text
Welcome to Sudoku

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

Enter command (e.g., A3 4, C5 clear, hint, check, quit):
A3 4

Move accepted.

Current grid:
    1 2 3 4 5 6 7 8 9
  A 5 3 4 _ 7 _ _ _ _
  B 6 _ _ 1 9 5 _ _ _
  ...
```

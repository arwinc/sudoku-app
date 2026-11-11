using System.Diagnostics;

public interface IPuzzleGenerator
{
  Board Generate();
}

public class PuzzleGenerator : IPuzzleGenerator
{
  private readonly int _boardSize;
  private readonly int _boxSize;
  private readonly int _preFilledCellCount;

  private const int MAX_ROW_ATTEMPTS = 5;
  private readonly List<int[]> generatedNumbers = [];
  private readonly IClueStrategy _clueStrategy;
  private readonly Random _random;

  public PuzzleGenerator(
    int boardSize = 9,
    int? preFilledCellCount = null,
    IClueStrategy? clueStrategy = null,
    Random? random = null
  )
  {
    _boardSize = boardSize;
    _boxSize = (int)Math.Sqrt(boardSize);
    _preFilledCellCount = preFilledCellCount ?? (boardSize == 4 ? 7 : 30);
    _clueStrategy = clueStrategy ?? new UniqueClueStrategy();
    _random = random ?? Random.Shared;
  }

  public PuzzleGenerator(Random? random) : this(9, 30, null, random) { }

  public Board Generate()
  {
    generatedNumbers.Clear();

    int totalAttempts = 0;
    int row = 0;

    // Loop until all 9 rows are successfully generated
    while (row < _boardSize)
    {
      (int[]? shuffledRow, bool success, int attempts) = TryGenerateRow(row);

      totalAttempts += attempts;

      if (success && shuffledRow != null)
      {
        if (row < generatedNumbers.Count)
        {
          generatedNumbers[row] = shuffledRow;
        }
        else
        {
          generatedNumbers.Add(shuffledRow);
        }
        row++; // Move to next row
      }
      else
      {
        // BACKTRACK: Remove previous row and step back 1 row
        if (generatedNumbers.Count > 0)
        {
          generatedNumbers.RemoveAt(generatedNumbers.Count - 1);
        }
        row = Math.Max(0, row - 1);
      }
    }

    Debug.WriteLine($">>> Total attempts: {totalAttempts}x");

    HashSet<int> fixedPositions = GetRandomFixedPositions();

    List<Cell> cells = [..generatedNumbers
      .SelectMany((usedValue, rowIndex) => usedValue.Select((value, colIndex) => {
        int position = rowIndex * _boardSize + colIndex;
        bool isFixed = fixedPositions.Contains(position);
        return new Cell(rowIndex, colIndex, value, isFixed);
      }
    ))];

    return new Board(cells);
  }

  private HashSet<int> GetRandomFixedPositions()
  {
    // Create a working grid copy initialized with full solution
    int size = _boardSize;
    int[,] grid = new int[size, size];
    for (int r = 0; r < size; r++)
    {
      for (int c = 0; c < size; c++)
      {
        grid[r, c] = generatedNumbers[r][c];
      }
    }

    return _clueStrategy.SelectClues(grid, _preFilledCellCount, _random);
  }

  private (int[]?, bool, int) TryGenerateRow(int row)
  {
    int attempts = 0;
    List<int> availableNumbers = new(_boardSize);

    while (attempts < MAX_ROW_ATTEMPTS)
    {
      attempts++;
      int[] currentRow = new int[_boardSize];
      bool rowComplete = true;

      for (int col = 0; col < _boardSize; col++)
      {
        // Collect valid numbers for cell (row, col)
        availableNumbers.Clear();
        for (int num = 1; num <= _boardSize; num++)
        {
          if (IsValid(row, col, num, currentRow))
          {
            availableNumbers.Add(num);
          }
        }

        // Dead end reached in this row attempt
        if (availableNumbers.Count == 0)
        {
          rowComplete = false;
          break;
        }

        // Pick a random valid number
        int selectedIndex = _random.Next(availableNumbers.Count);
        currentRow[col] = availableNumbers[selectedIndex];
      }

      if (rowComplete)
      {
        return (currentRow, true, attempts);
      }
    }

    // Row failed after MAX_ROW_ATTEMPTS -> trigger backtracking
    return (null, false, attempts);
  }

  private bool IsValid(int row, int col, int num, int[] currentRow)
  {
    // 1. Check current row
    for (int c = 0; c < col; c++)
    {
      if (currentRow[c] == num) return false;
    }

    // 2. Check previous rows for column conflicts
    for (int r = 0; r < row; r++)
    {
      if (generatedNumbers[r][col] == num) return false;
    }

    // 3. Check 3x3 subgrid in previous rows
    int boxStartRow = (row / _boxSize) * _boxSize;
    int boxStartCol = (col / _boxSize) * _boxSize;

    for (int r = boxStartRow; r < row; r++)
    {
      for (int c = boxStartCol; c < boxStartCol + _boxSize; c++)
      {
        if (generatedNumbers[r][c] == num) return false;
      }
    }

    return true;
  }
}

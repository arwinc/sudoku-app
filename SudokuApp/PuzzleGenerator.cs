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

  public PuzzleGenerator(IClueStrategy? clueStrategy, Random? random) : this(9, 30, clueStrategy, random) { }
  public PuzzleGenerator(Random? random) : this(null, random) { }

  public Board Generate()
  {
    int[,] grid = new int[_boardSize, _boardSize];
    int totalAttempts = 0;
    int totalBacktracks = 0;
    const int MAX_BACKTRACKS = 5_000;
    int row = 0;

    // Loop until all 9 rows are successfully generated
    while (row < _boardSize)
    {
      (bool success, int attempts) = TryGenerateRow(grid, row);

      totalAttempts += attempts;

      if (success)
      {
        row++; // Move to next row
      }
      else
      {
        // BACKTRACK: Clear current row
        ClearRow(grid, row);

        // Step back 1 row and clear
        row = Math.Max(0, row - 1);
        ClearRow(grid, row);

        totalBacktracks++;
        if (totalBacktracks > MAX_BACKTRACKS)
        {
          throw new InvalidOperationException("Exceeded maximum backtrack attempts");
        }
      }
    }

    Debug.WriteLine($">>> Total backtracks: {totalBacktracks}x");
    Debug.WriteLine($">>> Total attempts: {totalAttempts}x");

    HashSet<int> fixedPositions = _clueStrategy.SelectClues(grid, _preFilledCellCount, _random);

    List<Cell> cells = new(_boardSize * _boardSize);
    for (int rowIndex = 0; rowIndex < _boardSize; rowIndex++)
    {
      for (int colIndex = 0; colIndex < _boardSize; colIndex++)
      {
        int position = rowIndex * _boardSize + colIndex;
        bool isFixed = fixedPositions.Contains(position);
        cells.Add(new Cell(rowIndex, colIndex, grid[rowIndex, colIndex], isFixed));
      }
    }

    return new Board(cells);
  }

  private void ClearRow(int[,] grid, int row)
  {
    for (int col = 0; col < _boardSize; col++)
    {
      grid[row, col] = 0;
    }
  }

  private (bool, int) TryGenerateRow(int[,] grid, int row)
  {
    int attempts = 0;
    while (attempts < MAX_ROW_ATTEMPTS)
    {
      attempts++;
      bool rowComplete = true;

      for (int col = 0; col < _boardSize; col++)
      {
        // Collect valid numbers for cell (row, col)
        List<int> availableNumbers = GetAvailableNumbers(grid, row, col);

        // Dead end reached in this row attempt
        if (availableNumbers.Count == 0)
        {
          rowComplete = false;
          break;
        }

        // Pick a random valid number
        int selectedIndex = _random.Next(availableNumbers.Count);
        grid[row, col] = availableNumbers[selectedIndex];
      }

      if (rowComplete)
      {
        return (true, attempts);
      }

      ClearRow(grid, row);
    }

    // Row failed after MAX_ROW_ATTEMPTS -> trigger backtracking
    return (false, attempts);
  }

  private List<int> GetAvailableNumbers(int[,] grid, int row, int col)
  {
    List<int> availableNumbers = new(_boardSize);
    for (int num = 1; num <= _boardSize; num++)
    {
      if (IsValid(grid, row, col, num))
      {
        availableNumbers.Add(num);
      }
    }

    return availableNumbers;
  }

  private bool IsValid(int[,] grid, int row, int col, int num)
  {
    // 1. Check current row
    for (int c = 0; c < col; c++)
    {
      if (grid[row, c] == num) return false;
    }

    // 2. Check previous rows for column conflicts
    for (int r = 0; r < row; r++)
    {
      if (grid[r, col] == num) return false;
    }

    // 3. Check 3x3 subgrid in previous rows
    int boxStartRow = (row / _boxSize) * _boxSize;
    int boxStartCol = (col / _boxSize) * _boxSize;

    for (int r = boxStartRow; r < row; r++)
    {
      for (int c = boxStartCol; c < boxStartCol + _boxSize; c++)
      {
        if (grid[r, c] == num) return false;
      }
    }

    return true;
  }
}

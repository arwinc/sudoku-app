/*
 * Source: https://www.geeksforgeeks.org/dsa/program-sudoku-generator/
*/

public class SudokuGenerator : IPuzzleGenerator
{
  private readonly int _boardSize;
  private readonly int _boxSize;
  private readonly int _preFilledCellCount;
  private readonly Random _random;

  public SudokuGenerator(int boardSize = 9, int? preFilledCellCount = null, Random? random = null)
  {
    _boardSize = boardSize;
    _boxSize = (int)Math.Sqrt(boardSize);
    _preFilledCellCount = preFilledCellCount ?? (boardSize == 4 ? 7 : 30);
    _random = random ?? Random.Shared;
  }

  public SudokuGenerator(Random? random) : this(9, 30, random) {}

  public Board Generate()
  {
    int[,] sudoku = GenerateSudokuGrid();

    // TODO: Need improvement to ensure there can be only one valid solution
    HashSet<int> fixedPositions = GetRandomFixedPositions();

    int[] solution = sudoku.Cast<int>().ToArray();
    var cells = Enumerable.Range(0, solution.Length)
      .Select(position => new Cell(
        position / _boardSize,
        position % _boardSize,
        solution[position],
        fixedPositions.Contains(position)));

    return new Board(cells);
  }

  // Returns false if given 3x3 block contains num
  // Ensure the number is not used in the box
  private bool IsUnusedInBox(int[,] grid, int rowStart, int colStart, int num)
  {
    for (int i = 0; i < _boxSize; i++)
    {
      for (int j = 0; j < _boxSize; j++)
      {
        if (grid[rowStart + i, colStart + j] == num)
        {
          return false;
        }
      }
    }
    return true;
  }

  // Fill a 3x3 matrix
  // Assign valid random numbers to the 3x3 subgrid
  private void FillBox(int[,] grid, int row, int col)
  {
    int num;
    for (int i = 0; i < _boxSize; i++)
    {
      for (int j = 0; j < _boxSize; j++)
      {
        do
        {
          // Generate a random number between 1 and 9
          num = _random.Next(1, _boardSize + 1);
        } while (!IsUnusedInBox(grid, row, col, num));
        grid[row + i, col + j] = num;
      }
    }
  }

  // Check if it's safe to put num in row i
  // Ensure num is not already used in the row
  private bool IsUnusedInRow(int[,] grid, int i, int num)
  {
    for (int j = 0; j < _boardSize; j++)
    {
      if (grid[i, j] == num)
      {
        return false;
      }
    }
    return true;
  }

  // Check if it's safe to put num in column j
  // Ensure num is not already used in the column
  private bool IsUnusedInCol(int[,] grid, int j, int num)
  {
    for (int i = 0; i < _boardSize; i++)
    {
      if (grid[i, j] == num)
      {
        return false;
      }
    }
    return true;
  }

  // Check if it's safe to put num in the cell (i, j)
  // Ensure num is not used in row, column, or box
  private bool CheckIfSafe(int[,] grid, int i, int j, int num)
  {
    return (IsUnusedInRow(grid, i, num) && IsUnusedInCol(grid, j, num) &&
            IsUnusedInBox(grid, i - i % _boxSize, j - j % _boxSize, num));
  }

  // Fill the diagonal 3x3 matrices
  // The diagonal blocks are filled to simplify the process
  private void FillDiagonal(int[,] grid, bool reset = false)
  {
    for (int i = 0; i < _boardSize; i += _boxSize)
    {
      if (reset)
      {
        ResetBox(grid, i, i);
        continue;
      }

      // Fill each 3x3 subgrid diagonally
      FillBox(grid, i, i);
    }
  }

  private void ResetBox(int[,] grid, int row, int col)
  {
    for (int i = 0; i < _boxSize; i++)
    {
      for (int j = 0; j < _boxSize; j++)
      {
        grid[row + i, col + j] = 0;
      }
    }
  }

  // Fill remaining blocks in the grid
  // Recursively fill the remaining cells with valid numbers
  private bool FillRemaining(int[,] grid, int i, int j)
  {
    if (i == _boardSize)
    {
      return true;
    }
    if (j == _boardSize)
    {
      return FillRemaining(grid, i + 1, 0);
    }
    if (grid[i, j] != 0)
    {
      return FillRemaining(grid, i, j + 1);
    }
    for (int num = 1; num <= _boardSize; num++)
    {
      if (CheckIfSafe(grid, i, j, num))
      {
        grid[i, j] = num;
        if (FillRemaining(grid, i, j + 1))
        {
          return true;
        }
        grid[i, j] = 0;
      }
    }
    return false;
  }

  // Generate a Sudoku grid
  private int[,] GenerateSudokuGrid()
  {
    int[,] grid = new int[_boardSize, _boardSize];
    bool isFit;

    do
    {
      FillDiagonal(grid);

      isFit = FillRemaining(grid, 0, 0);
      if (!isFit)
      {
        // BACKTRACK: Reset previous diagonal
        FillDiagonal(grid, reset: true);
      }
    } while (!isFit);

    return grid;
  }

  private HashSet<int> GetRandomFixedPositions()
  {
    int totalCells = _boardSize * _boardSize;
    int[] positions = Enumerable.Range(0, totalCells).ToArray();
    for (int index = 0; index < _preFilledCellCount; index++)
    {
      int selected = _random.Next(index, positions.Length);
      (positions[index], positions[selected]) = (positions[selected], positions[index]);
    }

    return positions.Take(_preFilledCellCount).ToHashSet();
  }
}

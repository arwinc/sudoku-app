/*
 * Source: https://www.geeksforgeeks.org/dsa/program-sudoku-generator/
*/

public class SudokuGenerator
{
  private const int BOX_SIZE = 3;
  private const int PRE_FILLED_CELL_COUNT = 30;
  private readonly Random _random;

  public SudokuGenerator(Random? random = null)
  {
    _random = random ?? Random.Shared;
  }

  public Board Generate()
  {
    int[,] sudoku = GenerateSudokuGrid();

    // TODO: Need improvement to ensure there can be only one valid solution
    HashSet<int> fixedPositions = GetRandomFixedPositions();

    int[] solution = sudoku.Cast<int>().ToArray();
    var cells = Enumerable.Range(0, solution.Length)
      .Select(position => new Cell(
        position / Board.Size,
        position % Board.Size,
        solution[position],
        fixedPositions.Contains(position)));

    return new Board(cells);
  }

  // Returns false if given 3x3 block contains num
  // Ensure the number is not used in the box
  private bool IsUnusedInBox(int[,] grid, int rowStart, int colStart, int num)
  {
    for (int i = 0; i < BOX_SIZE; i++)
    {
      for (int j = 0; j < BOX_SIZE; j++)
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
    for (int i = 0; i < BOX_SIZE; i++)
    {
      for (int j = 0; j < BOX_SIZE; j++)
      {
        do
        {
          // Generate a random number between 1 and 9
          num = _random.Next(1, 10);
        } while (!IsUnusedInBox(grid, row, col, num));
        grid[row + i, col + j] = num;
      }
    }
  }

  // Check if it's safe to put num in row i
  // Ensure num is not already used in the row
  private bool IsUnusedInRow(int[,] grid, int i, int num)
  {
    for (int j = 0; j < Board.Size; j++)
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
    for (int i = 0; i < Board.Size; i++)
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
            IsUnusedInBox(grid, i - i % BOX_SIZE, j - j % BOX_SIZE, num));
  }

  // Fill the diagonal 3x3 matrices
  // The diagonal blocks are filled to simplify the process
  private void FillDiagonal(int[,] grid)
  {
    for (int i = 0; i < Board.Size; i += BOX_SIZE)
    {
      // Fill each 3x3 subgrid diagonally
      FillBox(grid, i, i);
    }
  }

  // Fill remaining blocks in the grid
  // Recursively fill the remaining cells with valid numbers
  private bool FillRemaining(int[,] grid, int i, int j)
  {
    if (i == Board.Size)
    {
      return true;
    }
    if (j == Board.Size)
    {
      return FillRemaining(grid, i + 1, 0);
    }
    if (grid[i, j] != 0)
    {
      return FillRemaining(grid, i, j + 1);
    }
    for (int num = 1; num <= Board.Size; num++)
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
    int[,] grid = new int[Board.Size, Board.Size];
    FillDiagonal(grid);
    FillRemaining(grid, 0, 0);
    return grid;
  }

  private HashSet<int> GetRandomFixedPositions()
  {
    int[] positions = Enumerable.Range(0, Board.Size * Board.Size).ToArray();
    for (int index = 0; index < PRE_FILLED_CELL_COUNT; index++)
    {
      int selected = _random.Next(index, positions.Length);
      (positions[index], positions[selected]) = (positions[selected], positions[index]);
    }

    return positions.Take(PRE_FILLED_CELL_COUNT).ToHashSet();
  }
}

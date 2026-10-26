using System.Diagnostics;

public interface IPuzzleGenerator
{
  Board Generate();
}

public class PuzzleGenerator : IPuzzleGenerator
{
  private const int BOX_SIZE = 3;
  private const int PRE_FILLED_CELL_COUNT = 30;
  private const int MAX_ROW_ATTEMPTS = 5;
  private readonly List<int[]> generatedNumbers = [];
  private readonly Random _random;

  public PuzzleGenerator(Random? random = null)
  {
    _random = random ?? Random.Shared;
  }

  public Board Generate()
  {
    generatedNumbers.Clear();

    int totalAttempts = 0;
    int row = 0;

    // Loop until all 9 rows are successfully generated
    while (row < Board.Size)
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

    HashSet<int> fixedPositions = GetRandomFixedPositions(_random);

    List<Cell> cells = [..generatedNumbers
      .SelectMany((usedValue, rowIndex) => usedValue.Select((value, colIndex) => {
        int position = rowIndex * Board.Size + colIndex;
        bool isFixed = fixedPositions.Contains(position);
        return new Cell(rowIndex, colIndex, value, isFixed);
      }
    ))];

    return new Board(cells);
  }

  private static HashSet<int> GetRandomFixedPositions(Random random)
  {
    int[] positions = Enumerable.Range(0, Board.Size * Board.Size).ToArray();
    for (int i = 0; i < PRE_FILLED_CELL_COUNT; i++)
    {
      int selected = random.Next(i, positions.Length);
      (positions[i], positions[selected]) = (positions[selected], positions[i]);
    }

    return positions.Take(PRE_FILLED_CELL_COUNT).ToHashSet();
  }

  private (int[]?, bool, int) TryGenerateRow(int row)
  {
    int attempts = 0;
    List<int> availableNumbers = new(Board.Size);

    while (attempts < MAX_ROW_ATTEMPTS)
    {
      attempts++;
      int[] currentRow = new int[Board.Size];
      bool rowComplete = true;

      for (int col = 0; col < Board.Size; col++)
      {
        // Collect valid numbers for cell (row, col)
        availableNumbers.Clear();
        for (int num = 1; num <= Board.Size; num++)
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
    int boxStartRow = (row / BOX_SIZE) * BOX_SIZE;
    int boxStartCol = (col / BOX_SIZE) * BOX_SIZE;

    for (int r = boxStartRow; r < row; r++)
    {
      for (int c = boxStartCol; c < boxStartCol + BOX_SIZE; c++)
      {
        if (generatedNumbers[r][c] == num) return false;
      }
    }

    return true;
  }
}
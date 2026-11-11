public interface IClueStrategy
{
  HashSet<int> SelectClues(int[,] solvedGrid, int targetClueCount, Random random);
}

public class RandomClueStrategy() : IClueStrategy
{
  public HashSet<int> SelectClues(int[,] solvedGrid, int targetClueCount, Random? random)
  {
    random ??= Random.Shared;
    int size = solvedGrid.GetLength(0);
    int[] positions = Enumerable.Range(0, size * size).ToArray();
    for (int i = 0; i < targetClueCount; i++)
    {
      int selected = random.Next(i, positions.Length);
      (positions[i], positions[selected]) = (positions[selected], positions[i]);
    }

    return positions.Take(targetClueCount).ToHashSet();
  }
}

public class UniqueClueStrategy() : IClueStrategy
{
  private int CountSolutions(int[,] grid, int count = 0)
  {
    int size = grid.GetLength(0);
    int boxSize = (int)Math.Sqrt(size);

    int emptyRow = -1;
    int emptyCol = -1;
    bool foundEmpty = false;

    for (int row = 0; row < size; row++)
    {
      for (int col = 0; col < size; col++)
      {
        if (grid[row, col] == 0)
        {
          emptyRow = row;
          emptyCol = col;
          foundEmpty = true;
          break;
        }
      }
      if (foundEmpty) break;
    }

    if (!foundEmpty)
    {
      return count + 1;
    }

    for (int num = 1; num <= size; num++)
    {
      if (IsSafeToPlace(grid, emptyRow, emptyCol, num, boxSize))
      {
        grid[emptyRow, emptyCol] = num;
        count = CountSolutions(grid, count);

        grid[emptyRow, emptyCol] = 0;
        if (count > 1)
        {
          return count;
        }
      }
    }

    return count;
  }

  private bool IsSafeToPlace(int[,] grid, int row, int col, int num, int boxSize)
  {
    int size = grid.GetLength(0);
    for (int i = 0; i < size; i++)
    {
      if (grid[row, i] == num) return false;
      if (grid[i, col] == num) return false;
    }

    int boxStartRow = (row / boxSize) * boxSize;
    int boxStartCol = (col / boxSize) * boxSize;
    for (int r = 0; r < boxSize; r++)
    {
      for (int c = 0; c < boxSize; c++)
      {
        if (grid[boxStartRow + r, boxStartCol + c] == num) return false;
      }
    }

    return true;
  }

  public HashSet<int> SelectClues(int[,] solvedGrid, int targetClueCount, Random? random)
  {
    random ??= Random.Shared;

    int[,] puzzleGrid = (int[,])solvedGrid.Clone();
    int size = puzzleGrid.GetLength(0);
    int totalCells = size * size;

    int[] positions = Enumerable.Range(0, totalCells).ToArray();
    for (int i = 0; i < targetClueCount; i++)
    {
      int selected = random.Next(i, positions.Length);
      (positions[i], positions[selected]) = (positions[selected], positions[i]);
    }

    int cluesRemaining = totalCells;
    foreach (int pos in positions)
    {
      if (cluesRemaining <= targetClueCount)
        break;

      int row = pos / size;
      int col = pos % size;

      // Backup existing clue value
      int originalValue = puzzleGrid[row, col];

      // Reset the clue
      puzzleGrid[row, col] = 0;

      // Check if clearing this clue leaves exactly 1 unique solution
      if (CountSolutions(puzzleGrid) == 1)
      {
        cluesRemaining--;
      }
      else
      {
        // Not unique anymore -> restore clue
        puzzleGrid[row, col] = originalValue;
      }
    }

    // Collect positions of all remaining non-zero clues
    HashSet<int> fixedPositions = new HashSet<int>();
    for (int r = 0; r < size; r++)
    {
      for (int c = 0; c < size; c++)
      {
        if (puzzleGrid[r, c] != 0)
        {
          fixedPositions.Add(r * size + c);
        }
      }
    }

    return fixedPositions;
  }
}

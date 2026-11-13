namespace SudokuApp.Tests;

public class SudokuGeneratorTests
{
  private readonly IClueStrategy clueStrategy = new RandomClueStrategy();

  [Fact]
  public void Generate_Returns81CellsWithExactly30PreFilled()
  {
    var board = NewPuzzleGenerator().Generate();

    Assert.Equal(9 * 9, board.Cells.Count);
    Assert.Equal(30, board.Cells.Count(cell => cell.IsFixed));
  }

  [Fact]
  public void Generate_ReturnsAValidCompletedSolution()
  {
    var board = NewPuzzleGenerator().Generate();
    int[] expectedValues = Enumerable.Range(1, 9).ToArray();

    foreach (var row in board.Cells.GroupBy(cell => cell.RowIndex))
    {
      Assert.Equal(expectedValues, row.Select(cell => cell.Value).Order().ToArray());
    }

    foreach (var column in board.Cells.GroupBy(cell => cell.ColumnIndex))
    {
      Assert.Equal(expectedValues, column.Select(cell => cell.Value).Order().ToArray());
    }

    for (int boxRow = 0; boxRow < 9; boxRow += 3)
    {
      for (int boxColumn = 0; boxColumn < 9; boxColumn += 3)
      {
        int[] boxValues = board.Cells
          .Where(cell => cell.RowIndex >= boxRow && cell.RowIndex < boxRow + 3
            && cell.ColumnIndex >= boxColumn && cell.ColumnIndex < boxColumn + 3)
          .Select(cell => cell.Value)
          .Order()
          .ToArray();

        Assert.Equal(expectedValues, boxValues);
      }
    }
  }

  [Fact]
  public void Generate_IsReproducibleWithTheSameSeed()
  {
    var firstBoard = NewPuzzleGenerator().Generate();
    var secondBoard = NewPuzzleGenerator().Generate();

    var firstCells = firstBoard.Cells
      .Select(cell => (cell.RowIndex, cell.ColumnIndex, cell.Value, cell.IsFixed));
    var secondCells = secondBoard.Cells
      .Select(cell => (cell.RowIndex, cell.ColumnIndex, cell.Value, cell.IsFixed));

    Assert.Equal(firstCells, secondCells);
  }

  private IPuzzleGenerator NewPuzzleGenerator()
  {
    return new SudokuGenerator(clueStrategy, new Random(42));
  }
}
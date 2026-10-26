namespace SudokuApp.Tests;

public class SudokuGeneratorTests
{
  [Fact]
  public void Generate_Returns81CellsWithExactly30PreFilled()
  {
    var board = new SudokuGenerator(new Random(42)).Generate();

    Assert.Equal(Board.Size * Board.Size, board.Cells.Count);
    Assert.Equal(30, board.Cells.Count(cell => cell.IsFixed));
  }

  [Fact]
  public void Generate_ReturnsAValidCompletedSolution()
  {
    var board = new SudokuGenerator(new Random(42)).Generate();
    int[] expectedValues = Enumerable.Range(1, Board.Size).ToArray();

    foreach (var row in board.Cells.GroupBy(cell => cell.RowIndex))
    {
      Assert.Equal(expectedValues, row.Select(cell => cell.Value).Order().ToArray());
    }

    foreach (var column in board.Cells.GroupBy(cell => cell.ColumnIndex))
    {
      Assert.Equal(expectedValues, column.Select(cell => cell.Value).Order().ToArray());
    }

    for (int boxRow = 0; boxRow < Board.Size; boxRow += 3)
    {
      for (int boxColumn = 0; boxColumn < Board.Size; boxColumn += 3)
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
    var firstBoard = new SudokuGenerator(new Random(42)).Generate();
    var secondBoard = new SudokuGenerator(new Random(42)).Generate();

    var firstCells = firstBoard.Cells
      .Select(cell => (cell.RowIndex, cell.ColumnIndex, cell.Value, cell.IsFixed));
    var secondCells = secondBoard.Cells
      .Select(cell => (cell.RowIndex, cell.ColumnIndex, cell.Value, cell.IsFixed));

    Assert.Equal(firstCells, secondCells);
  }
}
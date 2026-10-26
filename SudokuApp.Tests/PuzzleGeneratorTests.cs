namespace SudokuApp.Tests;

public class PuzzleGeneratorTests
{
  [Fact]
  public void Generate_ReturnsBoardWith81CellsAnd30PreFilledCells()
  {
    var board = new PuzzleGenerator(new Random(42)).Generate();

    Assert.Equal(81, board.Cells.Count);
    Assert.Equal(30, board.Cells.Count(cell => cell.IsFixed));
  }

  [Fact]
  public void Generate_ReturnsValidCompletedSolution()
  {
    var board = new PuzzleGenerator(new Random(42)).Generate();
    int[] expectedValues = Enumerable.Range(1, 9).ToArray();

    for (int row = 0; row < 9; row++)
    {
      int[] rowValues = board.Cells
        .Where(cell => cell.RowIndex == row)
        .Select(cell => cell.Value)
        .Order()
        .ToArray();

      Assert.Equal(expectedValues, rowValues);
    }

    for (int column = 0; column < 9; column++)
    {
      int[] columnValues = board.Cells
        .Where(cell => cell.ColumnIndex == column)
        .Select(cell => cell.Value)
        .Order()
        .ToArray();

      Assert.Equal(expectedValues, columnValues);
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
  public void Generate_ProducesSameBoard_WhenRandomSeedIsTheSame()
  {
    var firstBoard = new PuzzleGenerator(new Random(42)).Generate();
    var secondBoard = new PuzzleGenerator(new Random(42)).Generate();

    var firstCells = firstBoard.Cells
      .Select(cell => (cell.RowIndex, cell.ColumnIndex, cell.Value, cell.IsFixed));
    var secondCells = secondBoard.Cells
      .Select(cell => (cell.RowIndex, cell.ColumnIndex, cell.Value, cell.IsFixed));

    Assert.Equal(firstCells, secondCells);
  }


  [Fact]
  public void Generate_ReturnsAValidCompletedSolution_UnderStress()
  {
    var random = new Random(42);
    for (int i = 0; i < 500; i++)
    {

      var board = new PuzzleGenerator(random).Generate();
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
  }
}
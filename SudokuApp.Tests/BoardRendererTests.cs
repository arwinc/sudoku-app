namespace SudokuApp.Tests;

public class BoardRendererTests
{
  [Fact]
  public void Render_WritesHeaderAndAllRowsInColumnOrder()
  {
    using var output = new StringWriter();
    var board = new Board(Enumerable.Range(0, Board.Size)
      .SelectMany(row => Enumerable.Range(0, Board.Size)
        .Select(column => new Cell(row, column, column + 1, true))));
    var renderer = new BoardRenderer(output);

    renderer.Render(board);

    string[] lines = output.ToString()
      .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

    Assert.Equal(Board.Size + 1, lines.Length);
    Assert.StartsWith(" A ", lines[1]);
    Assert.EndsWith(" 9 ", lines[1]);
    Assert.StartsWith(" I ", lines[Board.Size]);
    Assert.EndsWith(" 9 ", lines[Board.Size]);
  }

  [Fact]
  public void Render_FormatsFixedEnteredAndEmptyCells()
  {
    using var output = new StringWriter();
    var fixedCell = new Cell(0, 0, 5, true);
    var enteredCell = new Cell(0, 1, 3, false);
    var emptyCell = new Cell(0, 2, 4, false);
    Assert.True(enteredCell.TrySetUserValue(7));
    var renderer = new BoardRenderer(output);

    renderer.Render(new Board([fixedCell, enteredCell, emptyCell]));

    string row = output.ToString()
      .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)[1];
    Assert.Contains(" 5 ", row);
    Assert.Contains("\u001b[31m7\u001b[0m", row);
    Assert.Contains("_", row);
  }
}
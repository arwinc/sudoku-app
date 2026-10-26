namespace SudokuApp.Tests;

public class BoardTests
{
  [Fact]
  public void TryPlaceValue_AllowsValidPlacement()
  {
    var cell = new Cell(0, 0, 5, false);
    var board = new Board(new[] { cell });

    var result = board.TryPlaceValue(cell, 7);

    Assert.True(result);
    Assert.Equal(7, cell.UserValue);
  }

  [Fact]
  public void TryPlaceValue_RejectsFixedCell()
  {
    var cell = new Cell(0, 0, 5, true);
    var board = new Board(new[] { cell });

    var result = board.TryPlaceValue(cell, 8);

    Assert.False(result);
    Assert.False(cell.UserValue.HasValue);
  }

  [Fact]
  public void TryPlaceValue_RejectsInvalidNumbers()
  {
    var cell = new Cell(0, 0, 5, false);
    var board = new Board(new[] { cell });

    Assert.False(board.TryPlaceValue(cell, 0));
    Assert.False(board.TryPlaceValue(cell, 10));
    Assert.False(cell.UserValue.HasValue);
  }

  [Fact]
  public void CheckCell_DetectsRowViolation()
  {
    var first = new Cell(0, 0, 4, false);
    var second = new Cell(0, 1, 9, false);
    var board = new Board(new[] { first, second });

    Assert.True(board.TryPlaceValue(first, 7));
    Assert.True(board.TryPlaceValue(second, 7));

    Assert.Equal(ViolationCheck.RowViolation, board.CheckCell(second));
  }

  [Fact]
  public void CheckCell_DetectsColumnViolation()
  {
    var first = new Cell(0, 0, 4, false);
    var second = new Cell(1, 0, 9, false);
    var board = new Board(new[] { first, second });

    Assert.True(board.TryPlaceValue(first, 7));
    Assert.True(board.TryPlaceValue(second, 7));

    Assert.Equal(ViolationCheck.ColumnViolation, board.CheckCell(second));
  }

  [Fact]
  public void CheckCell_DetectsSubgridViolation()
  {
    var first = new Cell(0, 0, 4, false);
    var second = new Cell(1, 1, 9, false);
    var board = new Board(new[] { first, second });

    Assert.True(board.TryPlaceValue(first, 7));
    Assert.True(board.TryPlaceValue(second, 7));

    Assert.Equal(ViolationCheck.SubgridViolation, board.CheckCell(second));
  }

  [Fact]
  public void CheckBoard_ReturnsViolationForFirstOffendingCell()
  {
    var first = new Cell(0, 0, 5, false);
    var second = new Cell(0, 1, 9, false);
    var third = new Cell(1, 0, 7, false);
    var board = new Board(new[] { first, second, third });

    Assert.True(board.TryPlaceValue(first, 5));
    Assert.True(board.TryPlaceValue(second, 5));
    Assert.True(board.TryPlaceValue(third, 7));

    var violation = board.CheckBoard();

    Assert.NotNull(violation);
    Assert.Equal(first, violation!.cell);
    Assert.Equal(ViolationCheck.RowViolation, violation.type);
  }

  [Fact]
  public void IsSolved_ReturnsTrue_WhenBoardIsSolved()
  {
    var board = new Board(new[]
    {
      new Cell(0, 0, 5, true),
      new Cell(0, 1, 3, true),
      new Cell(0, 2, 4, true),
      new Cell(1, 0, 6, true),
      new Cell(1, 1, 7, true),
      new Cell(1, 2, 2, true),
      new Cell(2, 0, 1, true),
      new Cell(2, 1, 9, true),
      new Cell(2, 2, 8, true)
    });

    Assert.True(board.IsSolved());
  }

  [Fact]
  public void CheckBoard_ReturnsNull_WhenNoViolationsExist()
  {
    var first = new Cell(0, 0, 5, false);
    var second = new Cell(0, 1, 6, false);
    var third = new Cell(1, 0, 7, false);
    var board = new Board(new[] { first, second, third });

    Assert.True(board.TryPlaceValue(first, 5));
    Assert.True(board.TryPlaceValue(second, 6));
    Assert.True(board.TryPlaceValue(third, 7));

    Assert.Null(board.CheckBoard());
  }
}

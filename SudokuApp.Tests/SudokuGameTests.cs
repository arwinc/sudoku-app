namespace SudokuApp.Tests;

public class SudokuGameTests
{
  [Fact]
  public void GetHintCell_SelectsFromUnresolvedEditableCells()
  {
    var fixedCell = new Cell(0, 0, 5, true);
    var firstEditableCell = new Cell(0, 1, 2, false);
    var secondEditableCell = new Cell(0, 2, 3, false);
    Assert.True(firstEditableCell.TrySetUserValue(firstEditableCell.Value));
    var game = new SudokuGame(
      new Board([fixedCell, firstEditableCell, secondEditableCell]),
      new FixedRandom(0));

    Assert.Same(secondEditableCell, game.GetHintCell());
  }

  [Fact]
  public void GetHintCell_ReturnsNull_WhenNoUnresolvedEditableCellsExist()
  {
    var fixedCell = new Cell(0, 0, 5, true);
    var solvedCell = new Cell(0, 1, 2, false);
    Assert.True(solvedCell.TrySetUserValue(solvedCell.Value));
    var game = new SudokuGame(new Board([fixedCell, solvedCell]), new FixedRandom(0));

    Assert.Null(game.GetHintCell());
  }

  [Fact]
  public void ProcessCellCommand_PlacesValue()
  {
    var cell = new Cell(0, 0, 5, false);
    var game = new SudokuGame(BoardTests.CreateTestBoard9x9(cell));

    var result = game.ProcessCellCommand(
      new SudokuCommand(SudokuCommandType.PlaceValue, 0, 0, 7));

    Assert.Equal(CellCommandStatus.Accepted, result.Status);
    Assert.Equal(7, cell.UserValue);
  }

  [Fact]
  public void ProcessCellCommand_ClearsEditableCell()
  {
    var cell = new Cell(0, 0, 5, false);
    Assert.True(cell.TrySetUserValue(7));
    var game = new SudokuGame(new Board([cell]));

    var result = game.ProcessCellCommand(
      new SudokuCommand(SudokuCommandType.Clear, 0, 0));

    Assert.Equal(CellCommandStatus.Accepted, result.Status);
    Assert.Null(cell.UserValue);
  }

  [Fact]
  public void ProcessUndoCommand_RevertLastCellCommand()
  {
    var cell = new Cell(0, 0, 5, false);
    var game = new SudokuGame(new Board([cell]));

    // Action: A1 1
    game.ProcessCellCommand(SudokuCommand.Create(SudokuCommandType.PlaceValue, cell, 1));

    game.UndoLastAction();
    Assert.Null(cell.UserValue);

    // Action: A1 1 » A1 clear
    game.ProcessCellCommand(SudokuCommand.Create(SudokuCommandType.PlaceValue, cell, 1));
    game.ProcessCellCommand(SudokuCommand.Create(SudokuCommandType.Clear, cell));

    game.UndoLastAction();
    Assert.Equal(1, cell.UserValue);
  }

  [Fact]
  public void ProcessUndoCommand_RevertMultipleCellCommandsAcrossGame()
  {
    var cell1 = new Cell(0, 0, 5, false);
    var cell2 = new Cell(1, 1, 6, false);
    var game = new SudokuGame(BoardTests.CreateTestBoard9x9(cell1, cell2));

    // Action : A1 1 » B2 2 » A1 3
    // History: A1 ø » B2 ø » A1 1
    game.ProcessCellCommand(SudokuCommand.Create(SudokuCommandType.PlaceValue, cell1, 1));
    game.ProcessCellCommand(SudokuCommand.Create(SudokuCommandType.PlaceValue, cell2, 2));
    game.ProcessCellCommand(SudokuCommand.Create(SudokuCommandType.PlaceValue, cell1, 3));

    // Current board
    Assert.Equal(3, cell1.UserValue);
    Assert.Equal(2, cell2.UserValue);

    // Undo A1 3
    game.UndoLastAction();
    Assert.Equal(1, cell1.UserValue);
    Assert.Equal(2, cell2.UserValue);

    // Undo B2 2
    game.UndoLastAction();
    Assert.Equal(1, cell1.UserValue);
    Assert.Null(cell2.UserValue);

    // Undo A1 1
    game.UndoLastAction();
    Assert.Null(cell1.UserValue);
    Assert.Null(cell2.UserValue);

    // Fill up again
    // Action : A1 1 » B2 2 » A1 clear » A1 3
    // History: A1 ø » B2 ø » A1 1     » A1 ø
    game.ProcessCellCommand(SudokuCommand.Create(SudokuCommandType.PlaceValue, cell1, 1));
    game.ProcessCellCommand(SudokuCommand.Create(SudokuCommandType.PlaceValue, cell2, 2));
    game.ProcessCellCommand(SudokuCommand.Create(SudokuCommandType.Clear, cell1));
    game.ProcessCellCommand(SudokuCommand.Create(SudokuCommandType.PlaceValue, cell1, 3));

    // Current board
    Assert.Equal(3, cell1.UserValue);
    Assert.Equal(2, cell2.UserValue);

    // Undo A1 3
    game.UndoLastAction(); 
    Assert.Null(cell1.UserValue);
    Assert.Equal(2, cell2.UserValue);

    // Undo A1 clear
    game.UndoLastAction();
    Assert.Equal(1, cell1.UserValue);
    Assert.Equal(2, cell2.UserValue);

    // Undo B1 2
    game.UndoLastAction();
    Assert.Equal(1, cell1.UserValue);
    Assert.Null(cell2.UserValue);

    // Undo A1 1
    game.UndoLastAction();
    Assert.Null(cell1.UserValue);
    Assert.Null(cell2.UserValue);
  }

  [Fact]
  public void ProcessCellCommand_RejectsFixedCell()
  {
    var cell = new Cell(0, 0, 5, true);
    var game = new SudokuGame(new Board([cell]));

    var result = game.ProcessCellCommand(
      new SudokuCommand(SudokuCommandType.PlaceValue, 0, 0, 7));

    Assert.Equal(CellCommandStatus.FixedCell, result.Status);
    Assert.Null(cell.UserValue);
  }

  [Fact]
  public void CheckBoard_ReturnsBoardViolation()
  {
    var first = new Cell(0, 0, 4, false);
    var second = new Cell(0, 1, 9, false);
    var board = BoardTests.CreateTestBoard9x9(first, second);
    Assert.True(board.TryPlaceValue(first, 7));
    Assert.True(board.TryPlaceValue(second, 7));
    var game = new SudokuGame(board);

    var violation = game.CheckBoard();

    Assert.NotNull(violation);
    Assert.Equal(ViolationCheck.RowViolation, violation!.type);
  }

  private sealed class FixedRandom(int selectedIndex) : Random
  {
    public override int Next(int maxValue) => selectedIndex;
  }
}
namespace SudokuApp.Tests;

public class SudokuCommandParserTests
{
  private readonly SudokuCommandParser _sut = new();

  [Fact]
  public void Parse_ReturnsQuitCommand()
  {
    Assert.Equal(SudokuCommandType.Quit, _sut.Parse("quit").Type);
  }

  [Fact]
  public void Parse_ReturnsHintCommand()
  {
    Assert.Equal(SudokuCommandType.Hint, _sut.Parse("hint").Type);
  }

  [Fact]
  public void Parse_ReturnsCheckCommand()
  {
    Assert.Equal(SudokuCommandType.Check, _sut.Parse("check").Type);
  }

  [Fact]
  public void Parse_ReturnsPlacementWithZeroBasedCoordinatesAndValue()
  {
    var command = _sut.Parse("B3 7");

    Assert.Equal(SudokuCommandType.PlaceValue, command.Type);
    Assert.Equal(1, command.RowIndex);
    Assert.Equal(2, command.ColumnIndex);
    Assert.Equal(7, command.Value);
  }

  [Fact]
  public void Parse_ReturnsClearWithZeroBasedCoordinates()
  {
    var command = _sut.Parse("C5 clear");

    Assert.Equal(SudokuCommandType.Clear, command.Type);
    Assert.Equal(2, command.RowIndex);
    Assert.Equal(4, command.ColumnIndex);
    Assert.Null(command.Value);
  }

  [Fact]
  public void Parse_ReturnsUndoCommand()
  {
    Assert.Equal(SudokuCommandType.Undo, _sut.Parse("undo").Type);
  }

  [Theory]
  [InlineData("new 4", 4)]
  [InlineData("new 9", 9)]
  public void Parse_ReturnsNewGameCommand_WithExpectedBoardSize(string input, int expectedSize)
  {
    var command = _sut.Parse(input);
    Assert.Equal(SudokuCommandType.NewGame, command.Type);
    Assert.Equal(expectedSize, command.BoardSize);
    Assert.Equal(expectedSize, _sut.BoardSize);
  }

  [Fact]
  public void Parse_ReturnsNewGameCommand_RetainingCurrentBoardSize_WhenSizeOmitted()
  {
    _sut.BoardSize = 4;
    var command = _sut.Parse("new");
    Assert.Equal(SudokuCommandType.NewGame, command.Type);
    Assert.Equal(4, command.BoardSize);
  }

  [Fact]
  public void Parse_UpdatesBoardSize_EnforcingDynamicMoveValidation()
  {
    _sut.Parse("new 4");
    var valid4x4Move = _sut.Parse("B2 3");
    Assert.Equal(SudokuCommandType.PlaceValue, valid4x4Move.Type);

    var invalidMove = _sut.Parse("E5 8");
    Assert.Equal(SudokuCommandType.Invalid, invalidMove.Type);
  }

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("A3 0")]
  [InlineData("J1 2")]
  [InlineData("A10 2")]
  [InlineData("A3 clear ")]
  [InlineData("NEW 4")]
  [InlineData(" new 4")]
  [InlineData("new 5")]
  [InlineData("new abc")]
  public void Parse_ReturnsInvalidForMalformedInput(string? input)
  {
    Assert.Equal(SudokuCommandType.Invalid, _sut.Parse(input).Type);
  }
}
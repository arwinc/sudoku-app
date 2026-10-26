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

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("A3 0")]
  [InlineData("J1 2")]
  [InlineData("A10 2")]
  [InlineData("A3 clear ")]
  public void Parse_ReturnsInvalidForMalformedInput(string? input)
  {
    Assert.Equal(SudokuCommandType.Invalid, _sut.Parse(input).Type);
  }
}
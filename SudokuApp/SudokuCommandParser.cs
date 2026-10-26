using System.Text.RegularExpressions;

public enum SudokuCommandType
{
  Invalid,
  Quit,
  Hint,
  Check,
  PlaceValue,
  Clear
}

public sealed record SudokuCommand(
  SudokuCommandType Type,
  int? RowIndex = null,
  int? ColumnIndex = null,
  int? Value = null);

public class SudokuCommandParser
{
  private static readonly Regex MovePattern = new(@"^([A-I])([1-9]) ([1-9]|clear)$");

  public SudokuCommand Parse(string? input)
  {
    if (string.Equals(input, "quit", StringComparison.Ordinal))
    {
      return new SudokuCommand(SudokuCommandType.Quit);
    }

    if (string.Equals(input, "hint", StringComparison.Ordinal))
    {
      return new SudokuCommand(SudokuCommandType.Hint);
    }

    if (string.Equals(input, "check", StringComparison.Ordinal))
    {
      return new SudokuCommand(SudokuCommandType.Check);
    }

    Match match = MovePattern.Match(input ?? string.Empty);
    if (!match.Success)
    {
      return new SudokuCommand(SudokuCommandType.Invalid);
    }

    int rowIndex = match.Groups[1].Value[0] - 'A';
    int columnIndex = match.Groups[2].Value[0] - '1';
    string action = match.Groups[3].Value;

    if (action == "clear")
    {
      return new SudokuCommand(SudokuCommandType.Clear, rowIndex, columnIndex);
    }

    return new SudokuCommand(
      SudokuCommandType.PlaceValue,
      rowIndex,
      columnIndex,
      Convert.ToInt32(action));
  }
}
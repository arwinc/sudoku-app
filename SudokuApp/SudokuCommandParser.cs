using System.Text.RegularExpressions;

public enum SudokuCommandType
{
  Invalid,
  Quit,
  Hint,
  Check,
  PlaceValue,
  Undo,
  Clear,
  NewGame
}

public sealed record SudokuCommand(
  SudokuCommandType Type,
  int? RowIndex = null,
  int? ColumnIndex = null,
  int? Value = null,
  int? BoardSize = null)
{
  public static SudokuCommand Create(
    SudokuCommandType type,
    Cell cell,
    int? value = null,
    int? boardSize = null
  ) => new SudokuCommand(type, cell.RowIndex, cell.ColumnIndex, value, boardSize);
}

public class SudokuCommandParser
{
  public int BoardSize { get; set; } = 9;

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

    if (string.Equals(input, "undo", StringComparison.Ordinal))
    {
      return new SudokuCommand(SudokuCommandType.Undo);
    }

    Match newGameMatch = Regex.Match(input ?? string.Empty, @"^new(?:\s([49]))?$", RegexOptions.IgnoreCase);
    if (newGameMatch.Success)
    {
      string sizeGroup = newGameMatch.Groups[1].Value;
      int? requestedSize = string.IsNullOrEmpty(sizeGroup) ? null : int.Parse(sizeGroup);
      if (requestedSize.HasValue)
      {
        BoardSize = requestedSize.Value;
      }
      return new SudokuCommand(SudokuCommandType.NewGame, BoardSize: BoardSize);
    }

    char maxRowChar = (char)('A' + BoardSize - 1);
    string movePattern = $@"^([A-{maxRowChar}])([1-{BoardSize}])\s+([1-{BoardSize}]|clear)$";

    Match match = Regex.Match(input ?? string.Empty, movePattern, RegexOptions.IgnoreCase);
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
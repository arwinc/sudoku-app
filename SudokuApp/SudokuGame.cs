public enum CellCommandStatus
{
  Accepted,
  FixedCell,
  CellNotFound,
  InvalidValue,
  InvalidCommand
}

public sealed record CellCommandResult(CellCommandStatus Status, Cell? Cell = null);

public class SudokuGame
{
  private readonly Random _random;

  public Board Board { get; }
  public bool IsSolved => Board.IsSolved();

  public SudokuGame(Board board, Random? random = null)
  {
    Board = board;
    _random = random ?? Random.Shared;
  }

  public Cell? GetHintCell()
  {
    Cell[] candidates = Board.Cells
      .Where(cell => cell.IsEditable && cell.UserValue != cell.Value)
      .ToArray();

    return candidates.Length == 0
      ? null
      : candidates[_random.Next(candidates.Length)];
  }

  public Board.BoardViolation? CheckBoard() => Board.CheckBoard();

  public CellCommandResult ProcessCellCommand(SudokuCommand command)
  {
    if (command.Type is not (SudokuCommandType.PlaceValue or SudokuCommandType.Clear)
      || command.RowIndex is not int rowIndex
      || command.ColumnIndex is not int columnIndex)
    {
      return new CellCommandResult(CellCommandStatus.InvalidCommand);
    }

    Cell? cell = Board.GetCell(rowIndex, columnIndex);
    if (cell is null)
    {
      return new CellCommandResult(CellCommandStatus.CellNotFound);
    }

    if (cell.IsFixed)
    {
      return new CellCommandResult(CellCommandStatus.FixedCell, cell);
    }

    if (command.Type == SudokuCommandType.Clear)
    {
      cell.Clear();
      return new CellCommandResult(CellCommandStatus.Accepted, cell);
    }

    if (command.Value is not int value)
    {
      return new CellCommandResult(CellCommandStatus.InvalidCommand, cell);
    }

    if (!Board.TryPlaceValue(cell, value))
    {
      return new CellCommandResult(CellCommandStatus.InvalidValue, cell);
    }

    return new CellCommandResult(CellCommandStatus.Accepted, cell);
  }
}
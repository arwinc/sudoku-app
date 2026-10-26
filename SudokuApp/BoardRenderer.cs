public class BoardRenderer
{
  private readonly TextWriter _output;

  public BoardRenderer(TextWriter? output = null)
  {
    _output = output ?? Console.Out;
  }

  public void Render(Board board)
  {
    WriteRow(' ', string.Join("", Enumerable.Range(1, Board.Size)));
    _output.WriteLine();

    var rowHeadings = board.Cells
      .OrderBy(cell => cell.RowIndex)
      .Select(cell => cell.RowLabel)
      .Distinct();

    foreach (char heading in rowHeadings)
    {
      WriteLabel(heading);
      foreach (Cell cell in board.Cells
        .Where(cell => cell.RowLabel == heading)
        .OrderBy(cell => cell.ColumnIndex))
      {
        _output.Write(' ');
        WriteCellValue(cell);
        _output.Write(' ');
      }

      _output.WriteLine();
    }
  }

  private void WriteRow(char heading, string values)
  {
    WriteLabel(heading);
    foreach (char value in values)
    {
      WriteLabel(value);
    }
  }

  private void WriteLabel(char value) => _output.Write($" {value} ");

  private void WriteCellValue(Cell cell)
  {
    if (cell.IsFixed)
    {
      _output.Write(cell.Value);
    }
    else if (cell.UserValue.HasValue)
    {
      _output.Write($"\u001b[31m{cell.UserValue.Value}\u001b[0m");
    }
    else
    {
      _output.Write('_');
    }
  }
}
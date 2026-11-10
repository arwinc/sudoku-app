public enum ViolationCheck
{
  None,
  RowViolation,
  ColumnViolation,
  SubgridViolation
};

public class Board
{
  public int Size { get; }

  public int BoxSize { get; }

  public IReadOnlyList<Cell> Cells { get; }

  public Board(IEnumerable<Cell> cells)
  {
    Cells = cells.ToList();
    Size = (int)Math.Sqrt(cells.Count());
    BoxSize = (int)Math.Sqrt(Size);
  }

  public Cell? GetCell(int row, int col) =>
    Cells.FirstOrDefault(x => x.RowIndex == row && x.ColumnIndex == col);

  public bool TryPlaceValue(Cell cell, int value)
  {
    if (cell is null || cell.IsFixed) return false;
    if (value is < 1 || value > Size) return false;
    return cell.TrySetUserValue(value);
  }

  public ViolationCheck CheckCell(Cell cell)
  {
    if (!cell.UserValue.HasValue) return ViolationCheck.None;

    var others = Cells.Where(x => x != cell);

    if (others.Any(x => x.RowIndex == cell.RowIndex
      && x.HasCurrentValueOf(cell.UserValue.Value)))
    {
      return ViolationCheck.RowViolation;
    }
    else if (others.Any(x => x.ColumnIndex == cell.ColumnIndex
      && x.HasCurrentValueOf(cell.UserValue.Value)))
    {
      return ViolationCheck.ColumnViolation;
    }
    else if (IsInSameSubgrid(cell.RowIndex, cell.ColumnIndex, cell.UserValue.Value))
    {
      return ViolationCheck.SubgridViolation;
    }
    return ViolationCheck.None;
  }

  private bool IsInSameSubgrid(int row, int col, int value)
  {
    int startRow = (row / BoxSize) * BoxSize;
    int startCol = (col / BoxSize) * BoxSize;

    return Cells
      .Where(x => !(x.RowIndex == row && x.ColumnIndex == col))
      .Any(x => x.RowIndex >= startRow && x.RowIndex < startRow + BoxSize
        && x.ColumnIndex >= startCol && x.ColumnIndex < startCol + BoxSize
        && x.HasCurrentValueOf(value));
  }

  public bool IsSolved() => Cells.All(x => x.IsFixed || x.UserValue == x.Value);

  public BoardViolation? CheckBoard()
  {
    foreach (var cell in Cells.Where(x => x.HasUserValue))
    {
      var violation = CheckCell(cell);
      if (violation != ViolationCheck.None)
      {
        return new BoardViolation(cell, violation);
      }
    }
    return null;
  }

  public sealed record BoardViolation(Cell cell, ViolationCheck type);
}
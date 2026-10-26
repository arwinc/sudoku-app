public class Cell
{
  public int RowIndex { get; }
  public int ColumnIndex { get; }
  public int Value { get; }
  public bool IsFixed { get; }
  public int? UserValue { get; private set; }
  public bool IsEditable => !IsFixed;

  const string ROW_HEADINGS = "ABCDEFGHI";
  const string COL_HEADINGS = "123456789";


  public Cell(int rowIndex, int columnIndex, int value, bool isFixed)
  {
    RowIndex = rowIndex;
    ColumnIndex = columnIndex;
    Value = value;
    IsFixed = isFixed;
  }

  public bool TrySetUserValue(int value)
  {
    if (IsFixed) return false;
    if (value is < 1 or > 9) return false;

    UserValue = value;
    return true;
  }

  public void Clear()
  {
    if (IsFixed) return;
    UserValue = null;
  }

  public char RowLabel => ROW_HEADINGS[RowIndex];
  public char ColLabel => COL_HEADINGS[ColumnIndex];
  public string Id => $"{RowLabel}{ColLabel}";

  public bool HasCurrentValueOf(int value) =>
    IsFixed ? Value == value : UserValue == value;

  public bool HasUserValue =>
    !IsFixed && UserValue.HasValue;
};
public class Cell : ICloneable
{
  public int RowIndex { get; }
  public int ColumnIndex { get; }
  public int Value { get; }
  public bool IsFixed { get; }
  public int? UserValue { get; private set; }
  public bool IsEditable => !IsFixed;

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

  public char RowLabel => (char)('A' + RowIndex);
  public char ColLabel => (char)('1' + ColumnIndex);
  public string Id => $"{RowLabel}{ColLabel}";

  public bool HasCurrentValueOf(int value) =>
    IsFixed ? Value == value : UserValue == value;

  public Cell Clone() => (Cell)this.MemberwiseClone();

  object ICloneable.Clone()
  {
    return Clone();
  }

  public bool HasUserValue =>
    !IsFixed && UserValue.HasValue;
};
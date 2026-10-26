namespace SudokuApp.Tests;

public class CellTests
{
  [Fact]
  public void TrySetUserValue_SetsValue_WhenCellIsEditable()
  {
    var sut = new Cell(1, 1, 1, false);

    Assert.True(sut.TrySetUserValue(2));
    Assert.Equal(2, sut.UserValue);
    Assert.True(sut.HasUserValue);
  }

  [Fact]
  public void TrySetUserValue_RejectsValue_WhenCellIsFixed()
  {
    var sut = new Cell(1, 1, 1, true);

    Assert.False(sut.TrySetUserValue(2));
    Assert.False(sut.UserValue.HasValue);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(10)]
  public void TrySetUserValue_RejectsOutOfRangeValue_WithoutChangingCurrentValue(int value)
  {
    var sut = new Cell(1, 1, 1, false);

    Assert.True(sut.TrySetUserValue(3));

    Assert.False(sut.TrySetUserValue(value));
    Assert.Equal(3, sut.UserValue);
  }

  [Fact]
  public void HasUserValue_ReturnsFalse_WhenCellHasNoUserEntry()
  {
    var sut = new Cell(1, 1, 1, false);

    Assert.False(sut.HasUserValue);
  }

  [Fact]
  public void HasCurrentValueOf_UsesSolutionValue_WhenCellIsFixed()
  {
    var sut = new Cell(1, 1, 5, true);

    Assert.True(sut.HasCurrentValueOf(5));
    Assert.False(sut.HasCurrentValueOf(7));
  }

  [Fact]
  public void HasCurrentValueOf_UsesUserValue_WhenCellIsEditable()
  {
    var sut = new Cell(1, 1, 5, false);
    Assert.True(sut.TrySetUserValue(7));

    Assert.True(sut.HasCurrentValueOf(7));
    Assert.False(sut.HasCurrentValueOf(5));
  }

  [Fact]
  public void Clear_RemovesUserValue_WhenCellIsEditable()
  {
    var sut = new Cell(1, 1, 1, false);

    Assert.True(sut.TrySetUserValue(3));

    sut.Clear();

    Assert.False(sut.UserValue.HasValue);
    Assert.False(sut.HasUserValue);
  }

  [Fact]
  public void Clear_DoesNotChangeValue_WhenCellIsFixed()
  {
    var sut = new Cell(1, 1, 5, true);

    sut.Clear();

    Assert.Equal(5, sut.Value);
    Assert.False(sut.UserValue.HasValue);
  }
}

namespace SudokuApp.Tests;

public class ClueStrategyTests
{
  private readonly int[,] solved4x4 = new int[4, 4]
  {
    { 4, 1, 2, 3 },
    { 3, 2, 1, 4 },
    { 2, 3, 4, 1 },
    { 1, 4, 3, 2 },
  };

  [Fact]
  public void RandomClueStrategy_ReturnsExactTargetCountAndValidIndex()
  {
    var sut = new RandomClueStrategy();
    int numberOfClues = 7;
    int totalCells = 16;

    HashSet<int> clues = sut.SelectClues(solved4x4, numberOfClues, null);

    Assert.Equal(numberOfClues, clues.Count);
    Assert.All(clues, index => Assert.InRange(index, 0, totalCells - 1));
  }

  [Fact]
  public void UniqueClueStrategy_ReturnsOneUniqueSolution()
  {
    var sut = new UniqueClueStrategy();
    int numberOfClues = 7;
    int totalCells = 16;

    HashSet<int> clues = sut.SelectClues(solved4x4, numberOfClues, null);

    Assert.Equal(numberOfClues, clues.Count);
    Assert.All(clues, index => Assert.InRange(index, 0, totalCells - 1));

    // Create blank grid
    // Then filled up with clues
    int[,] testGrid = new int[4, 4];
    foreach (int pos in clues)
    {
      int r = pos / 4;
      int c = pos % 4;
      testGrid[r, c] = solved4x4[r, c];
    }

    Assert.Equal(1, UniqueClueStrategy.CountSolutions(testGrid));
  }

  [Fact]
  public void CountSolutions_ReturnsMultipleSolution_WhenNextClueIsRemoved()
  {
    int[,] gridWithClue = new int[4, 4]
    {
      { 4, 1, 0, 3 }, // (0, 1) holds value 1
      { 3, 0, 0, 4 },
      { 2, 3, 4, 1 },
      { 1, 4, 3, 2 },
    };
    int countWithClue = UniqueClueStrategy.CountSolutions(gridWithClue);
    Assert.Equal(1, countWithClue);

    // Next "random" removes clue at (0, 1)
    int[,] gridNextClue = (int[,])gridWithClue.Clone();
    gridNextClue[0, 1] = 0;

    // When this clue at (0, 1) removed
    // it is now valid to fill either (1)(2) or (2)(1) in row 0 and row 1
    int countNextClue = UniqueClueStrategy.CountSolutions(gridNextClue);
    Assert.Equal(2, countNextClue);
  }
}
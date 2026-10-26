if (args.Length < 2)
{
  string usageInstructions = """
  Usage:
    dotnet run -- <ClassName> <IntegerValue>
    
  Example:
    dotnet run -- SudokuGenerator 42
  """;
  Console.WriteLine(usageInstructions);
  return;
}

string targetClassName = args[0];
string rawNumber = args[1];

if (!int.TryParse(rawNumber, out int maxCount))
{
  Console.WriteLine($"Error: '{rawNumber}' is not a valid integer.");
  return;
}

Type? type = AppDomain.CurrentDomain.GetAssemblies()
    .SelectMany(a => a.GetTypes())
    .FirstOrDefault(t => t.Name.Equals(targetClassName, StringComparison.OrdinalIgnoreCase));
if (type is null)
{
  Console.WriteLine($"Cannot find a class named '{targetClassName}'");
  return;
}

object? instance = Activator.CreateInstance(type, [Random.Shared]);
if (instance is IPuzzleGenerator puzzleGenerator)
{
  Run(puzzleGenerator, maxCount);
}
else
{
  Console.WriteLine($"'{targetClassName}' is not {nameof(IPuzzleGenerator)}");
}

void Run(IPuzzleGenerator puzzleGenerator, int maxCount)
{
  for (int i = 0; i < maxCount; i++)
  {
    Check(puzzleGenerator.Generate());
  }
}

void Check(Board board)
{
  int[] expectedValues = Enumerable.Range(1, 9).ToArray();

  for (int row = 0; row < 9; row++)
  {
    int[] rowValues = board.Cells
      .Where(cell => cell.RowIndex == row)
      .Select(cell => cell.Value)
      .Order()
      .ToArray();

    if (!expectedValues.SequenceEqual(rowValues)) throw new Exception("Duplicate in row");
  }

  for (int column = 0; column < 9; column++)
  {
    int[] columnValues = board.Cells
      .Where(cell => cell.ColumnIndex == column)
      .Select(cell => cell.Value)
      .Order()
      .ToArray();

    if (!expectedValues.SequenceEqual(columnValues)) throw new Exception("Duplicate in column");
  }

  for (int boxRow = 0; boxRow < 9; boxRow += 3)
  {
    for (int boxColumn = 0; boxColumn < 9; boxColumn += 3)
    {
      int[] boxValues = board.Cells
        .Where(cell => cell.RowIndex >= boxRow && cell.RowIndex < boxRow + 3
          && cell.ColumnIndex >= boxColumn && cell.ColumnIndex < boxColumn + 3)
        .Select(cell => cell.Value)
        .Order()
        .ToArray();

      if (!expectedValues.SequenceEqual(boxValues)) throw new Exception("Duplicate in subgrid");
    }
  }

}
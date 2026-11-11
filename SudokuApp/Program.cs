SudokuGame game;
SudokuCommandParser commandParser = new();
BoardRenderer boardRenderer = new();

Console.WriteLine("Welcome to Sudoku");
Console.Write("Select grid size — 4 (4x4) or 9 (9x9) [Default: 9]: ");
string? startupInput = Console.ReadLine();
int chosenSize = "4".Equals(startupInput?.Trim(), StringComparison.Ordinal) ? 4 : 9;
InitBoard(chosenSize);
boardRenderer.Render(game.Board);

string? input;
do
{
  Console.WriteLine("\nEnter command (e.g., A3 4, C5 clear, undo, hint, check, quit):");
  Console.WriteLine("Or start new game (e.g., new, new 4, new 9):");
  input = Console.ReadLine();

  if (input is null || !ProcessInput(input)) break;

  Console.WriteLine("\nCurrent grid:");
  boardRenderer.Render(game.Board);

  if (game.IsSolved)
  {
    Console.WriteLine("\nYou have successfully completed the Sudoku puzzle!");
    Console.WriteLine("Press any key to play again...");
    Console.ReadKey();

    InitBoard(chosenSize);
    boardRenderer.Render(game.Board);
  }
}
while (true);

bool ProcessInput(string? input)
{
  SudokuCommand command = commandParser.Parse(input);
  switch (command.Type)
  {
    case SudokuCommandType.Quit:
      return false;
    case SudokuCommandType.NewGame:
      int newSize = command.BoardSize ?? game.Board.Size;
      Console.WriteLine($"\nStarting new {newSize}x{newSize} game...");
      InitBoard(newSize);
      return true;
    case SudokuCommandType.Hint:
      WriteHintMessage(game.GetHintCell());
      break;
    case SudokuCommandType.Check:
      WriteCheckMessage(game.CheckBoard());
      break;
    case SudokuCommandType.Undo:
      game.UndoLastAction();
      break;
    case SudokuCommandType.PlaceValue:
    case SudokuCommandType.Clear:
      WriteCellCommandMessage(game.ProcessCellCommand(command));
      break;
  }

  return true;
}

void WriteHintMessage(Cell? cell)
{
  if (cell == null)
  {
    Console.WriteLine();
  }
  else
  {
    Console.WriteLine("Hint: Cell {0} = {1}", cell.Id, cell.Value);
  }
}

void WriteCheckMessage(Board.BoardViolation? violation)
{
  if (violation is null)
  {
    Console.WriteLine("No rule violations detected.");
    return;
  }

  var cell = violation.cell;
  string message = violation.type switch
  {
    ViolationCheck.RowViolation => $"Number {cell.UserValue} already exists in Row {cell.RowLabel}.",
    ViolationCheck.ColumnViolation => $"Number {cell.UserValue} already exists in Column {cell.ColLabel}.",
    ViolationCheck.SubgridViolation => $"Number {cell.UserValue} already exists in the same 3x3 subgrid.",
    _ => "No rule violations detected."
  };

  Console.WriteLine(message);
}

void WriteCellCommandMessage(CellCommandResult result)
{
  switch (result.Status)
  {
    case CellCommandStatus.Accepted:
      Console.WriteLine("\nMove accepted.");
      break;
    case CellCommandStatus.FixedCell:
      Console.WriteLine("\nInvalid move. {0} is pre-filled.", result.Cell?.Id);
      break;
    case CellCommandStatus.CellNotFound:
      Console.WriteLine("\nInvalid cell.");
      break;
    case CellCommandStatus.InvalidValue:
      Console.WriteLine("\nInvalid value.");
      break;
  }
}

void InitBoard(int size = 9)
{
  IPuzzleGenerator puzzleGenerator = new SudokuGenerator(boardSize: size);
  commandParser.BoardSize = size;
  game = new SudokuGame(puzzleGenerator.Generate());

  Console.WriteLine("\nHere is your puzzle:");
}

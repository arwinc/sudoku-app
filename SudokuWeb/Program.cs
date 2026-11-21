extern alias SudokuApp;

using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;

Console.SetOut(new DomTextWriter());
SudokuApp::ConsoleReader.ReadLineHandler = () => Program.ReadLineAsync();

var entryPoint = typeof(SudokuApp::Program).GetMethod(
  "<Main>$",
  BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public
);

var result = entryPoint?.Invoke(null, [ Array.Empty<string>() ]);

if (result is Task task)
{
  await task;
}

Console.WriteLine("\n[Session ended]");
Program.OnProgramExited();

partial class Program
{
  [JSImport("dom.appendOutput", "main.js")]
  internal static partial void AppendOutput(string text);

  [JSImport("dom.readLineAsync", "main.js")]
  // Hook up browser input before running your console app
  internal static partial Task<string?> ReadLineAsync();

  [JSImport("dom.onExit", "main.js")]
  internal static partial void OnProgramExited();
}

class DomTextWriter : TextWriter
{
  public override Encoding Encoding => Encoding.UTF8;
  public override void Write(char value) => Program.AppendOutput(value.ToString());
  public override void Write(char[] buffer, int index, int count) => Program.AppendOutput(new string(buffer, index, count));
  public override void Write(string? value)
  {
    if (value != null) Program.AppendOutput(value);
  }
  public override void WriteLine() => Program.AppendOutput("\n");
  public override void WriteLine(string? value) => Program.AppendOutput((value ?? "") + "\n");
}
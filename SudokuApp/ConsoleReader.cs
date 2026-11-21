public static class ConsoleReader
{
    // Defaults to standard terminal ReadLine when running on desktop/CLI
    public static Func<Task<string?>> ReadLineHandler { get; set; } = 
        () => Task.FromResult(Console.ReadLine());

    public static Task<string?> ReadLineAsync() => ReadLineHandler();
}

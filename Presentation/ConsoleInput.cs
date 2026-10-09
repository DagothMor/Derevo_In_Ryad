namespace Derevo;

internal sealed class ConsoleInput
{
    private readonly TextReader _reader;
    public ConsoleInput(TextReader reader) => _reader = reader;
    public UserInput Read() => Parse(_reader.ReadLine());

    public static UserInput Parse(string? line)
    {
        if (line is null) return new UserInput(InputKind.Exit);
        string text = line.Trim().ToLowerInvariant();
        var kind = text switch
        {
            "r" or "restart" => InputKind.Restart,
            "q" or "exit" => InputKind.Exit,
            "help" or "?" => InputKind.Help,
            "steps" => InputKind.Steps,
            "final" => InputKind.Final,
            "h" or "history" => InputKind.History,
            "hint" => InputKind.Hint,
            _ => InputKind.Invalid
        };
        if (kind != InputKind.Invalid) return new UserInput(kind);
        var parts = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2 && TryPosition(parts[0], out var first) &&
            TryPosition(parts[1], out var second))
            return new UserInput(InputKind.Move, new SwapCells(first, second));
        return new UserInput(InputKind.Invalid);
    }

    private static bool TryPosition(string text, out Position position)
    {
        position = default;
        text = text.ToUpperInvariant();
        if (text.Length != 2 || text[0] < 'A' || text[0] > 'H' ||
            text[1] < '1' || text[1] > '8') return false;
        position = new Position(text[1] - '1', text[0] - 'A');
        return true;
    }
}

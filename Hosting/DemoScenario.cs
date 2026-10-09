namespace Derevo;

internal static class DemoScenario
{
    public static int Run(TextWriter output)
    {
        output.WriteLine("Демонстрация: ход B1 B2, две волны, 90 очков, история и перезапуск.");
        using var input = new StringReader("steps\nB1 B2\nhistory\nrestart\nexit\n");
        using var services = Composition.Build(
            input,
            output,
            sourceFactory: () => new SequenceSource(1, 1, 1, 0, 1, 2),
            boardFactory: CreateBoard);
        return Composition.Run(services);
    }

    private static GameBoard CreateBoard() => GameBoard.FromRows(
        "ABADEABC", "BADEABCD", "CDEABCDE", "DEABCDEA",
        "EABCDEAB", "ABCDEABC", "BCDEABCD", "CDEABCDE");
}

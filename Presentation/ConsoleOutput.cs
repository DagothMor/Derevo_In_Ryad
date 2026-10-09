namespace Derevo;

internal sealed class ConsoleOutput
{
    private readonly TextWriter _writer;
    public bool StepByStep { get; set; }
    public ConsoleOutput(TextWriter writer) => _writer = writer;

    public void Help() => _writer.WriteLine("""
        ТРИ В РЯД
        A1 A2   — переставить соседние клетки
        hint    — подсказка
        steps   — промежуточные состояния
        final   — только итог хода
        history — история и статистика
        restart — новая партия
        exit    — выход
        help    — справка

        Автоматические бонусы: 4 — строка/столбец, 5+ — все элементы
        данного типа, пересечение — строка и столбец общей клетки.
        Очки: 10 × удалённые клетки × номер волны каскада.
        """);

    public void Show(GameFrame frame)
    {
        if (frame.Intermediate && !StepByStep) return;
        _writer.WriteLine();
        _writer.WriteLine(frame.Caption);
        _writer.WriteLine("    A B C D E F G H");
        for (int row = 0; row < frame.Rows.Count; row++)
        {
            string cells = string.Join(" ", frame.Rows[row].Select(c => c.ToString()));
            _writer.WriteLine($"{row + 1,2}  {cells}");
        }
        _writer.WriteLine($"Очки: {frame.Score}. Завершено ходов: {frame.Moves}.");
        if (frame.State == SessionState.Finished)
            _writer.WriteLine("Ходов нет. Партия завершена. restart или exit.");
        else if (!frame.Intermediate)
            _writer.WriteLine("Введите ход или команду:");
    }

    public void Message(string text) => _writer.WriteLine(text);

    public void Summary(GameSession session)
    {
        _writer.WriteLine();
        _writer.WriteLine($"Итог: {session.Score} очков, {session.History.Count} завершённых ходов.");
        foreach (var move in session.History)
            _writer.WriteLine($"{move.Number}. {move.From} ↔ {move.To}: +{move.Points}, волн: {move.Waves}");
    }
}

namespace Derevo;

internal sealed class GameBoard
{
    public const int Size = 8;
    public const char Empty = '.';
    private readonly char[,] _cells = new char[Size, Size];

    public GameBoard()
    {
        foreach (var p in Positions())
            _cells[p.Row, p.Column] = Empty;
    }

    public bool IsFull => Positions().All(p => At(p) != Empty);

    public char At(Position position)
    {
        if (!Inside(position))
            throw new ArgumentOutOfRangeException(nameof(position));
        return _cells[position.Row, position.Column];
    }

    public IReadOnlyList<string> Snapshot()
    {
        var rows = new string[Size];
        for (int row = 0; row < Size; row++)
        {
            var characters = new char[Size];
            for (int column = 0; column < Size; column++)
                characters[column] = _cells[row, column];
            rows[row] = new string(characters);
        }
        return Array.AsReadOnly(rows);
    }

    // Постусловие: заполненное поле без готовых комбинаций.
    public void Initialize(IElementSource source)
    {
        for (int row = 0; row < Size; row++)
        for (int column = 0; column < Size; column++)
        {
            var allowed = new List<char>();
            for (char value = 'A'; value <= 'E'; value++)
            {
                bool horizontal = column >= 2 &&
                    _cells[row, column - 1] == value &&
                    _cells[row, column - 2] == value;
                bool vertical = row >= 2 &&
                    _cells[row - 1, column] == value &&
                    _cells[row - 2, column] == value;
                if (!horizontal && !vertical)
                    allowed.Add(value);
            }
            _cells[row, column] = allowed[source.Next(allowed.Count)];
        }
    }

    // Предусловие: стабильное заполненное поле.
    // Отказ не меняет состояние; успешный обмен создаёт комбинацию.
    public SwapResult TrySwap(Position first, Position second)
    {
        if (!Inside(first) || !Inside(second) ||
            Math.Abs(first.Row - second.Row) +
            Math.Abs(first.Column - second.Column) != 1)
            return SwapResult.InvalidCoordinates;

        RequireStable();
        Exchange(first, second);
        if (FindRuns().Count > 0)
            return SwapResult.Accepted;
        Exchange(first, second);
        return SwapResult.NoCombination;
    }

    public SwapCells? FindMove()
    {
        RequireStable();
        foreach (var first in Positions())
        {
            var neighbours = new[]
            {
                new Position(first.Row, first.Column + 1),
                new Position(first.Row + 1, first.Column)
            };
            foreach (var second in neighbours)
            {
                if (!Inside(second)) continue;
                Exchange(first, second);
                bool found;
                try { found = FindRuns().Count > 0; }
                finally { Exchange(first, second); }
                if (found) return new SwapCells(first, second);
            }
        }
        return null;
    }

    public IReadOnlyList<MatchRun> FindRuns()
    {
        var runs = new List<MatchRun>();
        foreach (bool horizontal in new[] { true, false })
        for (int line = 0; line < Size; line++)
        {
            Position GetPosition(int offset) => horizontal
                ? new Position(line, offset) : new Position(offset, line);
            int start = 0;
            while (start < Size)
            {
                char value = At(GetPosition(start));
                int end = start + 1;
                while (value != Empty && end < Size &&
                       At(GetPosition(end)) == value)
                    end++;
                if (value != Empty && end - start >= 3)
                {
                    var cells = Enumerable.Range(start, end - start)
                        .Select(GetPosition).ToArray();
                    runs.Add(new MatchRun(Array.AsReadOnly(cells), horizontal));
                }
                start = end;
            }
        }
        return runs.AsReadOnly();
    }


    public RemovalPlan PlanRemoval()
    {
        var runs = FindRuns();
        var matched = runs.SelectMany(run => run.Cells).ToHashSet();
        var targets = new HashSet<Position>(matched);
        var descriptions = new List<string>();
        foreach (var run in runs)
        {
            var first = run.Cells[0];
            if (run.Cells.Count >= 5)
            {
                char value = At(first);
                targets.UnionWith(Positions().Where(p => At(p) == value));
                descriptions.Add($"удаление всех {value}");
            }
            else if (run.Cells.Count == 4)
            {
                targets.UnionWith(Positions().Where(p => run.Horizontal
                    ? p.Row == first.Row : p.Column == first.Column));
                descriptions.Add(run.Horizontal
                    ? "очистка строки" : "очистка столбца");
            }
        }

        var horizontal = runs.Where(run => run.Horizontal)
            .SelectMany(run => run.Cells).ToHashSet();
        var vertical = runs.Where(run => !run.Horizontal)
            .SelectMany(run => run.Cells).ToHashSet();
        foreach (var center in horizontal.Intersect(vertical))
        {
            targets.UnionWith(Positions().Where(p =>
                p.Row == center.Row || p.Column == center.Column));
            descriptions.Add("крест: строка и столбец");
        }

        var occupied = targets.Where(p => At(p) != Empty)
            .OrderBy(p => p.Row).ThenBy(p => p.Column).ToArray();
        return new RemovalPlan(Array.AsReadOnly(occupied), matched.Count,
            string.Join(", ", descriptions.Distinct()));
    }

    public void Remove(IEnumerable<Position> positions)
    {
        var targets = positions.Distinct().ToArray();
        if (targets.Any(p => !Inside(p)))
            throw new ArgumentOutOfRangeException(nameof(positions));
        foreach (var p in targets)
            _cells[p.Row, p.Column] = Empty;
    }

    // Постусловие: пустоты сверху; порядок остальных элементов сохранён.
    public void Collapse()
    {
        for (int column = 0; column < Size; column++)
        {
            int writeRow = Size - 1;
            for (int readRow = Size - 1; readRow >= 0; readRow--)
            {
                char value = _cells[readRow, column];
                if (value != Empty)
                    _cells[writeRow--, column] = value;
            }
            while (writeRow >= 0)
                _cells[writeRow--, column] = Empty;
        }
    }

    public void Fill(IElementSource source)
    {
        for (int column = 0; column < Size; column++)
        for (int row = 0; row < Size; row++)
            if (_cells[row, column] == Empty)
                _cells[row, column] = (char)('A' + source.Next(5));
    }

    public static GameBoard FromRows(params string[] rows)
    {
        if (rows.Length != Size || rows.Any(row => row.Length != Size) ||
            rows.Any(row => row.Any(c => c != Empty && (c < 'A' || c > 'E'))))
            throw new ArgumentException("Нужны 8 строк по 8 символов A–E или '.'.");
        var board = new GameBoard();
        foreach (var p in Positions())
            board._cells[p.Row, p.Column] = rows[p.Row][p.Column];
        return board;
    }

    private void RequireStable()
    {
        if (!IsFull || FindRuns().Count != 0)
            throw new InvalidOperationException("Требуется стабильное заполненное поле.");
    }

    private static bool Inside(Position p) =>
        p.Row >= 0 && p.Row < Size && p.Column >= 0 && p.Column < Size;

    private static IEnumerable<Position> Positions()
    {
        for (int row = 0; row < Size; row++)
        for (int column = 0; column < Size; column++)
            yield return new Position(row, column);
    }

    private void Exchange(Position a, Position b)
    {
        (_cells[a.Row, a.Column], _cells[b.Row, b.Column]) =
            (_cells[b.Row, b.Column], _cells[a.Row, a.Column]);
    }
}

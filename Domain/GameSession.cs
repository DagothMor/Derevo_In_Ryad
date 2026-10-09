namespace Derevo;

internal sealed class GameSession
{
    private readonly GameBoard _board;
    private readonly IElementSource _source;
    private readonly EventPipeline _pipeline;
    private readonly List<MoveRecord> _history = new();
    private SwapCells? _pendingMove;
    private long _scoreBeforeMove;
    private int _waves;

    public SessionState State { get; private set; } = SessionState.Initializing;
    public long Score { get; private set; }
    public IReadOnlyList<MoveRecord> History => _history.AsReadOnly();

    public GameSession(GameBoard board, IElementSource source, EventPipeline pipeline)
    {
        _board = board;
        _source = source;
        _pipeline = pipeline;
    }

    public void Start()
    {
        if (State != SessionState.Initializing)
            throw new InvalidOperationException("Сессия уже запущена.");
        if (!_board.IsFull) _board.Initialize(_source);
        if (_board.FindRuns().Count != 0)
            throw new InvalidOperationException("Начальное поле содержит комбинации.");
        SetStableState();
        PublishFrame("Начало партии", false);
    }

    public void Move(SwapCells command)
    {
        if (State != SessionState.AwaitingMove)
        {
            _pipeline.Post(new Notice("Сейчас нельзя выполнить ход."));
            return;
        }
        var result = _board.TrySwap(command.From, command.To);
        if (result != SwapResult.Accepted)
        {
            _pipeline.Post(new Notice(result == SwapResult.InvalidCoordinates
                ? "Нужны две соседние клетки внутри поля."
                : "Комбинации нет. Перестановка отменена."));
            return;
        }
        _pendingMove = command;
        _scoreBeforeMove = Score;
        _waves = 0;
        State = SessionState.Resolving;
        PublishFrame($"Перестановка {command.From} ↔ {command.To}", true);
        _pipeline.Post(new ResolveWave());
    }

    public void Advance()
    {
        if (State != SessionState.Resolving || _pendingMove is null)
            throw new InvalidOperationException("Нет хода для обработки.");
        var plan = _board.PlanRemoval();
        if (plan.MatchedCount == 0)
        {
            _history.Add(new MoveRecord(_history.Count + 1,
                _pendingMove.From, _pendingMove.To, _waves, Score - _scoreBeforeMove));
            _pendingMove = null;
            SetStableState();
            PublishFrame("Ход завершён", false);
            return;
        }
        _waves++;
        if (_waves > 1000)
            throw new InvalidOperationException("Слишком длинный каскад. Проверьте генератор.");


        long points = 10L * plan.Cells.Count * _waves;
        _board.Remove(plan.Cells);
        Score += points;
        string bonus = string.IsNullOrEmpty(plan.BonusDescription)
            ? "" : $" Бонус: {plan.BonusDescription}.";
        PublishFrame($"Волна {_waves}: удалено {plan.Cells.Count}, +{points} очков.{bonus}", true);
        _board.Collapse();
        PublishFrame("Падение элементов", true);
        _board.Fill(_source);
        PublishFrame("Заполнение поля", true);
        _pipeline.Post(new ResolveWave());
    }

    public SwapCells? Hint() => State == SessionState.AwaitingMove
        ? _board.FindMove() : null;

    private void SetStableState() => State = _board.FindMove() is null
        ? SessionState.Finished : SessionState.AwaitingMove;

    private void PublishFrame(string caption, bool intermediate) =>
        _pipeline.Post(new GameFrame(_board.Snapshot(), State, Score,
            _history.Count, caption, intermediate));
}

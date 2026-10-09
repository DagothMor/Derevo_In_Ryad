namespace Derevo;

internal enum InputKind
{
    Move, Restart, Exit, Help, Steps, Final, History, Hint, Invalid
}

internal sealed record UserInput(InputKind Kind, SwapCells? Move = null);

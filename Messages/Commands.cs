namespace Derevo;

internal sealed record StartGame() : IMessage;
internal sealed record SwapCells(Position From, Position To) : IMessage;
internal sealed record ResolveWave() : IMessage;

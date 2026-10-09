namespace Derevo;

internal sealed record MoveRecord(
    int Number, Position From, Position To, int Waves, long Points);

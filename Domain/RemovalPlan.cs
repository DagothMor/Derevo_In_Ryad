namespace Derevo;

internal sealed record RemovalPlan(
    IReadOnlyList<Position> Cells, int MatchedCount, string BonusDescription);

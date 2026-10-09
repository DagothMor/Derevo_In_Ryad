namespace Derevo;

internal readonly record struct Position(int Row, int Column)
{
    public override string ToString() => $"{(char)('A' + Column)}{Row + 1}";
}

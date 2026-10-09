namespace Derevo;

internal sealed class RandomElementSource : IElementSource
{
    private readonly Random _random = new();
    public RandomElementSource() { }
    public int Next(int exclusiveMax) => _random.Next(exclusiveMax);
}

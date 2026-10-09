namespace Derevo;

internal sealed class SequenceSource : IElementSource
{
    private readonly Queue<int> _values;
    public SequenceSource(params int[] values) => _values = new(values);

    public int Next(int exclusiveMax)
    {
        if (!_values.TryDequeue(out int value))
            throw new InvalidOperationException("Исчерпана тестовая последовательность.");
        if (value < 0 || value >= exclusiveMax)
            throw new InvalidOperationException("Тестовый элемент вне диапазона.");
        return value;
    }
}

namespace Derevo;


internal sealed class EventPipeline : IDisposable
{
    private readonly Queue<IMessage> _queue = new();
    private readonly Dictionary<Type, Action<IMessage>> _handlers = new();
    private bool _draining;
    private bool _disposed;

    public EventPipeline() { }

    public void Subscribe<T>(Action<T> handler) where T : IMessage
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _handlers.Add(typeof(T), message => handler((T)message));
    }

    public void Post<T>(T message) where T : IMessage
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _queue.Enqueue(message);
    }

    public void Drain()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_draining)
            throw new InvalidOperationException("Обработка очереди уже выполняется.");
        _draining = true;
        try
        {
            while (_queue.TryDequeue(out var message))
            {
                if (!_handlers.TryGetValue(message.GetType(), out var handler))
                    throw new InvalidOperationException(
                        $"Нет обработчика: {message.GetType().Name}");
                handler(message);
            }
        }
        catch
        {
            _queue.Clear();
            throw;
        }
        finally
        {
            _draining = false;
        }
    }

    public void Dispose()
    {
        _queue.Clear();
        _handlers.Clear();
        _disposed = true;
    }
}

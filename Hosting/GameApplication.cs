using Microsoft.Extensions.DependencyInjection;

namespace Derevo;

internal sealed class GameApplication
{
    private readonly IServiceScopeFactory _scopes;
    private readonly ConsoleInput _input;
    private readonly ConsoleOutput _output;

    public GameApplication(IServiceScopeFactory scopes,
        ConsoleInput input, ConsoleOutput output)
    {
        _scopes = scopes;
        _input = input;
        _output = output;
    }

    public int Run()
    {
        _output.Help();
        while (true)
        {
            // При restart эта область освобождается вместе с очередью.
            using var scope = _scopes.CreateScope();
            var session = scope.ServiceProvider.GetRequiredService<GameSession>();
            var pipeline = scope.ServiceProvider.GetRequiredService<EventPipeline>();
            Composition.Wire(session, pipeline, _output.Show, _output.Message);
            pipeline.Post(new StartGame());
            pipeline.Drain();

            bool restart = false;
            while (!restart)
            {
                var command = _input.Read();
                switch (command.Kind)
                {
                    case InputKind.Exit:
                        _output.Summary(session);
                        return 0;
                    case InputKind.Restart:
                        _output.Summary(session);
                        restart = true;
                        break;
                    case InputKind.Help:
                        _output.Help();
                        break;
                    case InputKind.Steps:
                        _output.StepByStep = true;
                        _output.Message("Включён пошаговый вывод.");
                        break;
                    case InputKind.Final:
                        _output.StepByStep = false;
                        _output.Message("Включён итоговый вывод.");
                        break;
                    case InputKind.History:
                        _output.Summary(session);
                        break;
                    case InputKind.Hint:
                        var hint = session.Hint();
                        _output.Message(hint is null
                            ? "Допустимого хода нет."
                            : $"Попробуй: {hint.From} {hint.To}");
                        break;
                    case InputKind.Move:
                        pipeline.Post(command.Move!);
                        pipeline.Drain();
                        break;
                    default:
                        _output.Message("Неверный ввод. Пример: A1 A2. Справка: help.");
                        break;
                }
            }
        }
    }
}

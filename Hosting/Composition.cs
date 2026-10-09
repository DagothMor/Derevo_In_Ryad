using Microsoft.Extensions.DependencyInjection;

namespace Derevo;

internal static class Composition
{
    public static ServiceProvider Build(
        TextReader? input = null,
        TextWriter? output = null,
        Func<IElementSource>? sourceFactory = null,
        Func<GameBoard>? boardFactory = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ConsoleInput>(_ => new(input ?? Console.In));
        services.AddSingleton<ConsoleOutput>(_ => new(output ?? Console.Out));
        services.AddSingleton<GameApplication>();
        services.AddScoped<IElementSource>(_ => sourceFactory is null
            ? new RandomElementSource() : sourceFactory());
        services.AddScoped<GameBoard>(_ => boardFactory is null
            ? new GameBoard() : boardFactory());
        services.AddScoped<EventPipeline>();
        services.AddScoped<GameSession>();

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });
    }

    public static int Run(IServiceProvider services)
        => services.GetRequiredService<GameApplication>().Run();

    public static void Wire(GameSession session, EventPipeline pipeline,
        Action<GameFrame> show, Action<string> message)
    {
        pipeline.Subscribe<StartGame>(_ => session.Start());
        pipeline.Subscribe<SwapCells>(session.Move);
        pipeline.Subscribe<ResolveWave>(_ => session.Advance());
        pipeline.Subscribe<GameFrame>(show);
        pipeline.Subscribe<Notice>(notice => message(notice.Text));
    }
}

using System.Text;

namespace Derevo;

internal static class Program
{
    public static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        try
        {
            using var services = Composition.Build();
            return Composition.Run(services);
        }
        catch (Exception error)
        {
            Console.Error.WriteLine($"Ошибка: {error.Message}");
            return 1;
        }
    }
}

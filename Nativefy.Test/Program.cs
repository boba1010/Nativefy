namespace Nativefy.Test;

internal class Program
{
    public static async Task Main()
    {
        using var searchEngine = new Nativefy.Engine.SearchEngine();

        var results = await searchEngine.GetResultAsync("Bones Imagine dragons");

        foreach (var search in results)
        {
            Console.WriteLine($"Title: {search.Title}");
            Console.WriteLine($"Url: {search.Url}");
        }
    }
}

using Nativefy.Engine;
using System.Text.Json;

using var nativefy = new NativefyEngine();

const string Id = "UC_x5XG1OV2P6uZZ5FSM9Ttw";

Console.Write("Enter the test name: ");
var method = Console.ReadLine()?.Trim().ToLowerInvariant();

switch (method)
{
    case "channel.videos":
        await ChannelVideos();
        break;

    case "channel.home":
        break;

    case "channel":
        break;

    default:
        Console.WriteLine($"Unknown test: {method}");
        break;
}

async Task ChannelVideos()
{
    var page = await nativefy.Channel.GetVideosSectionAsync(Id);

    var json = JsonSerializer.Serialize(page, new JsonSerializerOptions { WriteIndented = true });

    Console.WriteLine(json);
    Console.WriteLine();

    while (true)
    {
        if (string.IsNullOrEmpty(page.ContinuationToken))
        {
            Console.WriteLine("-- End --");
            return;
        }

        Console.WriteLine("Do you want to continue?");
        var answer = Console.ReadLine();

        answer = answer?.Trim().ToLower();

        if (answer is "y" or "yes")
        {
            page = await nativefy.Channel.GetNextVideosSectionAsync(Id, page.ContinuationToken);
            json = JsonSerializer.Serialize(page, new JsonSerializerOptions { WriteIndented = true });

            Console.WriteLine(json);
        }
        else
            return;
    }
}
using TubeSharp.Channels;

namespace Nativefy.Engine.Channels;

public sealed class Channel
{
    public string Id { get; }
    
    public string Title { get; }
    
    public string ImageUrl { get; }
    
    public string? BannerUrl { get; }
    
    public string? Description { get; }
    
    public string? VideoCount { get; }
    
    public string? Subscribers { get; }

    public string Handle { get; }

    public string Url { get; }

    public bool IsVerified { get; }

    public List<ChannelTab> Tabs { get; } = [];

    public Channel(YoutubeChannel channel)
    {
        Id = channel.Id;
        Title = channel.Name;
        ImageUrl = channel.AvatarUrl;
        Url = channel.Url;
        BannerUrl = channel.BannerUrl;
        Description = channel.Description;
        VideoCount = channel.VideoCount;
        Subscribers = channel.Subscribers;
        Handle = channel.Handle;
        IsVerified = channel.IsVerified;

        foreach (var tab in channel.Tabs)
        {
            if (tab.Title is "Home" or "Videos")
                Tabs.Add(new(tab.Title, tab.BrowseId));
        }
    }
}

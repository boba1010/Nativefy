using CommunityToolkit.Mvvm.ComponentModel;
using Nativefy.Models.Media;
using Nativefy.Services;
using Nativefy.Services.LocalMetadata;
using System.Collections.ObjectModel;

namespace Nativefy.ViewModels;

public sealed partial class HistoryViewModel(IHistoryService history, ILikedMediaService likedMedia) : ObservableObject
{
    private readonly ObservableCollection<HistoryItem> _history = [];

    public ObservableCollection<HistoryItem> FilteredHistory { get; set; } = [];

    public void Filter(string tag)
    {
        FilteredHistory.Clear();

        switch (tag)
        {
            case "Videos":
                foreach (var item in _history)
                {
                    if (item is HistoryMediaItem)
                        FilteredHistory.Add(item);
                }
                break;

            case "Playlists":
                foreach (var item in _history)
                {
                    if (item is HistoryPlaylist)
                        FilteredHistory.Add(item);
                }
                break;

            case "Searches":
                foreach (var item in _history)
                {
                    if (item is SearchHistory)
                        FilteredHistory.Add(item);
                }
                break;

            case "All":
            default:
                foreach (var item in _history)
                    FilteredHistory.Add(item);
                break;
        }
    }

    public void LoadHistory()
    {
        var items = history.GetItems();
        foreach (var item in items)
        {
            _history.Add(item);
            FilteredHistory.Add(item);
        }
    }

    public void LikeMedia(string id)
    {
        foreach (var history in _history)
        {
            if (history is HistoryPlayable playable)
            {
                if (playable.Id != id)
                    continue;

                if (playable is HistoryMediaItem media)
                {
                    likedMedia.AddToLikes(new()
                    {
                        Id = media.Id,
                        Url = media.Url,
                        AddedDate = DateTime.Now,
                        Author = media.Author,
                        Duration = media.Duration,
                        ImageUrl = media.ImageUrl,
                        Title = media.Title,
                    });
                }
            }
        }
    }
}

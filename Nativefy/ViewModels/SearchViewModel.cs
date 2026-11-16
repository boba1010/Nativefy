using ABI.System;
using CommunityToolkit.Mvvm.Messaging;
using Nativefy.Engine;
using Nativefy.Engine.Search;
using Nativefy.Messages;
using Nativefy.Models.Media;
using Nativefy.Services;
using Nativefy.Services.LocalMetadata;
using Nativefy.Services.OnlineMetadata;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Xml.Linq;
using YoutubeExplode.Common;

namespace Nativefy.ViewModels;

public sealed partial class SearchViewModel(
    NativefyEngine nativefy,
    ILikedMediaService likedMedia, 
    ISearchQueriesService search, 
    IHistoryService history,
    IOnlinePlaylistService onlinePlaylist,
    IPlaylistsService playlists) : BaseViewModel
{
    public ObservableCollection<ISearchResult> Results { get; } = [];
    public ObservableCollection<byte> DummyResults { get; } = new(Enumerable.Repeat(0, 100).Select(x => (byte)x));

    public async Task ExecuteSearchAsync(string query)
    {
        Results.Clear();
        IsLoading = true;

        try
        {
            search.SaveQuery(query);
            history.UpdateOrAdd(new SearchHistory
            {
                Id = query,
                Query = query,
            });
            
            var results = await nativefy.Search.GetResultAsync(query, 100);

            foreach (var result in results)
                Results.Add(result);
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task LikeMediaAsync(string id)
    {
        var item = Results.FirstOrDefault(r => r.Id == id) ?? throw new InvalidDataException();

        if (item is Engine.Search.Playlist playlist)
        {
            if (playlists.Exists(playlist.Id))
                return;

            var completePlaylist = await onlinePlaylist.GetPlaylistAsync(playlist.Id);
            playlists.CreatePlaylist(new()
            {
                Id = completePlaylist.Id,
                Title = completePlaylist.Title,
                ImageUrl = completePlaylist.ImageUrl,
                Author = completePlaylist.Author,
                Url = completePlaylist.Url,
                Items = [.. completePlaylist.Items],
            });

            WeakReferenceMessenger.Default.Send(new RefreshPlaylistsMessage());
        }
        else if (item is Video video)
        {
            likedMedia.AddToLikes(new()
            {
                Id = video.Id,
                Url = video.Url,
                AddedDate = DateTime.Now,
                Author = video.Author,
                Duration = video.Duration,
                ImageUrl = video.ImageUrl,
                Title = video.Title,
            });
        }
    }
}

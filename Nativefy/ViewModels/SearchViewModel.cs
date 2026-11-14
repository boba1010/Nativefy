using Nativefy.Engine;
using Nativefy.Engine.Playable;
using Nativefy.Models.Media;
using Nativefy.Services;
using Nativefy.Services.LocalMetadata;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nativefy.ViewModels;

public sealed partial class SearchViewModel(NativefyEngine nativefy, ILikedMediaService likedMedia, ISearchQueriesService search, IHistoryService history) : BaseViewModel
{
    public ObservableCollection<IPlayable> Results { get; } = [];
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
            {
                Results.Add(result switch
                {
                    Engine.Search.Video video => new PlayableMedia(video),
                    Engine.Search.Playlist playlist => new PlayablePlaylist(playlist),
                    _ => throw new NotSupportedException()
                });
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void LikeMedia(string id)
    {
        var item = Results.FirstOrDefault(r => r.Id == id) ?? throw new InvalidDataException();

        if (item is PlayablePlaylist)
            return;

        if (item is PlayableMedia media)
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

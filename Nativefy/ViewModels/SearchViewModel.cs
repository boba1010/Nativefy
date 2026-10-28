using Nativefy.Engine;
using Nativefy.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nativefy.ViewModels;

public sealed partial class SearchViewModel(NativefyEngine nativefy, ILikedMediaService likedMedia) : BaseViewModel
{
    public ObservableCollection<IPlayableItem> Results { get; } = [];
    public ObservableCollection<byte> DummyResults { get; } = new(Enumerable.Repeat(0, 100).Select(x => (byte)x));

    public async Task ExecuteSearchAsync(string query)
    {
        IsLoading = true;

        try
        {
            var results = await nativefy.Search.GetResultAsync(query, 100);

            foreach (var result in results)
                Results.Add(result);
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task LikeMediaAsync(string url)
    {
        var item = Results.FirstOrDefault(r => r.Url == url) ?? throw new InvalidDataException();

        if (item.IsPlaylist || !item.Duration.HasValue)
            return;

        await likedMedia.AddToLikesAsync(new()
        {
            Url = item.Url,
            AddedDate = DateTime.Now,
            Author = item.Author,
            Duration = item.Duration.Value,
            ImageUrl = item.ImageUrl,
            Title = item.Title,
        });
    }
}

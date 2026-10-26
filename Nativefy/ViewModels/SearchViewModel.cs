using Nativefy.Engine;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nativefy.ViewModels;

public sealed partial class SearchViewModel(NativefyEngine nativefy) : BaseViewModel
{
    public ObservableCollection<IPlayableItem> Results { get; } = [];

    public async Task ExecuteSearchAsync(string query)
    {
        var results = await nativefy.Search.GetResultAsync(query, 100);

        foreach (var result in results)
            Results.Add(result);
    }
}

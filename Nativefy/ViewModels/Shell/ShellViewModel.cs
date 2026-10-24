using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nativefy.ViewModels;

public sealed partial class ShellViewModel : ObservableObject
{
    public ObservableCollection<PlaylistNavItem> PlayLists { get; } = [];

    public async Task LoadPlaylistsAsync()
    {
        PlayLists.Add(new()
        {
            Name = "Funk!",
            ImageUrl = "https://fastly.picsum.photos/id/115/200/300.jpg"
        });
    }
}

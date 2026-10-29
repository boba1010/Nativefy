using Nativefy.Models.Media;

namespace Nativefy.Controls;

public sealed partial class NewPlaylistDialog : ContentDialog
{
    public LocalPlaylist? Playlist { get; } = new()
    {
        Name = "MINEEEEEEEEEEEE"
    };

    public NewPlaylistDialog()
    {
        InitializeComponent();
    }
}

using Nativefy.Models.Media;

namespace Nativefy.Controls;

public sealed partial class NewPlaylistDialog : ContentDialog
{
    public LocalPlaylist? Playlist { get; private set; } = new()
    {
        Author = "You",
        Id = Guid.NewGuid().ToString(),
        CreatedAt = DateTime.Now,
        UpdatedAt = DateTime.Now,
    };

    public NewPlaylistDialog()
    {
        InitializeComponent();
    }

    private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var name = PlaylistNameBox.Text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            Playlist = null;
            return;
        }

        Playlist?.Title = name;

        var coverUrl = PlaylistCoverUrlBox.Text.Trim();
        if (!string.IsNullOrEmpty(coverUrl))
            Playlist?.ImageUrl = coverUrl;
    }

    private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        Playlist = null;
    }
}

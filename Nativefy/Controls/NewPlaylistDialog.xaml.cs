namespace Nativefy.Controls;

public sealed partial class NewPlaylistDialog : ContentDialog
{
    public Playlist? Playlist { get; } = new()
    {
        Name = "MINEEEEEEEEEEEE"
    };

    public NewPlaylistDialog()
    {
        InitializeComponent();
    }
}

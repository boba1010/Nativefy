using Nativefy.Models.Media;

namespace Nativefy.Messages;

public sealed record SearchMessage(string Query);

public sealed record RefreshPlaylistsMessage();

public sealed record PlayPlaylistMessage(Playlist Playlist);

public sealed record PlayItemMessage(MediaItem Item);

public sealed record NavigationMessage(Type Page);

public sealed record PlayerBarVisiblityChangedMessage(bool IsVisible);

public sealed record NotificationMessage(Notification Notification);
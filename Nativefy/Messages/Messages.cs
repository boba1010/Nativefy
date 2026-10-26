using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Nativefy.Messages;

public enum DialogResult { None, Primary, Exit }

public sealed record DialogResponse(DialogResult Result, object? Data = null)
{
    public T? GetData<T>() => Data is T value ? value : default;
}

public sealed class OpenNewPlaylistDialogMessage(
    string title,
    string primaryButton = "OK",
    string? exitButton = null) : AsyncRequestMessage<DialogResponse>
{
    public string Title => title;
    public string? PrimaryButton => primaryButton;
    public string? ExitButton => exitButton;
}

public sealed record SearchMessage(string Query);
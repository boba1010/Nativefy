using CommunityToolkit.Mvvm.ComponentModel;

namespace Nativefy.Models;

public enum Severity
{
    Info,

    Warning,

    Success,

    Failure,
}

public sealed partial class Notification : ObservableObject
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Title { get; set; } = null!;

    [ObservableProperty]
    public partial string Message { get; set; } = null!;

    public Severity Severity { get; set; }

    [ObservableProperty]
    public partial bool Read { get; set; } = false;

    public Notification() { }

    public Notification(NotificationDto notification)
    {
        Id = notification.Id;
        Title = notification.Title;
        Message = notification.Message;
        Read = notification.Read;
    }
}

public sealed record NotificationDto(string Id, string Title, string Message, bool Read, Severity Severity)
{
    public NotificationDto(Notification notification) 
        : this(notification.Id, notification.Title, notification.Message, notification.Read, notification.Severity) { }
}
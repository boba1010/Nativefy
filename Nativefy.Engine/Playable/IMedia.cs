namespace Nativefy.Engine.Playable;

public interface IMedia : IPlayable
{
    TimeSpan Duration { get; }
}

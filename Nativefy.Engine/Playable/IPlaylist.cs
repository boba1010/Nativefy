namespace Nativefy.Engine.Playable;

public interface IPlaylist : IPlayable
{
    public List<IMedia> Media { get; }
}
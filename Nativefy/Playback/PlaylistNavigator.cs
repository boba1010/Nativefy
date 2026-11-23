using Nativefy.Controls;
using Nativefy.Models.Media;

namespace Nativefy.Playback;

public sealed class PlaylistNavigator
{
    private readonly List<int> _shuffleOrder = [];
    private int _shufflePosition = -1;

    public bool ShuffleEnabled { get; set; }

    public RepeatState RepeatState { get; set; }

    public void Reset()
    {
        _shuffleOrder.Clear();
        _shufflePosition = -1;
    }

    private void InitializeShuffle(int count, int currentIndex)
    {
        _shuffleOrder.Clear();
        _shuffleOrder.Add(currentIndex);

        List<int> remaining = [];

        for (int i = 0; i < count; i++)
        {
            if (i != currentIndex)
                remaining.Add(i);
        }

        // Fisher-Yates shuffle.
        for (int i = remaining.Count - 1; i > 0; i--)
        {
            int j = Random.Shared.Next(i + 1);
            (remaining[i], remaining[j]) = (remaining[j], remaining[i]);
        }

        _shuffleOrder.AddRange(remaining);
        _shufflePosition = 0;
    }

    private void EnsureShuffle(int count, int currentIndex)
    {
        if (_shuffleOrder.Count != count ||
            _shufflePosition < 0 ||
            _shuffleOrder[_shufflePosition] != currentIndex)
        {
            InitializeShuffle(count, currentIndex);
        }
    }

    public int GetNextIndex(Playlist? playlist, int currentIndex)
    {
        if (playlist is null || playlist.Items.Count == 0)
            return -1;

        int count = playlist.Items.Count;

        if (ShuffleEnabled && count > 1)
        {
            EnsureShuffle(count, currentIndex);

            if (_shufflePosition < count - 1)
                return _shuffleOrder[++_shufflePosition];

            if (RepeatState != RepeatState.RepeatAll)
                return -1;

            int previousIndex = currentIndex;

            InitializeShuffle(count, previousIndex);

            return _shuffleOrder[++_shufflePosition];
        }

        int nextIndex = currentIndex + 1;

        if (nextIndex >= count)
        {
            if (RepeatState != RepeatState.RepeatAll)
                return -1;

            nextIndex = 0;
        }

        return nextIndex;
    }

    public int GetPreviousIndex(Playlist? playlist, int currentIndex)
    {
        if (playlist is null || playlist.Items.Count == 0)
            return -1;

        int count = playlist.Items.Count;

        if (ShuffleEnabled && count > 1)
        {
            EnsureShuffle(count, currentIndex);

            if (_shufflePosition > 0)
                return _shuffleOrder[--_shufflePosition];

            if (RepeatState != RepeatState.RepeatAll)
                return -1;

            _shufflePosition = count - 1;
            return _shuffleOrder[_shufflePosition];
        }

        int previousIndex = currentIndex - 1;

        if (previousIndex < 0)
        {
            if (RepeatState != RepeatState.RepeatAll)
                return -1;

            previousIndex = count - 1;
        }

        return previousIndex;
    }
}
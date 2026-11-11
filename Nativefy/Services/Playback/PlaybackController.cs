using LibVLCSharp.Shared;
using Nativefy.Controls;
using Nativefy.Engine;
using Nativefy.Models.Media;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using VlcMedia = LibVLCSharp.Shared.Media;
using VlcPlayer = LibVLCSharp.Shared.MediaPlayer;

namespace Nativefy.Services.Playback;

public enum PlayerState
{
    Playing,
    Paused
}

public sealed class PlaybackController(IStreamingBackend backend, VideoCache cache)
{
    private readonly object _gate = new();

    private readonly List<MediaItem> _items = [];
    private int[] _order = [];
    private int _orderPos = -1;
    private bool _isShuffleOn;

    private LibVLC? _libVlc;
    private VlcPlayer? _player;
    private int _volume = 100;
    private bool _hasMedia;
    private bool _startPaused;
    private bool _seekPending;

    private (TimeSpan? StartAt, bool Play)? _deferredStart;

    private string? _retriedItemId;

    private CancellationTokenSource? _loadCts;
    private CancellationTokenSource? _resolutionLoadCts;
    private Resolution _resolution = Resolution.Standard360p;

    public VlcPlayer? Player => _player;
    public IReadOnlyList<MediaItem> Queue => _items;

    public RepeatState RepeatState { get; set; }

    public bool IsShuffleOn
    {
        get => _isShuffleOn;
        set
        {
            if (_isShuffleOn == value)
                return;

            _isShuffleOn = value;

            lock (_gate)
                BuildOrder(CurrentItemIndexLocked());
        }
    }

    public bool IsPlaying { get; private set; }

    public TimeSpan Duration { get; private set; }
    public TimeSpan Position { get; private set; }

    public Resolution Resolution
    {
        get => _resolution;
        private set
        {
            if (_resolution == value)
                return;

            _resolution = value;
            ResolutionChanged?.Invoke();
        }
    }

    public uint QueueCount
    {
        get { lock (_gate) return (uint)_items.Count; }
    }

    public uint CurrentIndex
    {
        get
        {
            lock (_gate)
            {
                int index = CurrentItemIndexLocked();
                return index < 0 ? uint.MaxValue : (uint)index;
            }
        }
    }

    public MediaItem? CurrentItem
    {
        get
        {
            lock (_gate)
            {
                int index = CurrentItemIndexLocked();
                return index < 0 ? null : _items[index];
            }
        }
    }

    public event Action<PlayerState>? StateChanged;
    public event Action<RepeatState>? RepeatStateChanged;
    public event Action<TimeSpan>? PositionChanged;
    public event Action<MediaItem>? CurrentItemChanged;
    public event Action? QueueChanged;
    public event Action? SeekCompleted;
    public event Action? ResolutionChanged;
    public event Action<Resolution[]>? AvailableResolutionsChanged;
    public event Action<Exception>? PlaybackFailed;

    // ───────────────────────── Player lifetime ─────────────────────────

    public void InitializeVideo(string[] swapChainOptions)
    {
        (TimeSpan? StartAt, bool Play)? resume = _deferredStart;

        lock (_gate)
        {
            if (resume is null && _hasMedia)
                resume = (Position, IsPlaying);

            DisposePlayerLocked();

            _libVlc = new LibVLC(swapChainOptions);
            AttachPlayerLocked(_libVlc);

            _deferredStart = null;
        }

        if (resume is { } r && CurrentItem is not null)
            _ = LoadCurrentAsync(r.StartAt, r.Play);
    }

    private void AttachPlayerLocked(LibVLC libVlc)
    {
        var player = new VlcPlayer(libVlc) { Volume = _volume };

        player.Playing += Player_Playing;
        player.Paused += Player_Paused;
        player.Stopped += Player_Stopped;
        player.EndReached += Player_EndReached;
        player.EncounteredError += Player_EncounteredError;
        player.TimeChanged += Player_TimeChanged;
        player.LengthChanged += Player_LengthChanged;

        _player = player;
    }

    private void DisposePlayerLocked()
    {
        VlcPlayer? player = _player;
        LibVLC? libVlc = _libVlc;

        _player = null;
        _libVlc = null;
        _hasMedia = false;

        if (player is not null)
        {
            player.Playing -= Player_Playing;
            player.Paused -= Player_Paused;
            player.Stopped -= Player_Stopped;
            player.EndReached -= Player_EndReached;
            player.EncounteredError -= Player_EncounteredError;
            player.TimeChanged -= Player_TimeChanged;
            player.LengthChanged -= Player_LengthChanged;
        }

        if (player is not null || libVlc is not null)
        {
            Task.Run(() =>
            {
                player?.Dispose();
                libVlc?.Dispose();
            });
        }
    }

    private void Player_Playing(object? sender, EventArgs e)
    {
        if (_startPaused)
        {
            _startPaused = false;
            VlcPlayer? player = _player;
            Task.Run(() => player?.SetPause(true));
            return;
        }

        SetPlaying(true);
    }

    private void Player_Paused(object? sender, EventArgs e) => SetPlaying(false);

    private void Player_Stopped(object? sender, EventArgs e) => SetPlaying(false);

    private void Player_TimeChanged(object? sender, MediaPlayerTimeChangedEventArgs e)
    {
        Position = TimeSpan.FromMilliseconds(e.Time);
        PositionChanged?.Invoke(Position);

        if (_seekPending)
        {
            _seekPending = false;
            SeekCompleted?.Invoke();
        }
    }

    private void Player_LengthChanged(object? sender, MediaPlayerLengthChangedEventArgs e)
    {
        Duration = TimeSpan.FromMilliseconds(e.Length);
    }

    private void Player_EndReached(object? sender, EventArgs e)
    {
        Task.Run(OnEndReachedAsync);
    }

    private void Player_EncounteredError(object? sender, EventArgs e)
    {
        Debug.WriteLine("LibVLC playback error.");

        MediaItem? item = CurrentItem;

        if (item is null)
            return;

        if (_retriedItemId == item.Id)
        {
            SetPlaying(false);
            return;
        }

        _retriedItemId = item.Id;

        TimeSpan resumeAt = Position;

        Task.Run(() => LoadCurrentAsync(resumeAt, play: true, forceRefresh: true));
    }

    private async Task OnEndReachedAsync()
    {
        SetPlaying(false);

        if (RepeatState == RepeatState.RepeatOne)
        {
            await LoadCurrentAsync(TimeSpan.Zero, play: true);
            return;
        }

        int next;

        lock (_gate)
        {
            next = _orderPos + 1;

            if (next >= _order.Length)
            {
                if (RepeatState != RepeatState.RepeatAll || _order.Length == 0)
                    return;

                next = 0;
            }
        }

        await MoveToAsync(next, play: true);
    }

    private void SetPlaying(bool value)
    {
        if (IsPlaying == value)
            return;

        IsPlaying = value;

        StateChanged?.Invoke(value ? PlayerState.Playing : PlayerState.Paused);
    }

    // ───────────────────────── Queue ─────────────────────────

    private int CurrentItemIndexLocked() =>
        _orderPos >= 0 && _orderPos < _order.Length ? _order[_orderPos] : -1;

    // Rebuilds the play order. currentItemIndex (index into _items) stays current, and goes first when shuffling.
    private void BuildOrder(int currentItemIndex)
    {
        _order = new int[_items.Count];

        for (int i = 0; i < _order.Length; i++)
            _order[i] = i;

        if (_isShuffleOn && _order.Length > 1)
        {
            Random.Shared.Shuffle(_order);

            if (currentItemIndex >= 0)
            {
                int at = Array.IndexOf(_order, currentItemIndex);
                (_order[0], _order[at]) = (_order[at], _order[0]);
                _orderPos = 0;
            }
            else
                _orderPos = -1;
        }
        else
            _orderPos = currentItemIndex;
    }

    private void SetQueue(IEnumerable<MediaItem> items)
    {
        _loadCts?.Cancel();

        lock (_gate)
        {
            _items.Clear();
            _items.AddRange(items);
            BuildOrder(-1);
        }

        _deferredStart = null;

        QueueChanged?.Invoke();
    }

    private async Task MoveToAsync(int orderPos, bool play)
    {
        MediaItem item;

        lock (_gate)
        {
            if (orderPos < 0 || orderPos >= _order.Length)
                return;

            _orderPos = orderPos;
            item = _items[_order[orderPos]];
        }

        _retriedItemId = null;
        Position = TimeSpan.Zero;
        Duration = TimeSpan.Zero;

        _ = NotifyCurrentItemChangedAsync(item);

        await LoadCurrentAsync(TimeSpan.Zero, play);
    }

    private async Task NotifyCurrentItemChangedAsync(MediaItem item)
    {
        CurrentItemChanged?.Invoke(item);

        try
        {
            Resolution[] resolutions = await GetCurrentVideoResolutionAsync();
            AvailableResolutionsChanged?.Invoke(resolutions);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    public Task PlayPlaylistAsync(Playlist playlist)
    {
        ArgumentNullException.ThrowIfNull(playlist);

        SetQueue(playlist.Items);

        return playlist.Items.Count == 0 ? Task.CompletedTask : MoveToAsync(0, play: true);
    }

    public Task PlayItemAsync(MediaItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        SetQueue([item]);

        return MoveToAsync(0, play: true);
    }

    public void ClearQueue()
    {
        cache.Close();
        _loadCts?.Cancel();

        lock (_gate)
        {
            _items.Clear();
            _order = [];
            _orderPos = -1;
        }

        _deferredStart = null;
        _hasMedia = false;

        _player?.Stop();
        SetPlaying(false);

        QueueChanged?.Invoke();

        Position = TimeSpan.Zero;
        Duration = TimeSpan.Zero;
    }

    public void SkipNext()
    {
        int next;

        lock (_gate)
        {
            if (_order.Length == 0)
                return;

            next = _orderPos + 1;

            if (next >= _order.Length)
            {
                if (RepeatState != RepeatState.RepeatAll)
                    return;

                next = 0;
            }
        }

        _ = MoveToAsync(next, play: true);
    }

    public void SkipPrevious()
    {
        int previous;

        lock (_gate)
        {
            if (_order.Length == 0)
                return;

            previous = _orderPos - 1;

            if (previous < 0)
            {
                if (RepeatState != RepeatState.RepeatAll)
                    previous = -1;
                else
                    previous = _order.Length - 1;
            }
        }

        if (previous < 0)
        {
            Seek(TimeSpan.Zero); // already at the first track: restart it
            return;
        }

        _ = MoveToAsync(previous, play: true);
    }

    public void ToggleRepeat()
    {
        RepeatState previousState = RepeatState;

        RepeatState = RepeatState switch
        {
            RepeatState.Off => RepeatState.RepeatAll,
            RepeatState.RepeatAll => RepeatState.RepeatOne,
            RepeatState.RepeatOne => RepeatState.Off,
            _ => RepeatState.Off
        };

        if (previousState != RepeatState)
            RepeatStateChanged?.Invoke(RepeatState);
    }

    public void ToggleShuffle() => IsShuffleOn = !IsShuffleOn;

    // ───────────────────────── Loading / playback ─────────────────────────

    private async Task LoadCurrentAsync(TimeSpan? startAt, bool play, bool forceRefresh = false)
    {
        MediaItem? item = CurrentItem;

        if (item is null)
            return;

        _loadCts?.Cancel();

        var cts = new CancellationTokenSource();
        _loadCts = cts;
        CancellationToken token = cts.Token;

        if (_player is null || _libVlc is null)
        {
            _deferredStart = (startAt, play);
            return;
        }

        Resolution resolution = Resolution; // captured: the user may switch while this runs

        try
        {
            if (forceRefresh)
                cache.Remove(item.Id, resolution); // the cached data may be what's broken

            // Fully cached (or already open): no backend call needed.
            if (!cache.TryGetLocal(item.Id, resolution, out StreamUrls? playUrls))
            {
                StreamUrls remote = await backend.GetStreamUrlsAsync(item, resolution, token);

                token.ThrowIfCancellationRequested();

                playUrls = cache.Open(item.Id, resolution, remote);
            }

            token.ThrowIfCancellationRequested();

            if (!ReferenceEquals(CurrentItem, item))
                return;

            StartPlayback(playUrls, startAt, play);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to start playback: {ex}");
            SetPlaying(false);
            PlaybackFailed?.Invoke(ex);
        }
    }

    private void StartPlayback(StreamUrls urls, TimeSpan? startAt, bool play)
    {
        lock (_gate)
        {
            if (_player is null || _libVlc is null)
                return;

            using var media = new VlcMedia(_libVlc, new Uri(urls.Video!));

            if (!string.IsNullOrEmpty(urls.Audio))
                media.AddOption($":input-slave={urls.Audio.Replace("#", "%23")}");

            if (startAt is { } start && start > TimeSpan.Zero)
                media.AddOption($":start-time={start.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture)}");

            _startPaused = !play;
            _hasMedia = true;
            _seekPending = false;

            _player.Play(media);
        }
    }

    public async Task SetResolutionAsync(Resolution resolution)
    {
        if (Resolution == resolution)
            return;

        Resolution = resolution;

        if (CurrentItem is null)
            return;

        _retriedItemId = null;

        // Reload the same track at the new resolution and resume where it was.
        await LoadCurrentAsync(Position, play: IsPlaying);
    }

    public async Task<Resolution[]> GetCurrentVideoResolutionAsync()
    {
        _resolutionLoadCts?.Cancel();

        var cts = new CancellationTokenSource();
        _resolutionLoadCts = cts;

        MediaItem? item = CurrentItem;

        if (item is null)
            return [];

        Resolution[] resolutions = await backend.GetResolutionsAsync(item.Id, cts.Token);

        cts.Token.ThrowIfCancellationRequested();

        return resolutions;
    }

    // ───────────────────────── Transport ─────────────────────────

    public void Play()
    {
        VlcPlayer? player = _player;

        if (player is null)
        {
            if (CurrentItem is not null)
                _deferredStart = (_deferredStart?.StartAt, true);

            return;
        }

        if (!_hasMedia)
        {
            _ = LoadCurrentAsync(TimeSpan.Zero, play: true);
            return;
        }

        player.Play();
        SetPlaying(true);
    }

    public void Pause()
    {
        if (_player is null)
        {
            if (_deferredStart is { } d)
                _deferredStart = (d.StartAt, false);

            return;
        }

        _player.SetPause(true);
        SetPlaying(false);
    }

    public void TogglePlayPause()
    {
        if (IsPlaying)
            Pause();
        else
            Play();
    }

    public void Seek(TimeSpan position)
    {
        VlcPlayer? player = _player;

        if (player is null)
            return;

        Debug.WriteLine($"IsSeekable: {player.IsSeekable}");

        _seekPending = true;
        player.Time = (long)position.TotalMilliseconds;

        Position = position;
    }

    public void SetVolume(double volume)
    {
        _volume = (int)Math.Clamp(volume, 0, 100);

        _player?.Volume = _volume;
    }
}
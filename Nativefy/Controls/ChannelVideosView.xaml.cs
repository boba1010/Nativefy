namespace Nativefy.Controls;

public sealed partial class ChannelVideosView : UserControl
{
    public object Videos
    {
        get => GetValue(VideosProperty);
        set => SetValue(VideosProperty, value);
    }

    private static readonly DependencyProperty VideosProperty =
        DependencyProperty.Register(nameof(Videos), typeof(object), typeof(ChannelVideosView), new PropertyMetadata(null));

    public Tab Tab
    {
        get => (Tab)GetValue(TabProperty);
        set => SetValue(TabProperty, value);
    }

    private static readonly DependencyProperty TabProperty =
        DependencyProperty.Register(nameof(Tab), typeof(Tab), typeof(ChannelHomeView), new PropertyMetadata(null));

    public event Action<string>? PlayClick;

    public ChannelVideosView()
    {
        InitializeComponent();
    }

    private void Play_Clicked(object sender, RoutedEventArgs e)
    {
        var tag = (sender as Button)?.Tag;
        if (tag is null)
            return;

        PlayClick?.Invoke((string)tag);
    }
}

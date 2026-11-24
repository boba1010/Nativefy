namespace Nativefy.Controls;

public sealed partial class ChannelHomeView : UserControl
{
    public object Shelves
    {
        get => GetValue(ShelvesProperty);
        set => SetValue(ShelvesProperty, value);
    }

    private static readonly DependencyProperty ShelvesProperty =
        DependencyProperty.Register(nameof(Shelves), typeof(object), typeof(ChannelHomeView), new PropertyMetadata(null));

    public Tab Tab
    {
        get => (Tab)GetValue(TabProperty);
        set => SetValue(TabProperty, value);
    }

    private static readonly DependencyProperty TabProperty =
        DependencyProperty.Register(nameof(Tab), typeof(Tab), typeof(ChannelHomeView), new PropertyMetadata(null));

    public event Action<string>? PlayClick;

    public ChannelHomeView()
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

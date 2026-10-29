using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Nativefy.Controls;

public sealed partial class LibraryNavigationView : UserControl
{
    private bool resizing;
    private double startX;
    private double startWidth;

    public ObservableCollection<SidebarGroup> NavigationGroups { get; } = [];

    public object? Header { get; set; }
    public object? InnerContent { get; set; }

    public static readonly DependencyProperty IsPaneOpenProperty =
        DependencyProperty.Register(nameof(IsPaneOpen), typeof(bool), typeof(LibraryNavigationView), new PropertyMetadata(true, OnIsPaneOpenChanged));

    public bool IsPaneOpen
    {
        get => (bool)GetValue(IsPaneOpenProperty);
        set => SetValue(IsPaneOpenProperty, value);
    }

    public static readonly DependencyProperty OpenPaneLengthProperty =
        DependencyProperty.Register(nameof(OpenPaneLength), typeof(double), typeof(LibraryNavigationView), 
            new PropertyMetadata(300D, OnOpenPaneLengthChanged));

    public double OpenPaneLength
    {
        get => (double)GetValue(OpenPaneLengthProperty);
        set => SetValue(OpenPaneLengthProperty, value);
    }

    public static readonly DependencyProperty AddCommandProperty =
        DependencyProperty.Register(nameof(AddCommand), typeof(ICommand), typeof(LibraryNavigationView),
            new PropertyMetadata(null, OnAddCommandChanged));

    public ICommand? AddCommand
    {
        get => (ICommand?)GetValue(AddCommandProperty);
        set => SetValue(AddCommandProperty, value);
    }

    public event EventHandler<string>? ItemInvoked;

    public LibraryNavigationView()
    {
        InitializeComponent();
        InitializeDefaultStructure();
        Loaded += (s, e) => SyncLayoutState(IsPaneOpen);
    }

    private static void OnAddCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LibraryNavigationView control)
            foreach (var group in control.NavigationGroups.Where(g => g.GroupTag == "PlaylistGroup"))
                group.CompactCommand = e.NewValue as ICommand;
    }

    private static void OnIsPaneOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LibraryNavigationView control && e.NewValue is bool isOpen)
            control.SyncLayoutState(isOpen);
    }

    private static void OnOpenPaneLengthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LibraryNavigationView control && e.NewValue is double newWidth && control.IsPaneOpen)
            control.SidebarColumn.Width = new GridLength(newWidth);
    }

    private void InitializeDefaultStructure()
    {
        var libraryGroup = new SidebarGroup { HeaderText = "Library", GroupTag = "LibraryGroup" };
        libraryGroup.Items.Add(new() { Tag = "LikedMedia", Text = "Liked Media", Glyph = "\uEB51" }); // Heart Icon
        libraryGroup.Items.Add(new() { Tag = "WatchLater", Text = "Watch Later", Glyph = "\uE917" }); // Clock Icon
        libraryGroup.Items.Add(new() { Tag = "LikedAuthors", Text = "Liked Authors", Glyph = "\uE716" }); // People Icon
        NavigationGroups.Add(libraryGroup);

        var playlistGroup = new SidebarGroup 
        { 
            HeaderText = "My Playlists", 
            GroupTag = "PlaylistGroup", 
            CompactGlyph = "\uE710",
        };
        NavigationGroups.Add(playlistGroup);
    }

    public void CreateNewGroup(string groupTag, string title)
    {
        NavigationGroups.Add(new SidebarGroup { GroupTag = groupTag, HeaderText = title });
        SyncLayoutState(IsPaneOpen);
    }

    public void AddItemToGroup(string groupTag, string itemTag, string text, string glyph = null!, string imageUrl = null!)
    {
        var group = NavigationGroups.FirstOrDefault(g => g.GroupTag == groupTag);
        group?.Items.Add(new SidebarMenuItem { Tag = itemTag, Text = text, Glyph = glyph, ImageUrl = imageUrl });
        SyncLayoutState(IsPaneOpen);
    }

    public void ClearGroupItems(string groupTag)
    {
        var group = NavigationGroups.FirstOrDefault(g => g.GroupTag == groupTag);
        group?.Items.Clear();
        SyncLayoutState(IsPaneOpen);
    }

    private void OnGroupHeaderClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string tag)
        {
            var targetGroup = NavigationGroups.FirstOrDefault(g => g.GroupTag == tag);
            if (targetGroup != null)
            {
                if (!IsPaneOpen)
                {
                    IsPaneOpen = true;
                    targetGroup.IsExpanded = true;
                }
                else
                    targetGroup.IsExpanded = !targetGroup.IsExpanded;

                SyncLayoutState(IsPaneOpen);
                ItemInvoked?.Invoke(this, tag);
            }
        }
    }

    private void OnSubmenuItemClicked(object sender, RoutedEventArgs e) =>
        ItemInvoked?.Invoke(this, ((Button)sender).Tag?.ToString() ?? string.Empty);

    private void SyncLayoutState(bool isOpen)
    {
        var textVisibility = isOpen ? Visibility.Visible : Visibility.Collapsed;
        var separatorVisibility = isOpen ? Visibility.Collapsed : Visibility.Visible;
        var alignment = isOpen ? HorizontalAlignment.Left : HorizontalAlignment.Center;

        SidebarColumn.Width = isOpen ? new GridLength(OpenPaneLength) : new GridLength(60);

        for (int i = 0; i < NavigationGroups.Count; i++)
        {
            var groupContainer = NavigationGroupsControl.ContainerFromIndex(i) as ContentPresenter;
            if (groupContainer == null) 
                continue;

            var group = NavigationGroups[i];

            if (FindVisualChildByName(groupContainer, "HeaderFullContent") is Grid fullContent)
                fullContent.Visibility = textVisibility;

            if (FindVisualChildByName(groupContainer, "HeaderSeparatorLine") is Border separatorLine)
                separatorLine.Visibility = separatorVisibility;

            var showCompact = !isOpen && group.HasCompactMode;

            if (FindVisualChildByName(groupContainer, "GroupHeaderButton") is Button headerBtn)
            {
                headerBtn.Visibility = showCompact ? Visibility.Collapsed : Visibility.Visible;
                headerBtn.IsEnabled = isOpen;
                headerBtn.Height = isOpen ? 40 : 20;
            }

            if (FindVisualChildByName(groupContainer, "HeaderCompactContent") is StackPanel compactView)
                compactView.Visibility = showCompact ? Visibility.Visible : Visibility.Collapsed;

            if (FindVisualChildByName(groupContainer, "GroupItemsControl") is ItemsControl childItemsControl)
            {
                childItemsControl.Visibility = (isOpen && !group.IsExpanded) ? Visibility.Collapsed : Visibility.Visible;
                childItemsControl.Padding = isOpen ? new Thickness(16, 0, 0, 0) : new Thickness(0);

                for (int j = 0; j < group.Items.Count; j++)
                {
                    var childContainer = childItemsControl.ContainerFromIndex(j) as ContentPresenter;
                    if (childContainer == null) 
                        continue;

                    if (FindVisualChild<Button>(childContainer) is Button subBtn)
                        subBtn.HorizontalContentAlignment = alignment;

                    if (FindVisualChildByName(childContainer, "ItemText") is TextBlock subTxt)
                        subTxt.Visibility = textVisibility;
                }
            }
        }
    }

    private void OnItemTextLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is TextBlock tb)
            tb.Visibility = IsPaneOpen ? Visibility.Visible : Visibility.Collapsed;
    }

    private T? FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
        {
            var child = VisualTreeHelper.GetChild(obj, i);
            if (child is T target) 
                return target;

            var nested = FindVisualChild<T>(child);
            if (nested != null) 
                return nested;
        }
        return null;
    }

    private FrameworkElement? FindVisualChildByName(DependencyObject obj, string name)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
        {
            var child = VisualTreeHelper.GetChild(obj, i);
            if (child is FrameworkElement fe && fe.Name == name) 
                return fe;
            
            var nested = FindVisualChildByName(child, name);
            if (nested != null) 
                return nested;
        }
        return null;
    }

    private void ResizeGrip_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        resizing = true;
        startX = e.GetCurrentPoint(this).Position.X;
        startWidth = IsPaneOpen ? OpenPaneLength : 60;
        resizeGrip.CapturePointer(e.Pointer);
    }

    private void ResizeGrip_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!resizing) 
            return;

        if (!IsPaneOpen) 
            IsPaneOpen = true;

        var currentX = e.GetCurrentPoint(this).Position.X;
        var delta = currentX - startX;
        var newWidth = startWidth + delta;

        if (newWidth < 250) newWidth = 250;
        if (newWidth > 480) newWidth = 480;

        OpenPaneLength = newWidth;
    }

    private void ResizeGrip_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        resizing = false;
        resizeGrip.ReleasePointerCapture(e.Pointer);
    }

    private void ResizeGrip_PointerEntered(object sender, PointerRoutedEventArgs e) =>
        resizeGrip.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 30, 30, 30));

    private void ResizeGrip_PointerExited(object sender, PointerRoutedEventArgs e) =>
        resizeGrip.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
}

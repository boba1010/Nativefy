using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.ObjectModel;

namespace Nativefy.Controls;

public sealed partial class LibraryNavigationView : UserControl
{
    private bool resizing;
    private double startX;
    private double startWidth;
    private int _layoutUpdateId = 0;

    public object? Header { get; set; }
    public object? Footer { get; set; }
    public object? InnerContent { get; set; }

    public object? MenuItemsSource
    {
        get => GetValue(MenuItemsSourceProperty);
        set => SetValue(MenuItemsSourceProperty, value);
    }

    public static readonly DependencyProperty MenuItemsSourceProperty =
        DependencyProperty.Register(
            nameof(MenuItemsSource),
            typeof(object),
            typeof(LibraryNavigationView),
            new PropertyMetadata(null));

    public object SelectedItem
    {
        get => navView.SelectedItem;
        set => navView.SelectedItem = value;
    }

    public static readonly DependencyProperty IsPaneOpenProperty =
    DependencyProperty.Register(
        nameof(IsPaneOpen),
        typeof(bool),
        typeof(LibraryNavigationView),
        new PropertyMetadata(true, OnIsPaneOpenDependencyPropertyChanged));

    public bool IsPaneOpen
    {
        get => (bool)GetValue(IsPaneOpenProperty);
        set => SetValue(IsPaneOpenProperty, value);
    }

    public double OpenPaneLength
    {
        get => navView.OpenPaneLength;
        set => navView.OpenPaneLength = value;
    }

    public NavigationViewPaneDisplayMode PaneDisplayMode
    {
        get => navView.PaneDisplayMode;
        set => navView.PaneDisplayMode = value;
    }


    public event TypedEventHandler<NavigationView, NavigationViewItemInvokedEventArgs>? ItemInvoked;
    public event TypedEventHandler<NavigationView, NavigationViewSelectionChangedEventArgs>? ItemChanged;

    public LibraryNavigationView()
    {
        InitializeComponent();

        Loaded += LibraryNavigationView_Loaded;
    }

    private void LibraryNavigationView_Loaded(object sender, RoutedEventArgs e)
    {
        if (MenuItemsSource == null)
            return;

        if (MenuItemsSource is ObservableCollection<PlaylistNavItem> playLists)
            RenderPlayListsSection(playLists);
    }

    private static void RenderPlayList(PlaylistNavItem playList, NavigationViewItem playlistsSection)
    {
        var item = new NavigationViewItem
        {
            DataContext = playList,
            Content = new Grid()
        };

        var grid = (Grid)item.Content;

        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var playlistImage = new Image
        {
            Source = new BitmapImage(new Uri(playList.ImageUrl)),
            Width = 32,
            Height = 32,
            Margin = new Thickness(0, 0, 8, 0)
        };

        var playlistText = new TextBlock
        {
            Text = playList.Name,
            VerticalAlignment = VerticalAlignment.Center
        };

        Grid.SetColumn(playlistImage, 0);
        Grid.SetColumn(playlistText, 1);

        grid.Children.Add(playlistImage);
        grid.Children.Add(playlistText);

        playlistsSection.MenuItems.Add(item);
    }


    private void RenderPlayListsSection(ObservableCollection<PlaylistNavItem> playLists)
    {
        var playlistsSection = new NavigationViewItem
        {
            Icon = new FontIcon { Glyph = "\uE142" },
            SelectsOnInvoked = false,
            IsExpanded = true,
        };

        var grid = new Grid();
        grid.Children.Add(new TextBlock { Text = "Your playlists" });

        var addBtn = new Button
        {
            Content = new FontIcon { Glyph = "\uE710", FontSize = 15 },
            Width = 15,
            Height = 15,
            Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
            BorderThickness = new Thickness(0),
            Padding = new Thickness(0),
            HorizontalAlignment = HorizontalAlignment.Right,
        };

        var flyout = new MenuFlyout();

        flyout.Items.Add(new MenuFlyoutItem()
        {
            Text = "New Playlist",
            Tag = "NewPlaylist",
            Icon = new FontIcon { Glyph = "\uE710" },
        });

        flyout.Items.Add(new MenuFlyoutItem()
        {
            Text = "New Folder",
            Tag = "NewFolder",
            Icon = new FontIcon { Glyph = "\uE8F4" },
        });

        addBtn.Flyout = flyout;

        grid.Children.Add(addBtn);
        playlistsSection.Content = grid;

        foreach (var playlist in playLists)
            RenderPlayList(playlist, playlistsSection);

        navView.MenuItems.Add(playlistsSection);
    }

    private static void OnIsPaneOpenDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LibraryNavigationView control && e.NewValue is bool isOpen)
        {
            control.navView.IsPaneOpen = isOpen;

            int currentTicketId = ++control._layoutUpdateId;

            control.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
            {
                if (currentTicketId != control._layoutUpdateId)
                    return;

                if (isOpen)
                    control.OnPaneOpened();
                else
                    control.OnPaneClosed();
            });
        }
    }

    private void OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        ItemInvoked?.Invoke(sender, args);
    }

    private void ResizeGrip_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        resizing = true;

        startX = e.GetCurrentPoint(this)
            .Position.X;

        startWidth = OpenPaneLength;

        resizeGrip.CapturePointer(e.Pointer);
    }

    private void ResizeGrip_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!resizing)
            return;

        if (!navView.IsPaneOpen)
            navView.IsPaneOpen = true;

        var currentX = e.GetCurrentPoint(this)
             .Position.X;

        var delta = currentX - startX;

        var newWidth =  startWidth + delta;

        if (newWidth < 250)
            newWidth = 250;

        if (newWidth > 480)
            newWidth = 480;

        OpenPaneLength = newWidth;
    }

    private void ResizeGrip_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        resizing = false;

        resizeGrip.ReleasePointerCapture(
            e.Pointer);
    }

    private void OnPaneOpened()
    {
        navView.MenuItems.Clear();

        var libraryHeader = new NavigationViewItem
        {
            Content = "Library",
            SelectsOnInvoked = false,
            IsExpanded = true,
            Icon = new SymbolIcon(Symbol.Library)
        };

        libraryHeader.MenuItems.Add(new NavigationViewItem { Content = "Liked Songs", Icon = new FontIcon { Glyph = "\uE00B", Tag = "LikedSongs" } });
        libraryHeader.MenuItems.Add(new NavigationViewItem { Content = "Albums", Icon = new FontIcon { Glyph = "\uE93C" }, Tag = "Albums" });
        libraryHeader.MenuItems.Add(new NavigationViewItem { Content = "Artists", Icon = new SymbolIcon(Symbol.Contact), Tag = "Artists" });
        libraryHeader.MenuItems.Add(new NavigationViewItem { Content = "Podcasts", Icon = new FontIcon { Glyph = "\uEC05", Tag = "Podcasts" } });

        navView.MenuItems.Add(libraryHeader);

        if (MenuItemsSource is ObservableCollection<PlaylistNavItem> playLists)
            RenderPlayListsSection(playLists);
    }

    private void OnPaneClosed()
    {
        var temporaryList = new List<object>(navView.MenuItems);

        navView.MenuItems.Clear();

        int seperatorCount = 0;

        foreach (var item in temporaryList)
        {
            var customSeparator = new NavigationViewItem
            {
                SelectsOnInvoked = false,
                IsEnabled = false,
                Content = new Grid
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Width = 30,
                    Children = {
                        new Border
                        {
                            Height = 1,
                            Background = (Brush)Application.Current.Resources["SystemControlBaseMediumLowAcrylicElementMediumBrush"],
                            Opacity = 0.6
                        }
                    }
                },
            };

            navView.MenuItems.Add(customSeparator);
            seperatorCount++;


            if (item is NavigationViewItem navViewItem)
            {
                var childSnapshot = new List<object>(navViewItem.MenuItems);
                navViewItem.MenuItems.Clear();

                foreach (var childItem in childSnapshot)
                {
                    if (seperatorCount == 2)
                    {
                        var addItem = new NavigationViewItem
                        {
                            Icon = new FontIcon { Glyph = "\uE710" },
                            SelectsOnInvoked = false
                        };

                        addItem.MenuItems.Add(new NavigationViewItem { Icon = new FontIcon { Glyph = "\uE710" }, Content = "New Playlist", Tag = "NewPlaylist" });
                        addItem.MenuItems.Add(new NavigationViewItem { Icon = new FontIcon { Glyph = "\uE8F4" }, Content = "New Folder", Tag = "NewFolder" });

                        navView.MenuItems.Add(addItem);
                    }

                    navView.MenuItems.Add(childItem);
                }
            }
        }
    }

    private void ResizeGrip_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        resizeGrip.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 30, 30, 30));
    }

    private void ResizeGrip_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        resizeGrip.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
    }

    private void OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        ItemChanged?.Invoke(sender, args);
    }
}

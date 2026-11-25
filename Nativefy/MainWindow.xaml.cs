using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Nativefy.Controls;
using Nativefy.Messages;
using Nativefy.Models.Media;
using Nativefy.Views;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Nativefy;

public sealed partial class MainWindow : Window
{
    private const int MinWidth = 1400;
    private const int MinHeight = 800;
    private bool _isFullscreen;

    public ShellViewModel ViewModel { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        AppWindow.SetIcon("Assets/Nativefy.ico");

        OverlappedPresenter presenter = OverlappedPresenter.Create();

        presenter.PreferredMinimumHeight = MinHeight;
        presenter.PreferredMinimumWidth = MinWidth;

        AppWindow.SetPresenter(presenter);

        ViewModel = App.Services.GetRequiredService<ShellViewModel>();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(Titlebar);

        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;

        ContentFrame.Navigate(typeof(HomePage));

        ContentFrame.Navigated += OnNavigated;

        WeakReferenceMessenger.Default.Register<RefreshPlaylistsMessage>(this, (r, m) => Load());
        WeakReferenceMessenger.Default.Register<NavigationMessage>(this, (r, m) =>
        {
            if (ContentFrame.CurrentSourcePageType != m.Page)
                ContentFrame.Navigate(m.Page);
        });
    }

    private async Task<LocalPlaylist?> ShowCreatePlaylistDialogAsync()
    {
        var dialog = new NewPlaylistDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "Create a new playlist",
            PrimaryButtonText = "Create",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary
        };

        var result = await dialog.ShowAsync();

        return result == ContentDialogResult.Primary ? dialog.Playlist : null;
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        Titlebar.IsBackButtonEnabled = ContentFrame.CanGoBack;
    }

    private void OnHomeButtonClick(object sender, RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(HomePage));
    }

    private void OnItemInvoked(object sender, string tag)
    {
        if (string.IsNullOrEmpty(tag))
            return;

        switch (tag)
        {
            case "LikedMedia":
                 ContentFrame.Navigate(typeof(LikedMediaPage));
                break;

            case "LikedChannels":
                ContentFrame.Navigate(typeof(LikedChannelsPage));
                break;

            case "History":
                ContentFrame.Navigate(typeof(HistoryPage));
                break;

            /* SHOULD NOT BE REMOVED OR WILL CAUSE UNDEFINED BEHAVIOR */
            case "LibraryGroup":
                break;

            case "PlaylistGroup":
                break;
            /* ------------------------------------------------------ */

            default:
                if (string.IsNullOrEmpty(tag))
                    return;
                 ContentFrame.Navigate(typeof(PlaylistPage), tag);
                break;
        }
    }

    private void OnPaneToggleRequested(TitleBar sender, object args)
    {
        MainNavView.IsPaneOpen = !MainNavView.IsPaneOpen;
    }

    private void ContentFrame_Loading(FrameworkElement sender, object args)
    {
        Load();
    }

    [RelayCommand]
    public async Task AddPlaylist()
    {
        var response = await ShowCreatePlaylistDialogAsync();
        if (response is null)
            return;

        ViewModel.CreatePlaylist(response);
        Load();
    }

    [RelayCommand]
    public async Task RenamePlaylist(string id)
    {
        var playlistTitle = ViewModel.MenuItems.FirstOrDefault(p => p.Tag == id)?.Text;

        TextBox titleBox = new()
        {
            Text = playlistTitle
        };

        var deleteDialog = new ContentDialog
        {
            XamlRoot = Root.XamlRoot,
            Title = "Rename playlist",
            Content = titleBox,
            PrimaryButtonText = "Save",
            SecondaryButtonText = "Cancel"
        };

        var result = await deleteDialog.ShowAsync();
        if (result is ContentDialogResult.Secondary)
            return;

        var newName = titleBox.Text;

        if (string.IsNullOrEmpty(newName))
            return;

        ViewModel.RenamePlaylist(id, newName);
        Load();
    }

    [RelayCommand]
    public async Task DeletePlaylist(string id)
    {
        var playlistTitle = ViewModel.MenuItems.FirstOrDefault(p => p.Tag == id)?.Text;

        var deleteDialog = new ContentDialog
        {
            XamlRoot = Root.XamlRoot,
            Title = "Are you sure you want to delete this playlist?",
            Content = $"Playlist: {playlistTitle}",
            PrimaryButtonText = "Delete",
            SecondaryButtonText = "Cancel"
        };

        var result = await deleteDialog.ShowAsync();
        if (result is ContentDialogResult.Secondary)
            return;

        ViewModel.DeletePlaylist(id);
        Load();
    }

    private void Load()
    {
        ViewModel.LoadPlaylists();

        MainNavView.ClearGroupItems("PlaylistGroup");

        foreach (var item in ViewModel.MenuItems)
        {
            item.DeleteCommand = DeletePlaylistCommand;
            item.RenameCommand = RenamePlaylistCommand;
            MainNavView.AddItemToGroup(item, "PlaylistGroup");
        }
    }

    private void Searchbox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        var searchQuery = args.QueryText;

        if (string.IsNullOrEmpty(searchQuery))
        {
            if (ContentFrame.CurrentSourcePageType == typeof(HomePage))
                return;

            ContentFrame.Navigate(typeof(HomePage));
            return;
        }

        if (ContentFrame.CurrentSourcePageType == typeof(SearchPage))
        {
            WeakReferenceMessenger.Default.Send<SearchMessage>(new(searchQuery));
            return;
        }

        ContentFrame.Navigate(typeof(SearchPage), searchQuery);
    }

    private void Root_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!_isFullscreen) return;

        var pointerPosition = e.GetCurrentPoint(Root).Position;

        if (pointerPosition.Y <= 15)
            Titlebar.Visibility = Visibility.Visible;

        else if (Titlebar.Visibility == Visibility.Visible)
        {
            if (pointerPosition.Y > Titlebar.ActualHeight)
                Titlebar.Visibility = Visibility.Collapsed;
        }
    }

    private void Titlebar_BackRequested(TitleBar sender, object args)
    {
        if (ContentFrame.CanGoBack)
        {
            ContentFrame.GoBack();
            ContentFrame.ForwardStack.Clear();
        }
    }

    private void ToggleFullscreen_Click(object sender, RoutedEventArgs e)
    {
        _isFullscreen = !_isFullscreen;

        if (_isFullscreen)
        {
            MainNavView.IsPaneOpen = false;
            MainNavView.IsPaneVisible = false;

            Grid.SetRowSpan(Titlebar, 2);
            Titlebar.Visibility = Visibility.Collapsed;
            Titlebar.Background = new SolidColorBrush(ColorHelper.FromArgb(100, 0, 0, 0));
            Titlebar.IsPaneToggleButtonVisible = false;

            Root.PointerMoved += Root_PointerMoved;

            AppWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
        }
        else
        {
            AppWindow.SetPresenter(AppWindowPresenterKind.Default);

            Root.PointerMoved -= Root_PointerMoved;

            MainNavView.IsPaneVisible = true;
            Grid.SetRowSpan(Titlebar, 1);
            Titlebar.Background = null;
            Titlebar.Visibility = Visibility.Visible;
            Titlebar.IsPaneToggleButtonVisible = true;
        }
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(SettingsPage));
    }

    private void NotificationsCenter_Opened(object sender, object e)
    {
        ViewModel.IsNotificationsCenterOpen = true;
        ViewModel.ClearUnreadCount();
    }
}
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Nativefy.Controls;
using Nativefy.Messages;
using Nativefy.Views;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Nativefy;

public sealed partial class MainWindow : Window
{
    private const int MinWidth = 1400;
    private const int MinHeight = 800;
    private readonly IntPtr _hwnd;
    private readonly Win32.SUBCLASSPROC _subclassProc;
    private bool _isFullscreen;

    public ShellViewModel ViewModel { get; set; }
    public ICommand AddCommand { get; set; }

    public MainWindow()
    {
        InitializeComponent();

        _hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

        _subclassProc = new Win32.SUBCLASSPROC(WindowSubclass);
        Win32.SetWindowSubclass(_hwnd, _subclassProc, 0, IntPtr.Zero);

        Closed += OnWindowClosed;

        ViewModel = App.Services.GetRequiredService<ShellViewModel>();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(Titlebar);

        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;

        ContentFrame.Navigate(typeof(HomePage));

        ContentFrame.Navigated += OnNavigated;

        AddCommand = new RelayCommand(AddPlaylist);

        WeakReferenceMessenger.Default.Register<OpenNewPlaylistDialogMessage>(this, (r, m) => m.Reply(ShowDialogAsync(m)));
        WeakReferenceMessenger.Default.Register<RefreshPlaylistsMessage>(this, (r, m) => _ = LoadAsync());
        WeakReferenceMessenger.Default.Register<NavigationMessage>(this, (r, m) =>
        {
            if (ContentFrame.CurrentSourcePageType != m.Page)
                ContentFrame.Navigate(m.Page);
        });
    }

    private async Task<DialogResponse> ShowDialogAsync(OpenNewPlaylistDialogMessage m)
    {
        var dialog = new NewPlaylistDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = m.Title,
            PrimaryButtonText = m.PrimaryButton,
            CloseButtonText = m.ExitButton ?? string.Empty,
            DefaultButton = ContentDialogButton.Primary
        };

        var result = await dialog.ShowAsync();

        return result == ContentDialogResult.Primary
            ? new DialogResponse(DialogResult.Primary, dialog.Playlist)
            : new DialogResponse(DialogResult.Exit);
    }

    private void OnWindowClosed(object sender, WindowEventArgs args)
    {
        // Break the native lifecycle connection to prevent unmanaged memory leaks
        Win32.RemoveWindowSubclass(_hwnd, _subclassProc, 0);
        Closed -= OnWindowClosed;
        ContentFrame.Navigated -= OnNavigated;
    }

    private IntPtr WindowSubclass(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, IntPtr dwRefData)
    {
        if (uMsg == Win32.WM_GETMINMAXINFO)
        {
            // Marshal the unmanaged memory pointer into our structure
            Win32.MINMAXINFO mmi = Marshal.PtrToStructure<Win32.MINMAXINFO>(lParam);

            // Enforce our minimum dimensions
            mmi.ptMinTrackSize.X = MinWidth;
            mmi.ptMinTrackSize.Y = MinHeight;

            // Copy the updated structure back to the unmanaged memory pointer
            Marshal.StructureToPtr(mmi, lParam, false);
            return IntPtr.Zero;
        }

        return Win32.DefSubclassProc(hWnd, uMsg, wParam, lParam);
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

            case "LikedAuthors":
                break;

            case "WatchLater":
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
        _ = LoadAsync();
    }

    public void AddPlaylist()
    {
        _ = ViewModel.CreatePlaylistAsync();
        _ =LoadAsync();
    }

    private async Task LoadAsync()
    {
        await ViewModel.LoadPlaylistsAsync();

        MainNavView.ClearGroupItems("PlaylistGroup");

        foreach (var item in ViewModel.MenuItems)
        {
            MainNavView.AddItemToGroup("PlaylistGroup", item.Tag, item.Text, imageUrl: item.ImageUrl!);
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

    private void HidePlayerbar_Click(object sender, RoutedEventArgs e)
    {
        var btn = (AppBarToggleButton)sender;

        if (btn.IsChecked.HasValue)
            PlaybackBar.Visibility = btn.IsChecked.Value ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ToggleFullscreen_Click(object sender, RoutedEventArgs e)
    {
        _isFullscreen = !_isFullscreen;

        if (_isFullscreen)
        {
            MainNavView.IsPaneVisible = false;
            MainNavView.IsPaneOpen = false;

            Grid.SetRowSpan(Titlebar, 2);
            Titlebar.Visibility = Visibility.Collapsed;
            Titlebar.Background = new SolidColorBrush(ColorHelper.FromArgb(100, 0, 0, 0));

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
        }
    }
}

internal static class Win32
{
    public const uint WM_GETMINMAXINFO = 0x0024;

    public delegate IntPtr SUBCLASSPROC(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, IntPtr dwRefData);

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MINMAXINFO
    {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize; // This controls the minimum resize limit
        public POINT ptMaxTrackSize;
    }

    [DllImport("Comctl32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool SetWindowSubclass(IntPtr hWnd, SUBCLASSPROC pfnSubclass, uint uIdSubclass, IntPtr dwRefData);

    [DllImport("Comctl32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool RemoveWindowSubclass(IntPtr hWnd, SUBCLASSPROC pfnSubclass, uint uIdSubclass);

    [DllImport("Comctl32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr DefSubclassProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);
}
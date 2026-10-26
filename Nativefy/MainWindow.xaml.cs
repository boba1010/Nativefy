using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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
    public ShellViewModel ViewModel { get; set; }

    public ICommand AddCommand { get; set; }

    private readonly IntPtr _hwnd;
    private readonly Win32.SUBCLASSPROC _subclassProc;

    private const int MinWidth = 1400;
    private const int MinHeight = 800;

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
            case "LikedSongs":
                 //ContentFrame.Navigate(typeof(LikedSongsPage));
                break;

            case "Albums":
                ContentFrame.Navigate(typeof(AlbumsPage));
                break;

            case "LibraryGroup":
                break;

            case "PlaylistGroup":
                break;

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

    private async void ContentFrame_Loading(FrameworkElement sender, object args)
    {
        await LoadAsync();   
    }

    public async void AddPlaylist()
    {
        await ViewModel.CreatePlaylistAsync();
        await LoadAsync();
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
using Microsoft.UI.Windowing;
using Nativefy.Views;
using System.Runtime.InteropServices;

namespace Nativefy;

public sealed partial class MainWindow : Window
{
    public ShellViewModel ViewModel { get; set; }

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

        ViewModel = App.Services.GetRequiredService<ShellViewModel>();

        AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(titlebar);

        contentFrame.Navigate(typeof(HomePage));

        contentFrame.Navigated += OnNavigated;
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
            return IntPtr.Zero; // Message handled
        }

        return Win32.DefSubclassProc(hWnd, uMsg, wParam, lParam);
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        if (e.SourcePageType == contentFrame.CurrentSourcePageType)
            return;

        titlebar.IsBackButtonEnabled = contentFrame.CanGoBack;
    }

    private void OnHomeButtonClick(object sender, RoutedEventArgs e)
    {
        contentFrame.Navigate(typeof(HomePage));
    }

    private void OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer is NavigationViewItem selectedItem)
        {
            if (selectedItem.DataContext != null) { }

            string targetTag = selectedItem.Tag?.ToString()!;

            if (string.IsNullOrEmpty(targetTag))
                return;

            switch (targetTag)
            {
                case "LikedSongs":
                    // contentFrame.Navigate(typeof(LikedSongsPage));
                    break;

                case "Albums":
                     //contentFrame.Navigate(typeof(AlbumsPage));
                    break;

                default:
                    // contentFrame.Navigate(typeof(), targetTag);
                    break;
            }
        }
    }

    private void OnPaneToggleRequested(TitleBar sender, object args)
    {
        mainNavView.IsPaneOpen = !mainNavView.IsPaneOpen;
    }

    private async void ContentFrame_Loading(FrameworkElement sender, object args)
    {
        await ViewModel.LoadPlaylistsAsync();
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
        public static extern IntPtr DefSubclassProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);
    }
}

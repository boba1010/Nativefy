using CommunityToolkit.Mvvm.ComponentModel;
using CrashReporter.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.IO;
using System.Text.Json;
using WinRT.Interop;

namespace CrashReporter
{
    public sealed partial class MainWindow : Window
    {
        public MainViewModel? ViewModel { get; set; }
        public MainWindow()
        {
            InitializeComponent();

            ViewModel = new();

            var hWnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            if (appWindow != null)
            {
                int width = 600;
                int height = 300;
                appWindow.Resize(new Windows.Graphics.SizeInt32(width, height));

                var displayArea = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(windowId, Microsoft.UI.Windowing.DisplayAreaFallback.Primary);

                int centerX = (displayArea.WorkArea.Width - width) / 2;
                int centerY = (displayArea.WorkArea.Height - height) / 2;

                int posX = displayArea.WorkArea.X + centerX;
                int posY = displayArea.WorkArea.Y + centerY;

                appWindow.Move(new Windows.Graphics.PointInt32(posX, posY));
            }

            if (appWindow?.Presenter is OverlappedPresenter presenter)
            {
                presenter.IsResizable = false;
                presenter.IsMaximizable = false;
                presenter.SetBorderAndTitleBar(true, false);
            }

            Activated += MainWindow_Activated;
        }

        private async void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            await ViewModel!.LoadCrashFile();
        }

        private void OnCloseClick(object? sender, RoutedEventArgs e)
        {
            App.Current.Exit();
        }
    }
}

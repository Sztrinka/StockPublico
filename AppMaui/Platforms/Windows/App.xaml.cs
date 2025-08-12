using AppMaui.Helpers;
using Microsoft.Maui.Controls; 
using Microsoft.UI.Xaml;
using WinRT.Interop;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace AppMaui.WinUI
{
    public partial class App : MauiWinUIApplication
    {
        public App()
        {
            this.InitializeComponent();
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);

            var window = Microsoft.Maui.Controls.Application.Current.Windows.FirstOrDefault();

            if (window?.Handler?.PlatformView is Microsoft.UI.Xaml.Window nativeWindow)
            {
                // 🔑 Obtener el handle (HWND) real
                var hwnd = WindowNative.GetWindowHandle(nativeWindow);

                // Obtener el AppWindow desde el HWND
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

                // Maximizar la ventana
                if (appWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
                {
                    presenter.Maximize();
                }
            }
        }

    }
}
#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.Maui.Platform;
using Windows.Graphics;
#endif
using System;

namespace AppMaui.Helpers
{
#if WINDOWS
    public static class WindowHelper
    {
        public static void CentrarVentanaPrincipal(Microsoft.Maui.Controls.Window mauiWindow)
        {
            var nativeWindow = (mauiWindow.Handler.PlatformView as Microsoft.UI.Xaml.Window);
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

            var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);
            var centerPosition = new Windows.Graphics.PointInt32
            {
                X = displayArea.WorkArea.Width / 2 - appWindow.Size.Width / 2,
                Y = displayArea.WorkArea.Height / 2 - appWindow.Size.Height / 2
            };

            appWindow.Move(centerPosition);
        }
    }
#endif
}

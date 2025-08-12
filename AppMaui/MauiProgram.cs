using AppMaui.Helpers;
using AppMaui.Pages;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace AppMaui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseBarcodeReader()
            .ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler(typeof(CameraBarcodeReaderView), typeof(CameraBarcodeReaderViewHandler));
                handlers.AddHandler(typeof(BarcodeGeneratorView), typeof(BarcodeGeneratorViewHandler));
            })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif
        builder.Services.AddSingleton<BdService>();
        builder.Services.AddSingleton<MenuPage>();
        builder.Services.AddSingleton<StockPage>();
        builder.Services.AddTransient<ConsumirProdPage>();
        builder.Services.AddTransient<Deposito>();
        builder.Services.AddTransient<InventUniPage>();
        builder.Services.AddSingleton<StockCeluPage>();
        builder.Services.AddTransient<AjusteCeluPage>();
        return builder.Build(); 
    }
}






//using System.Net.NetworkInformation;
//using Microsoft.Extensions.Logging;
//using ZXing.Net.Maui;
//using ZXing.Net.Maui.Controls;

//namespace AppMaui
//{
//    public static class MauiProgram
//    {
//        public static MauiApp CreateMauiApp()
//        {
//            var builder = MauiApp.CreateBuilder();

//            builder
//                .UseMauiApp<App>()
//                .UseBarcodeReader()
//                .ConfigureMauiHandlers(handlers =>
//                {
//                    handlers.AddHandler(typeof(CameraBarcodeReaderView), typeof(CameraBarcodeReaderViewHandler));
//                    handlers.AddHandler(typeof(BarcodeGeneratorView), typeof(BarcodeGeneratorViewHandler));
//                })
//                .ConfigureFonts(fonts =>
//                {
//                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
//                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
//                });

//#if DEBUG
//            builder.Logging.AddDebug();
//#endif

//            return builder.Build();
//        }
//    }
//}


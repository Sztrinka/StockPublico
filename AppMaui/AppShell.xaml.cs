namespace AppMaui;
using AppMaui.Pages;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("menu", typeof(MenuPage));
        Routing.RegisterRoute("deposito", typeof(Deposito));
        Routing.RegisterRoute("consumirprod", typeof(ConsumirProdPage));
        Routing.RegisterRoute("stockunidad", typeof(InventUniPage));
        Routing.RegisterRoute(nameof(StockPage), typeof(StockPage));
        Routing.RegisterRoute("stockcelu", typeof(StockCeluPage));
        Routing.RegisterRoute("ajustecelu", typeof(AjusteCeluPage));
    }
}


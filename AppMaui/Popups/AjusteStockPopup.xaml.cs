using AppMaui.Models;
using AppMaui.Pages;
using CommunityToolkit.Maui.Views;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Maui;

namespace AppMaui.Popups;

public partial class AjusteStockPopup : Popup
{
    public AjusteStockPopup(ProductoDTO producto)
    {
        InitializeComponent();
        BindingContext = producto;
        Color = Colors.Transparent;
    }

    private void OnConfirmarClicked(object sender, EventArgs e)
    {
        if (int.TryParse(DiaEntry.Text, out int nuevaDia) &&
            int.TryParse(LpzEntry.Text, out int nuevaLpz))
        {
            var prod = BindingContext as ProductoDTO;
            prod.Cantidad1 = nuevaDia;
            prod.Cantidad2 = nuevaLpz;
            Close();
        }
        else
        {
            // Opcional: validación
        }
    }

    private void OnCancelarClicked(object sender, EventArgs e)
    {
        Close();
    }
}

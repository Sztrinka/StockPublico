using AppMaui.Models;
using CommunityToolkit.Maui.Views;
namespace AppMaui.Popups;

public partial class AjustePcPopup : Popup
{
	public AjustePcPopup(Producto productoExistente, UbicacionStock deposito)
	{
		InitializeComponent();
        Color = Colors.Transparent;

        TituloLabel.Text = productoExistente.Nombre;

        if (productoExistente != null)
        {
            CantidadEntry.Text = productoExistente.GetStock(deposito, Propietario.Dia).ToString();
            CantidadEntry2.Text = productoExistente.GetStock(deposito, Propietario.Lpz).ToString();
        }
    }

    private void OnCancelarClicked(object sender, EventArgs e)
    {
        Close();
    }
    private void OnConfirmarClicked(object sender, EventArgs e)
    {
        var producto = new ProductoDTO
        {
            Cantidad1 = int.TryParse(CantidadEntry.Text, out int c) ? c : 0,
            Cantidad2 = int.TryParse(CantidadEntry2.Text, out int c2) ? c2 : 0
        };
        Close(producto);
    }
}
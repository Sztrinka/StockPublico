using AppMaui.Models;
using CommunityToolkit.Maui.Views;
namespace AppMaui.Popups;

public partial class ModProdPopup : Popup
{
	public ModProdPopup(Producto productoExistente)
	{
		InitializeComponent();
        Color = Colors.Transparent;
        if (productoExistente != null)
        {
            NombreEntry.Text = productoExistente.Nombre;
            ObservacionEntry.Text = productoExistente.Observacion;
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
            Nombre = NombreEntry.Text,
            Observacion = ObservacionEntry.Text ?? "",
        };
        Close(producto);
    }

}
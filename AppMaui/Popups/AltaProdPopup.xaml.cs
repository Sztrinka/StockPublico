using AppMaui.Models;
using CommunityToolkit.Maui.Views;
namespace AppMaui.Popups;

public partial class AltaProdPopup : Popup
{
    public AltaProdPopup()
	{
		InitializeComponent();
        Color = Colors.Transparent;
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
            Cantidad1 = int.TryParse(CantidadEntry.Text, out int c) ? c : 0,
            Cantidad2 = int.TryParse(CantidadEntry2.Text, out int c2) ? c2 : 0
        };

        Close(producto);
    }

}
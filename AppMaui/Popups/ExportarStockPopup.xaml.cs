using CommunityToolkit.Maui.Views;

namespace AppMaui.Popups;

public partial class ExportarStockPopup : Popup
{
    public ExportarStockPopup()
    {
        InitializeComponent();
        DepositoPicker.SelectedIndex = 0;

        Color = Colors.Transparent;
    }

    private void OnAceptarClicked(object sender, EventArgs e)
    {
        var index = DepositoPicker.SelectedIndex;
        if (index >= 0)
        {
            Close(index); // Devuelve 0 a 4 seg�n la selecci�n
        }
        else
        {
            // Si no se seleccion� nada, tambi�n pod�s cerrar con -1 o mostrar alerta
            Close(-1);
        }
    }
}
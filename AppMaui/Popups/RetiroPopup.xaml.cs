using AppMaui.Models;
using CommunityToolkit.Maui.Views;

namespace AppMaui.Popups;

public partial class RetiroPopup : Popup
{
    private int _stock;
    private Propietario _propietario;

    public RetiroPopup(string producto, int stock, Propietario propietario)
    {
        Color = Colors.Transparent;
        InitializeComponent();

        var propietarioStr = propietario == Propietario.Dia ? "DIA" : "LPZ";

        lblTitulo.Text = $"{producto} ({propietarioStr})";
        lblDeposito.Text = $"Depósito {propietarioStr} (hay {stock})";

        _stock = stock;
        _propietario = propietario;
    }

    private void OnAceptarClicked(object sender, EventArgs e)
    {
        int cant = 0;
        if (!string.IsNullOrEmpty(entryCantidad.Text) && !int.TryParse(entryCantidad.Text, out cant))
        {
            this.ShowToast("Cantidad inválida");
            return;
        }

        if (cant > _stock)
        {
            this.ShowToast("Supera el stock disponible");
            return;
        }

        Close(cant); // Devuelve datos al cerrar el popup
    }

    private void ShowToast(string mensaje)
    {
        Application.Current.MainPage.DisplayAlert("Error", mensaje, "OK");
    }
}
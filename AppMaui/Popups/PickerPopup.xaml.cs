using System.Collections.ObjectModel;
using AppMaui.Models;
using CommunityToolkit.Maui.Views;

namespace AppMaui.Popups;

public partial class PickerPopup : Popup
{
    public event EventHandler<string> UbicacionSeleccionada;
    public PickerPopup()
    {
    InitializeComponent();
        Color = Colors.Transparent;
    }

    private void OnUbicacionSeleccionada(object sender, EventArgs e)
    {
        if (sender is Button btn && !string.IsNullOrWhiteSpace(btn.Text))
        {
            UbicacionSeleccionada?.Invoke(this, btn.Text);
            Close();
        }
    }
}

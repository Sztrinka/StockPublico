using AppMaui.Models;
using AppMaui.Popups;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;


namespace AppMaui.Pages;

public partial class MenuPage : ContentPage
{
	public MenuPage()
	{
		InitializeComponent();
    }

    private async void RetirarDeposito(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("/deposito");
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("/consumirprod");
    }

    private async void Button_Clicked_2(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("/stockunidad");
    }
    private void OnMostrarOpciones(object sender, EventArgs e)
    {
        var popup = new MenuOpcionesPopup();
        this.ShowPopup(popup);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        CambiarTitulo();
    }

    private void CambiarTitulo()
    {
        var movil = Preferences.Get("numero_de_movil", -1);

        switch (movil)
        {
            case (int)UbicacionStock.Movil1:
                Title = "Móvil 1"; break;
            case (int)UbicacionStock.Movil2:
                Title = "Móvil 2"; break;
            case (int)UbicacionStock.Movil3:
                Title = "Móvil 3"; break;
            default:
                break;
        }
    }

}
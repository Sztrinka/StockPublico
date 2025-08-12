using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AppMaui.Helpers;
using AppMaui.Models;
using AppMaui.Popups;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
namespace AppMaui.Pages;

[QueryProperty(nameof(ProductoId), "productoId")]
public partial class AjusteCeluPage : ContentPage
{
    private readonly BdService _bdService;
    public string ProductoId { get; set; }
    private int _productoId;

    private Producto Producto { get; set; }
    public AjusteCeluPage(BdService bdService)
    {
        InitializeComponent();
        BindingContext = this;
        _bdService = bdService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (int.TryParse(ProductoId, out _productoId))
        {
            await RefrescarDatosAsync();
        }
        else
        {
            await DisplayAlert("Error", "Error al cargar el producto", "OK");
            await Shell.Current.GoToAsync("..");
        }
    }

    private async void OnEditarNombre_Clicked(object sender, EventArgs e)
    {
        if (Producto is null) return;
        var popup = new ModProdPopup(Producto);
        var productoEditado = await this.ShowPopupAsync(popup);

        if (productoEditado is null || productoEditado is not ProductoDTO datosEditados) return;

        if (await _bdService.ActualizarProductoAsync(Producto.Id, datosEditados))
        {
            await RefrescarDatosAsync();
            await DisplayAlert("Éxito", "El producto se actualizó correctamente.", "OK");
            
        }
        else
        {
            await DisplayAlert("Error", "El producto no se pudo actualizar.", "Aceptar");
        }
    }
    private async void OnEditarStockTaller_Clicked(object sender, EventArgs e)
    {
        await AjustarStockAsync(UbicacionStock.Taller);
    }
    private async void OnEditarStockM1_Clicked(object sender, EventArgs e)
    {
        await AjustarStockAsync(UbicacionStock.Movil1);

    }
    private async void OnEditarStockM2_Clicked(object sender, EventArgs e)
    {
        await AjustarStockAsync(UbicacionStock.Movil2);
    }
    private async void OnEditarStockM3_Clicked(object sender, EventArgs e)
    {
        await AjustarStockAsync(UbicacionStock.Movil3);
    }
    private async void OnEliminar_Clicked(object sender, EventArgs e)
    {
        if (sender is Button boton)
        {
            bool confirmar = await DisplayAlert("Atención!",
                $"¿ELIMINAR el producto '{Producto.Nombre}'? Esta acción no se puede deshacer.", "Sí", "No");

            if (confirmar)
            {
                if (await _bdService.BorrarProductoAsync(Producto.Id))
                {
                    await DisplayAlert("Exito!", $"Se eliminó el producto {Producto.Nombre}.", "Aceptar");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await DisplayAlert("Error", $"No se pudo borrar el producto.", "Aceptar");
                    return;
                }

            }
        }
    }

    private async Task AjustarStockAsync(UbicacionStock deposito)
    {
        if (Producto is null) return;

        var popup = new AjustePcPopup(Producto, deposito);
        var productoEditado = await this.ShowPopupAsync(popup);

        if (productoEditado is null || productoEditado is not ProductoDTO datosEditados) return;

        if (Producto.GetStock(deposito, Propietario.Dia) != datosEditados.Cantidad1 || Producto.GetStock(deposito, Propietario.Lpz) != datosEditados.Cantidad2)
        {
            if (await _bdService.ActualizarStockAsync((int)deposito, Producto.Id, datosEditados.Cantidad1, datosEditados.Cantidad2))
            {
                await RefrescarDatosAsync();
                await DisplayAlert("Éxito", $"Stock actualizado:\nDía: {datosEditados.Cantidad1}\nLPZ: {datosEditados.Cantidad2}", "OK");
            }
            else
            {
                await DisplayAlert("Error", "El producto no se pudo actualizar.", "Aceptar");
            }
        }
    }

    private async Task RefrescarDatosAsync()
    {
        Producto = await _bdService.GetProductoAsync(_productoId);

        var stockTallerDia = Producto.GetStock(UbicacionStock.Taller, Propietario.Dia);
        var stockTallerLpz = Producto.GetStock(UbicacionStock.Taller, Propietario.Lpz);
        var stockMovil1Dia = Producto.GetStock(UbicacionStock.Movil1, Propietario.Dia);
        var stockMovil1Lpz = Producto.GetStock(UbicacionStock.Movil1, Propietario.Lpz);
        var stockMovil2Dia = Producto.GetStock(UbicacionStock.Movil2, Propietario.Dia);
        var stockMovil2Lpz = Producto.GetStock(UbicacionStock.Movil2, Propietario.Lpz);
        var stockMovil3Dia = Producto.GetStock(UbicacionStock.Movil3, Propietario.Dia);
        var stockMovil3Lpz = Producto.GetStock(UbicacionStock.Movil3, Propietario.Lpz);

        var stockTotal = stockTallerDia + stockTallerLpz + stockMovil1Dia + stockMovil2Dia + stockMovil3Dia + stockMovil1Lpz + stockMovil2Lpz + stockMovil3Lpz;



        NombreProductoLabel.Text = Producto.Nombre;
        ObservacionLabel.Text = Producto.Observacion;
        TallerDiaLabel.Text = $"Cantidad Dia: {stockTallerDia.ToString()}";
        TallerLpzLabel.Text = $"Cantidad Lpz: {stockTallerLpz.ToString()}";
        Movil1DiaLabel.Text = $"Cantidad Dia: {stockMovil1Dia.ToString()}";
        Movil1LpzLabel.Text = $"Cantidad Lpz: {stockMovil1Lpz.ToString()}";
        Movil2DiaLabel.Text = $"Cantidad Dia: {stockMovil2Dia.ToString()}";
        Movil2LpzLabel.Text = $"Cantidad Lpz: {stockMovil2Lpz.ToString()}";
        Movil3DiaLabel.Text = $"Cantidad Dia: {stockMovil3Dia.ToString()}";
        Movil3LpzLabel.Text = $"Cantidad Lpz: {stockMovil3Lpz.ToString()}";
        StockTotalLabel.Text = $"Total Stock: {stockTotal.ToString()}";
    }
}
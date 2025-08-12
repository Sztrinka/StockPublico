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

public partial class StockCeluPage : ContentPage
{

    private readonly BdService _bdService;
    public List<ProductoDTO> _todosLosProductos { get; set; } = new();
    public ObservableCollection<ProductoDTO> ProductosFiltrados { get; set; }
    public string NombreUbicacion { get; set; } = "LPZ"; // binding sencillo
    public StockCeluPage(BdService bdService)
	{
        InitializeComponent();

        _bdService = bdService;
        ProductosFiltrados = new ObservableCollection<ProductoDTO>();
        BindingContext = this;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!Preferences.Get("soy_admin", false))
            await Shell.Current.GoToAsync("//LoginPage");

        await CargarInventarioDesdeBd("TODO");
        FiltrarProductos(BuscadorEntry.Text ?? "");
    }

    private void FiltrarProductos(string texto)
    {
        var filtrados = string.IsNullOrWhiteSpace(texto)
            ? new ObservableCollection<ProductoDTO>(_todosLosProductos)
            : new ObservableCollection<ProductoDTO>(_todosLosProductos
                  .Where(p => p.Nombre.ToLower().Contains(texto.ToLower())));

        ProductosFiltrados.Clear();
        foreach (var p in filtrados)
            ProductosFiltrados.Add(p);
    }

    private async Task CargarInventarioDesdeBd(string ubicacion)
    {
        LoadingOverlay.IsVisible = true;
        ResultadoProductos productos = await _bdService.GetProductosAsync();
        int depositoInt = 1;
        switch (ubicacion.ToUpper())
        {
            default:
            case "TODO":
                //productos = await _bdService.GetProductosAsync();
                depositoInt = 1;
                
                break;
            case "TALLER":
                //productos = await _bdService.GetProductosPorDepositoAsync(UbicacionStock.Taller);
                depositoInt = 1;
                break;
            case "M1":
                //productos = await _bdService.GetProductosPorDepositoAsync(UbicacionStock.Movil1);
                depositoInt = 2;
                break;
            case "M2":
                //productos = await _bdService.GetProductosPorDepositoAsync(UbicacionStock.Movil2);
                depositoInt = 3;
                break;
            case "M3":
                //productos = await _bdService.GetProductosPorDepositoAsync(UbicacionStock.Movil3);
                depositoInt = 4;
                break;
        }

        if(ubicacion.ToUpper() != "TODO")
        {
            productos.Productos = productos.Productos
            .Where(x => x.Stock.Any(s => s.Deposito_id == depositoInt && (s.CantidadDia > 0 || s.CantidadLpz > 0)))
            .ToList();
        }

        var productosDto = productos.Productos.Select(x => new ProductoDTO
        {
            Id = x.Id,
            Nombre = x.Nombre,
            Observacion = x.Observacion,
            Cantidad1 = x.Stock.FirstOrDefault(s => s.Deposito_id == 1)?.CantidadDia ?? 0,
            Cantidad2 = x.Stock.FirstOrDefault(s => s.Deposito_id == 1)?.CantidadLpz ?? 0,
            M1 = (x.Stock.FirstOrDefault(s => s.Deposito_id == 2)?.CantidadDia ?? 0) + (x.Stock.FirstOrDefault(s => s.Deposito_id == 2)?.CantidadLpz ?? 0),
            M2 = (x.Stock.FirstOrDefault(s => s.Deposito_id == 3)?.CantidadDia ?? 0) + (x.Stock.FirstOrDefault(s => s.Deposito_id == 3)?.CantidadLpz ?? 0),
            M3 = (x.Stock.FirstOrDefault(s => s.Deposito_id == 4)?.CantidadDia ?? 0) + (x.Stock.FirstOrDefault(s => s.Deposito_id == 4)?.CantidadLpz ?? 0),
            CantidadAMostrar = (x.Stock.FirstOrDefault(s => s.Deposito_id == depositoInt)?.CantidadDia ?? 0) +
                (x.Stock.FirstOrDefault(s => s.Deposito_id == depositoInt)?.CantidadLpz ?? 0),
        })
            .OrderBy(x => x.Nombre);
        _todosLosProductos = productosDto.ToList();
        LoadingOverlay.IsVisible = false;
    }

    private async void OnAltaClicked(object sender, EventArgs e) 
    {
        var popup = new AltaProdPopup();
        var nuevoProductoResultado = await this.ShowPopupAsync(popup);

        if (nuevoProductoResultado is null || nuevoProductoResultado is not ProductoDTO nuevoProductoDTO) return;

        var nuevoProducto = new Producto
        {
            Nombre = nuevoProductoDTO.Nombre,
            Observacion = nuevoProductoDTO.Observacion
        };

        var nuevoProductoId = await _bdService.AgregarProductoAsync(nuevoProducto);

        if (nuevoProductoId != -1)
        {
            await _bdService.ActualizarStockAsync((int)UbicacionStock.Taller,
                nuevoProductoId,
                nuevoProductoDTO.Cantidad1,
                nuevoProductoDTO.Cantidad2);
            await DisplayAlert("Exito!", $"Se creó el producto {nuevoProducto.Nombre}", "Aceptar");
        }
        else
        {
            await DisplayAlert("Error", $"No se pudo crear el producto.", "Aceptar");
        }
    }


    #region picker ubicacion
    private async void OnUbicacionBtnClicked(object sender, EventArgs e)
    {
        var popup = new PickerPopup();

        popup.UbicacionSeleccionada += async (s, ubicacion) =>
        {
            UbicacionBtn.Text = $"{ubicacion}";
            await CargarInventarioDesdeBd(ubicacion);
            FiltrarProductos("");
        };

        await this.ShowPopupAsync(popup);
    }
    
    #endregion

    private void OnMostrarOpciones(object sender, EventArgs e)
    {
        //Menú cerrar sesión
        var popup = new MenuOpcionesPopup();
        this.ShowPopup(popup);
    }

    private void OnBuscadorTextChanged(object sender, TextChangedEventArgs e)
    {
        FiltrarProductos(e.NewTextValue);
    }

    private async void OnProductoTapped(object sender, EventArgs e)
    {
        if (sender is VisualElement visual && visual.BindingContext is ProductoDTO producto)
        {
            LoadingOverlay.IsVisible = true;
            await Shell.Current.GoToAsync($"/ajustecelu?productoId={producto.Id}");
        }
    }
    // Botones de navegación
    private async void BtnAnterior_Clicked(object sender, EventArgs e)
    {
        //if (currentPage > 1)
        //{
        //    currentPage--;
        //    await CargarPaginaActualAsync();
        //}
    }
    private async void BtnSiguiente_Clicked(object sender, EventArgs e)
    {
        //if (currentPage < TotalPaginas)
        //{
        //    currentPage++;
        //    await CargarPaginaActualAsync();
        //}
    }

    private async void EntryPagina_Completed(object sender, EventArgs e)
    {
        //if (int.TryParse(entryPagina.Text, out int paginaDeseada))
        //{
        //    if (paginaDeseada >= 1 && paginaDeseada <= TotalPaginas)
        //    {
        //        currentPage = paginaDeseada;
        //        await CargarPaginaActualAsync();
        //    }
        //    else
        //    {
        //        await DisplayAlert("Error", $"Ingresá un número entre 1 y {TotalPaginas}.", "OK");
        //        entryPagina.Text = currentPage.ToString();
        //    }
        //}
        //else
        //{
        //    await DisplayAlert("Error", "Número inválido.", "OK");
        //    entryPagina.Text = currentPage.ToString();
        //}
    }


}
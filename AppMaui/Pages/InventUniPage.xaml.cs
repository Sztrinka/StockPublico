using System.Collections.ObjectModel;
using System.Linq;
using AppMaui.Helpers;
using AppMaui.Models;
using AppMaui.Popups;
using CommunityToolkit.Maui.Views;
namespace AppMaui.Pages;

public partial class InventUniPage : ContentPage
{
    private readonly BdService _bdService;
    public List<ProductoDTO> _todosLosProductos { get; set; } = new();
    public ObservableCollection<ProductoDTO> ProductosFiltrados { get; set; }

    public InventUniPage(BdService bdService)
    {
        InitializeComponent();
        _bdService = bdService;

        ProductosFiltrados = new ObservableCollection<ProductoDTO>();

        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await CargarInventarioDesdeBd();
        FiltrarProductos("");
    }

    

    // Método que se llamará desde afuera cuando haya base de datos
    //public void RecargarDesdeBaseDeDatos(List<ProductoDTO> productosDesdeBD)
    //{
    //    ProductosUnidad.Clear();
    //    foreach (var p in productosDesdeBD)
    //        ProductosUnidad.Add(p);

    //    FiltrarProductos(BuscadorEntry.Text);
    //}

    private async Task CargarInventarioDesdeBd()
    {
        var movil = Preferences.Get("numero_de_movil", -1);
        if (movil == -1) await Shell.Current.GoToAsync("//login");

        var depositoActual = (UbicacionStock)movil;
        var productos = await _bdService.GetProductosPorDepositoAsync(depositoActual);
        var productosDto = productos.Productos.Select(x => new ProductoDTO
        {
            Id = x.Id,
            Nombre = x.Nombre,
            Observacion = x.Observacion,
            Cantidad1 = x.Stock.FirstOrDefault(s => s.Deposito_id == movil).CantidadDia,
            Cantidad2 = x.Stock.FirstOrDefault(s => s.Deposito_id == movil).CantidadLpz,
        });
        _todosLosProductos = productosDto.ToList();
    }

    private void OnBuscadorTextChanged(object sender, TextChangedEventArgs e)
    {
        FiltrarProductos(e.NewTextValue);
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

    //AJUSTE DE STOCK

    //private async void OnModificarCantidadTapped(object sender, EventArgs e)
    //{
    //    var frame = sender as Frame;
    //    if (frame?.BindingContext is ProductoDTO producto)
    //    {
    //        var popup = new AjusteStockPopup(producto);
    //        await this.ShowPopupAsync(popup);

    //        ProductosFiltrados = new ObservableCollection<ProductoDTO>(ProductosFiltrados);
    //        BindingContext = null;
    //        BindingContext = this;
    //    }
    //}

}
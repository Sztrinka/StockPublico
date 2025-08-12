using AppMaui.Helpers;
using AppMaui.Models;
using AppMaui.Popups;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace AppMaui.Pages;

public partial class ConsumirProdPage : ContentPage, INotifyPropertyChanged
{
    private bool _procesandoCodigo = false;
    private readonly BdService _bdService;
    private List<Producto> _productosEnEsteMovil = new();
    private UbicacionStock _deposito;

    public ObservableCollection<ModifCantidad> Productos { get; set; } = new ObservableCollection<ModifCantidad>();

    public ConsumirProdPage(BdService bdService)
    {
        InitializeComponent();
        BindingContext = this;

        _bdService = bdService;
        var movil = Preferences.Get("numero_de_movil", -1);

        _deposito = (UbicacionStock)movil;


        qrReaderEliminar.BarcodesDetected += OnBarcodesDetected;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        

        CargandoOverlay.IsVisible = true;

        try
        {
            if (_deposito < 0) throw new Exception();

            var productos = await _bdService.GetProductosPorDepositoAsync(_deposito);
            _productosEnEsteMovil = productos.Productos;
        }
        catch (Exception)
        {
            CargandoOverlay.IsVisible = false;
            await DisplayAlert("Error", "No se pudieron leer los productos de la unidad", "OK");
            await Shell.Current.GoToAsync("..");
        }
        finally
        {
            CargandoOverlay.IsVisible = false;
        }

        

    }

    private void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (_procesandoCodigo)
            return;

        var codigo = e.Results.FirstOrDefault()?.Value;
        if (string.IsNullOrEmpty(codigo))
            return;
        var producto = new Producto(); // get producto de verdad

        if (producto is null) return;

        _procesandoCodigo = true;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            CargandoOverlay.IsVisible = true;
            try
            {
                // Parsear el contenido del QR: producto_id:83;propietario:dia
                var partes = codigo.Split(';', StringSplitOptions.RemoveEmptyEntries);
                var dict = new Dictionary<string, string>();

                foreach (var parte in partes)
                {
                    var kv = parte.Split(':');
                    if (kv.Length == 2)
                        dict[kv[0].Trim().ToLower()] = kv[1].Trim().ToLower();
                }

                if (!dict.TryGetValue("producto_id", out string idStr) || !int.TryParse(idStr, out int idProducto))
                {
                    await DisplayAlert("Error", $"El QR no contiene un ID válido. Leído: '{codigo}'", "OK");
                    return;
                }

                if (!dict.TryGetValue("propietario", out string propietarioStr) ||
                    !(propietarioStr == "dia" || propietarioStr == "lpz"))
                {
                    await DisplayAlert("Error", $"Propietario no válido en QR. Leído: '{codigo}'", "OK");
                    return;
                }

                var propietario = propietarioStr == "dia" ? Propietario.Dia : Propietario.Lpz;

                if (Productos.Any(x => x.ProductoId == idProducto && x.Propietario == propietario))
                {
                    await DisplayAlert("Error", $"Este producto ya está cargado", "OK");
                    return;
                }

                // Buscar el producto
                var producto = _productosEnEsteMovil.Find(x => x.Id == idProducto);
                if (producto == null)
                {
                    await DisplayAlert("Error", $"Producto con ID {idProducto} no encontrado.", "OK");
                    return;
                }

                // Obtener stock del depósito para el propietario

                var stock = producto.GetStock(_deposito, propietario);

                CargandoOverlay.IsVisible = false;

                var popup = new RetiroPopup(producto.Nombre, stock, propietario);
                var result = await this.ShowPopupAsync(popup);

                if (result is int cantidad && cantidad > 0)
                {
                    if (cantidad > stock)
                    {
                        await DisplayAlert("Error", "Cantidad a retirar supera el stock disponible.", "OK");
                        return;
                    }

                    Productos.Add(new ModifCantidad
                    {
                        Producto = producto.Nombre,
                        ProductoId = producto.Id,
                        Propietario = propietario,
                        NuevaCantidad = cantidad,
                        StockActual = stock
                    });
                }
                else
                {
                    await DisplayAlert("Aviso", "No se ingresó cantidad, operación cancelada.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error inesperado", ex.Message, "OK");
            }
            finally
            {
                if (Productos.Any()) ConfirmarButton.IsEnabled = true;
                _procesandoCodigo = false;
                CargandoOverlay.IsVisible = false;
            }
        });
    }

    private async void OnModificarCantidadTapped(object sender, EventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is ModifCantidad seleccionado)
        {

            var popup = new RetiroPopup(seleccionado.Producto, seleccionado.StockActual, seleccionado.Propietario);
            var result = await this.ShowPopupAsync(popup);

            if (result is int cantidad && cantidad > 0)
            {
                //retirarStock del deposito(prod_id, dia, lpz);
                seleccionado.NuevaCantidad = cantidad;

            }
            else
            {
                await DisplayAlert("Error", "Cantidad inválida, por favor ingrese un número.", "OK");
            }
        }
    }

    private void OnEliminarProductoClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is ModifCantidad producto)
        {
            Productos.Remove(producto);
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string nombre = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombre));
    }

    private async void Confirmar_Clicked(object sender, EventArgs e)
    {
        var vm = BindingContext as ConsumirProdPage;
        if (vm != null)
        {
            CargandoOverlay.IsVisible = true;
            var productosConfirmados = vm.Productos.ToList();
            var movimientos = new List<MovimientoStock>();

            foreach (var producto in productosConfirmados)
            {
                movimientos.Add(new MovimientoStock(
                        producto.ProductoId,
                        _deposito,
                        0,
                        producto.NuevaCantidad,
                        producto.Propietario
                    ));
            }

            var errores = await _bdService.ConsumirProductoAsync(movimientos);

            CargandoOverlay.IsVisible = false;

            if (errores.Any())
            {
                await DisplayAlert("Error", "Algunos productos no se pudieron cargar.", "OK");
            }
            else
            {
                await DisplayAlert("Confirmación", "Los productos fueron confirmados correctamente.", "OK");
                await Shell.Current.GoToAsync("..");
            }

        }
    }
}
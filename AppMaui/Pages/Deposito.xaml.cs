using AppMaui.Helpers;
using AppMaui.Models;
using AppMaui.Popups;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace AppMaui.Pages;

public partial class Deposito : ContentPage, INotifyPropertyChanged
{
    private bool _procesandoCodigo = false;
    private readonly BdService _bdService;

    public ObservableCollection<ModifCantidad> Productos { get; set; } = new ObservableCollection<ModifCantidad>();

    public Deposito(BdService bdService)
    {
        InitializeComponent();
        _bdService = bdService;

        // Asigno BindingContext a la propia clase para enlazar Productos
        this.BindingContext = this;

        // Suscribo el evento del scanner
        qrReader.BarcodesDetected += OnBarcodesDetected;
    }

    // Evento cuando detecta código QR
    private void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (_procesandoCodigo)
            return;

        var codigo = e.Results.FirstOrDefault()?.Value;
        if (string.IsNullOrEmpty(codigo))
            return;

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
                var producto = await _bdService.GetProductoAsync(idProducto);
                if (producto == null)
                {
                    await DisplayAlert("Error", $"Producto con ID {idProducto} no encontrado.", "OK");
                    return;
                }

                // Obtener stock del depósito para el propietario

                var stock = producto.GetStock(UbicacionStock.Taller, propietario);

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
        //  MODIFICAR PARA QUE PUEDA MODIFICAR DOS CANTIDADES Y NO UNA SOLA
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

    // Evento para eliminar producto al tocar el botón basura
    private void OnEliminarProductoClicked(object sender, EventArgs e)
    {
        if (sender is Button btn)
        {
            if (btn.BindingContext is ModifCantidad producto)
            {
                Productos.Remove(producto);
                if (Productos.Any()) ConfirmarButton.IsEnabled = true;
                else ConfirmarButton.IsEnabled = false;
            }
        }
    }

    // Implemento INotifyPropertyChanged 
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string nombre = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombre));
    }

    private async void Confirmar_Clicked(object sender, EventArgs e)
    {
        // Obtené la lista de productos (suponiendo que la tienes en el BindingContext)
        var vm = BindingContext as Deposito;
        if (vm != null)
        {
            CargandoOverlay.IsVisible = true;
            var productosConfirmados = vm.Productos.ToList();
            var movimientos = new List<MovimientoStock>();

            var movil = Preferences.Get("numero_de_movil", -1);
            if (movil == -1) await Shell.Current.GoToAsync("//login");

            var depositoDestino = (UbicacionStock)movil;

            foreach (var producto in productosConfirmados)
            {
                movimientos.Add(new MovimientoStock(
                        producto.ProductoId,
                        UbicacionStock.Taller,
                        depositoDestino,
                        producto.NuevaCantidad,
                        producto.Propietario
                    ));
            }

            var errores = await _bdService.MoverStockAsync(movimientos);

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

// Clase modelo para productos con cantidad
public class ModifCantidad : INotifyPropertyChanged
{
    private int _nuevaCantidad;

    public string Producto { get; set; }
    public int ProductoId { get; set; }
    public Propietario Propietario { get; set; }
    public int StockActual { get; set; }
    public int NuevaCantidad { get => _nuevaCantidad; 
        set
        {
            if(_nuevaCantidad != value)
            {
                _nuevaCantidad = value;
                OnPropertyChanged(nameof(NuevaCantidad));
                OnPropertyChanged(nameof(DisplayText));
            }
        }
    }
    public string DisplayText => $"{Producto} ({(Propietario == Propietario.Dia ? "DIA" : "LPZ")}): {NuevaCantidad} unidades";

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string nombre = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombre));
    }
}


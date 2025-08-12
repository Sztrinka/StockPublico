using AppMaui.Helpers;
using AppMaui.Models;
using AppMaui.Popups;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using QRCoder;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


#if WINDOWS
using System.Windows.Forms;
#endif

namespace AppMaui.Pages;

public partial class StockPage : ContentPage, INotifyPropertyChanged
{
    private readonly BdService _bdService;
    private string _textoFiltroActual = "";
    private string _columnaOrdenActual = "Nombre";
    private bool _ordenAscendente = true;

    // Propiedad que expone la lista filtrada y ordenada para el Binding
    public List<ProductoDTO> StockItems { get; set; } = new();

    public StockPage(BdService bdService)
    {
        InitializeComponent();
        BindingContext = this;
        _bdService = bdService;

        UbicacionPicker.SelectedIndex = 0;

        AplicarFiltroYOrden();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var productos = await _bdService.GetProductosAsync(paginaActual: 1, orderBy: "nombre.asc");
        //StockItems.Clear();
        var productosDto = productos.Productos.Select(x => new ProductoDTO
        {
            Id = x.Id,
            Nombre = x.Nombre,
            Observacion = x.Observacion,
            Cantidad1 = x.Stock.FirstOrDefault(s => s.Deposito_id == 1)?.CantidadDia ?? 0,
            Cantidad2 = x.Stock.FirstOrDefault(s => s.Deposito_id == 1)?.CantidadLpz ?? 0,
            M1 = (x.Stock.FirstOrDefault(s => s.Deposito_id == 2)?.CantidadLpz ?? 0) + (x.Stock.FirstOrDefault(s => s.Deposito_id == 2)?.CantidadLpz ?? 0),
            M2 = (x.Stock.FirstOrDefault(s => s.Deposito_id == 3)?.CantidadLpz ?? 0) + (x.Stock.FirstOrDefault(s => s.Deposito_id == 3)?.CantidadLpz ?? 0),
            M3 = (x.Stock.FirstOrDefault(s => s.Deposito_id == 4)?.CantidadLpz ?? 0) + (x.Stock.FirstOrDefault(s => s.Deposito_id == 4)?.CantidadLpz ?? 0),
        }).ToList();
        StockItems = productosDto;
    }

    // Método para filtrar y ordenar la lista según estado actual
    private async void AplicarFiltroYOrden()
    {
        currentPage = 1;
        await CargarPaginaActualAsync();
    }

    #region Eventos

    // Evento para filtrar mientras se escribe en el buscador
    private void OnBuscadorStockChanged(object sender, TextChangedEventArgs e)
    {
        _textoFiltroActual = e.NewTextValue ?? "";
        // No hace nada hasta que se presione Enter
    }

    private void OnBuscadorSearchButtonPressed(object sender, EventArgs e)
    {
        AplicarFiltroYOrden();
    }

    // Eventos para ordenar al hacer clic en cada encabezado de columna
    private void BtnProducto_Clicked(object sender, System.EventArgs e)
    {
        CambiarOrden("Nombre");
    }

    #endregion
    // Método auxiliar que cambia la columna de orden y alterna asc/desc si es la misma
    private void CambiarOrden(string columna)
    {
        if (_columnaOrdenActual == columna)
        {
            _ordenAscendente = !_ordenAscendente;
        }
        else
        {
            _columnaOrdenActual = columna;
            _ordenAscendente = true;
        }
        AplicarFiltroYOrden();
    }

    // Métodos para Alta, Baja, Modificación y Ajuste de stock
    private async void OnAltaClicked(object sender, System.EventArgs e)
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
            AplicarFiltroYOrden();
            await DisplayAlert("Exito!", $"Se creó el producto {nuevoProducto.Nombre}", "Aceptar");
        }
        else
        {
            await DisplayAlert("Error", $"No se pudo crear el producto.", "Aceptar");
        }
    }

    private async void OnBajaClicked(object sender, EventArgs e)
    {
        // Obtener el producto vinculado al botón presionado
        if (sender is Microsoft.Maui.Controls.Button boton && boton.BindingContext is ProductoDTO producto)
        {
            bool confirmar = await DisplayAlert("Atención!",
                $"¿ELIMINAR el producto '{producto.Nombre}'? Esta acción no se puede deshacer.", "Sí", "No");

            if (confirmar)
            {
                if (await _bdService.BorrarProductoAsync(producto.Id))
                {
                    await CargarPaginaActualAsync();
                    await DisplayAlert("Exito!", $"Se eliminó el producto {producto.Nombre}.", "Aceptar");
                }
                else
                {
                    await DisplayAlert("Error", $"No se pudo borrar el producto.", "Aceptar");
                    return;
                }

            }
        }
    }

    private async void OnModificarClicked(object sender, EventArgs e)
    {

        if (sender is Microsoft.Maui.Controls.Button boton && boton.BindingContext is ProductoDTO productoDto)
        {
            var producto = await _bdService.GetProductoAsync(productoDto.Id);
            if (producto is null) return;
            var popup = new ModProdPopup(producto);
            var productoEditado = await this.ShowPopupAsync(popup);

            if (productoEditado is null || productoEditado is not ProductoDTO datosEditados) return;

            if (await _bdService.ActualizarProductoAsync(producto.Id, datosEditados))
            {
                await CargarPaginaActualAsync();
                await DisplayAlert("Éxito", "El producto se actualizó correctamente.", "OK");
            }
            else
            {
                await DisplayAlert("Error", "El producto no se pudo actualizar.", "Aceptar");
            }
        }
    }

    private async void OnAjusteStockClicked(object sender, EventArgs e)
    {

        if (sender is Microsoft.Maui.Controls.Button boton && boton.BindingContext is ProductoDTO productoOriginalDto)
        {
            var productoOriginal = await _bdService.GetProductoAsync(productoOriginalDto.Id);
            if (productoOriginal is null) return;

            var popup = new AjustePcPopup(productoOriginal, UbicacionStock.Taller);
            var productoEditado = await this.ShowPopupAsync(popup);

            if (productoEditado is null || productoEditado is not ProductoDTO datosEditados) return;

            if (productoOriginal.GetStock(UbicacionStock.Taller, Propietario.Dia) != datosEditados.Cantidad1 || productoOriginal.GetStock(UbicacionStock.Taller, Propietario.Lpz) != datosEditados.Cantidad2)
            {
                if (await _bdService.ActualizarStockAsync(1, productoOriginal.Id, datosEditados.Cantidad1, datosEditados.Cantidad2))
                {
                    await CargarPaginaActualAsync();
                    await DisplayAlert("Éxito", $"Stock actualizado:\nDía: {datosEditados.Cantidad1}\nLPZ: {datosEditados.Cantidad2}", "OK");
                }
                else
                {
                    await DisplayAlert("Error", "El producto no se pudo actualizar.", "Aceptar");
                }
            }
        }
    }

    // Evento para imprimir QR
    private async void OnImprimirQRClicked(object sender, EventArgs e)
    {
        if (sender is Microsoft.Maui.Controls.Button button && button.BindingContext is ProductoDTO producto)
        {
            await Microsoft.Maui.Controls.Application.Current.MainPage.Navigation.PushAsync(
                new QRViewPage(producto));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    //Paginado
    private const int PageSize = 15;
    private int currentPage = 1;
    private List<Producto> productosFiltradosOrdenados = new();
    private int TotalPaginas { get; set; }


    // Botones de navegación
    private async void BtnAnterior_Clicked(object sender, EventArgs e)
    {
        if (currentPage > 1)
        {
            currentPage--;
            await CargarPaginaActualAsync();
        }
    }
    private async void BtnSiguiente_Clicked(object sender, EventArgs e)
    {
        if (currentPage < TotalPaginas)
        {
            currentPage++;
            await CargarPaginaActualAsync();
        }
    }

    private async Task CargarPaginaActualAsync()
    {
        var textoFiltro = _textoFiltroActual?.Trim().ToLower() ?? "";
        var orderBy = $"{_columnaOrdenActual.ToLower()}.{(_ordenAscendente ? "asc" : "desc")}";

        var indexDepositoSeleccionado = UbicacionPicker.SelectedIndex;
        var resultadoPaginado = await _bdService.GetProductosAsync(currentPage, PageSize, textoFiltro, orderBy, (UbicacionStock)indexDepositoSeleccionado);

        TotalPaginas = (int)Math.Ceiling((double)resultadoPaginado.CantidadTotal / PageSize);

        var productosDto = resultadoPaginado.Productos.Select(x => new ProductoDTO
        {
            Id = x.Id,
            Nombre = x.Nombre,
            Observacion = x.Observacion,
            Cantidad1 = x.Stock.FirstOrDefault(s => s.Deposito_id == 1)?.CantidadDia ?? 0,
            Cantidad2 = x.Stock.FirstOrDefault(s => s.Deposito_id == 1)?.CantidadLpz ?? 0,
            M1 = (x.Stock.FirstOrDefault(s => s.Deposito_id == 2)?.CantidadDia ?? 0) + (x.Stock.FirstOrDefault(s => s.Deposito_id == 2)?.CantidadLpz ?? 0),
            M2 = (x.Stock.FirstOrDefault(s => s.Deposito_id == 3)?.CantidadDia ?? 0) + (x.Stock.FirstOrDefault(s => s.Deposito_id == 3)?.CantidadLpz ?? 0),
            M3 = (x.Stock.FirstOrDefault(s => s.Deposito_id == 4)?.CantidadDia ?? 0) + (x.Stock.FirstOrDefault(s => s.Deposito_id == 4)?.CantidadLpz ?? 0),
        }).ToList();

        StockItems = productosDto;
        OnPropertyChanged(nameof(StockItems));

        entryPagina.Text = currentPage.ToString();
        lblTotalPaginas.Text = $"de {TotalPaginas}";

        btnAnterior.IsEnabled = currentPage > 1;
        btnSiguiente.IsEnabled = currentPage < TotalPaginas;
    }
    private async void EntryPagina_Completed(object sender, EventArgs e)
    {
        if (int.TryParse(entryPagina.Text, out int paginaDeseada))
        {
            if (paginaDeseada >= 1 && paginaDeseada <= TotalPaginas)
            {
                currentPage = paginaDeseada;
                await CargarPaginaActualAsync();
            }
            else
            {
                await DisplayAlert("Error", $"Ingresá un número entre 1 y {TotalPaginas}.", "OK");
                entryPagina.Text = currentPage.ToString();
            }
        }
        else
        {
            await DisplayAlert("Error", "Número inválido.", "OK");
            entryPagina.Text = currentPage.ToString();
        }
    }

    //EXPORTAR
    private async void Exportar_Clicked(object sender, EventArgs e)
    {
        var popup = new ExportarStockPopup();
        var resultado = await this.ShowPopupAsync(popup);

        if (resultado is int depositoId && depositoId >= 0)
        {
            var productos = await _bdService.GetProductosAsync();
            Exportar(productos.Productos, depositoId);
        }
    }

    //CERRAR SESION
    private async void CerrarSesion_Clicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Cerrar sesión", "¿Seguro que querés cerrar sesión?", "Sí", "No");
        if (confirm)
        {
            // Ajusta la ruta según la configuración de tu Shell o navegación
            Preferences.Set("estoy_logueado", false);
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }

    public async void Exportar(List<Producto> productos, int deposito = 0)
    {


#if WINDOWS
        // Crear el contenido del CSV
        StringBuilder sb = new StringBuilder();

        var nombreDeArchivo = "stock.csv";
        // Agregar encabezado
        sb.AppendLine("sep=;");
        switch (deposito)
        {
            default:
            case 0:
                sb.AppendLine("Nombre;TallerDIA;TallerLPZ;Movil1DIA;Movil1LPZ;Movil2DIA;Movil2LPZ;Movil3DIA;Movil3LPZ;Observacion;");
                nombreDeArchivo = "stock.csv";
                break;
            case 1:
                sb.AppendLine("Nombre;TallerDIA;TallerLPZ;Observacion;");
                nombreDeArchivo = "stock_taller.csv";
                break;
            case 2:
                sb.AppendLine("Nombre;Movil1DIA;Movil1LPZ;Observacion;");
                nombreDeArchivo = "stock_movil1.csv";
                break;
            case 3:
                sb.AppendLine("Nombre;Movil2DIA;Movil2LPZ;Observacion;");
                nombreDeArchivo = "stock_movil2.csv";
                break;
            case 4:
                sb.AppendLine("Nombre;Movil3DIA;Movil3LPZ;Observacion;");
                nombreDeArchivo = "stock_movil3.csv";
                break;
        }
        

        var productosFiltrados = productos.AsEnumerable();
        if (deposito > 0)
            productosFiltrados = productos.Where(x => x.Stock.Any(s => s.Deposito_id == deposito));

        var productosDto = productosFiltrados.Select(x => new
        {
            Id = x.Id,
            Nombre = x.Nombre,
            Observacion = x.Observacion,
            TDIA = x.Stock.FirstOrDefault(s => s.Deposito_id == 1)?.CantidadDia ?? 0,
            TLPZ = x.Stock.FirstOrDefault(s => s.Deposito_id == 1)?.CantidadLpz ?? 0,
            M1DIA = x.Stock.FirstOrDefault(s => s.Deposito_id == 2)?.CantidadDia ?? 0,
            M1LPZ = x.Stock.FirstOrDefault(s => s.Deposito_id == 2)?.CantidadLpz ?? 0,
            M2DIA = x.Stock.FirstOrDefault(s => s.Deposito_id == 3)?.CantidadDia ?? 0,
            M2LPZ = x.Stock.FirstOrDefault(s => s.Deposito_id == 3)?.CantidadLpz ?? 0,
            M3DIA = x.Stock.FirstOrDefault(s => s.Deposito_id == 4)?.CantidadDia ?? 0,
            M3LPZ = x.Stock.FirstOrDefault(s => s.Deposito_id == 4)?.CantidadLpz ?? 0
        }).ToList();

        // Agregar filas de datos
        foreach (var producto in productosDto)
        {
            if (deposito > 0 &&
                ((deposito == 1 && producto.TDIA + producto.TLPZ == 0) ||
                (deposito == 2 && producto.M1DIA + producto.M1LPZ == 0) ||
                (deposito == 3 && producto.M2DIA + producto.M2LPZ == 0) ||
                (deposito == 4 && producto.M3DIA + producto.M3LPZ == 0))) { continue; }

            switch (deposito)
            {
                default:
                case 0:
                    sb.AppendLine($"{producto.Nombre};{producto.TDIA};{producto.TLPZ};{producto.M1DIA};{producto.M1LPZ};{producto.M2DIA};{producto.M2LPZ};{producto.M3DIA};{producto.M3LPZ};{producto.Observacion};");
                    break;
                case 1:
                    sb.AppendLine($"{producto.Nombre};{producto.TDIA};{producto.TLPZ};{producto.Observacion};");
                    break;
                case 2:
                    sb.AppendLine($"{producto.Nombre};{producto.M1DIA};{producto.M1LPZ};{producto.Observacion};");
                    break;
                case 3:
                    sb.AppendLine($"{producto.Nombre};{producto.M2DIA};{producto.M2LPZ};{producto.Observacion};");
                    break;
                case 4:
                    sb.AppendLine($"{producto.Nombre};{producto.M3DIA};{producto.M3LPZ};{producto.Observacion};");
                    break;
            }

        }

        // Crear un cuadro de diálogo de guardado
        var saveFileDialog = new SaveFileDialog
        {
            Filter = "Archivos CSV (*.csv)|*.csv",
            FileName = nombreDeArchivo
        };

        // Mostrar el cuadro de diálogo de guardado
        var result = saveFileDialog.ShowDialog();

        if (result == DialogResult.OK)
        {
            // Guardar el archivo con codificación UTF-8
            string filePath = saveFileDialog.FileName;
            await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.Latin1);
            Console.WriteLine($"Archivo guardado en: {filePath}");
            await DisplayAlert("Exito", $"Se guardó el archivo {Path.GetFileName(filePath)}", "OK");
        }
        else
        {
            Console.WriteLine("El usuario canceló la operación.");
        }

#endif
    }

    private void UbicacionPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        AplicarFiltroYOrden();
    }

    private async void OnStockMovilTapped(object sender, EventArgs e)
    {
        if (sender is Microsoft.Maui.Controls.Label label && label.BindingContext is ProductoDTO productoOriginalDto)
        {
            var gesture = label.GestureRecognizers.FirstOrDefault() as TapGestureRecognizer;
            int depositoInt;
            if (gesture?.CommandParameter is string paramStr && int.TryParse(paramStr, out depositoInt))
            {
                var productoOriginal = await _bdService.GetProductoAsync(productoOriginalDto.Id);
                if (productoOriginal is null) return;

                UbicacionStock deposito = (UbicacionStock)depositoInt;
                if (deposito == 0) deposito = UbicacionStock.Taller;

                var popup = new AjustePcPopup(productoOriginal, deposito);
                var productoEditado = await this.ShowPopupAsync(popup);

                if (productoEditado is null || productoEditado is not ProductoDTO datosEditados) return;

                if (productoOriginal.GetStock(deposito, Propietario.Dia) != datosEditados.Cantidad1 || productoOriginal.GetStock(deposito, Propietario.Lpz) != datosEditados.Cantidad2)
                {
                    if (await _bdService.ActualizarStockAsync((int)deposito, productoOriginal.Id, datosEditados.Cantidad1, datosEditados.Cantidad2))
                    {
                        await CargarPaginaActualAsync();
                        await DisplayAlert("Éxito", $"Stock actualizado:\nDía: {datosEditados.Cantidad1}\nLPZ: {datosEditados.Cantidad2}", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Error", "El producto no se pudo actualizar.", "Aceptar");
                    }
                }

                //var abmPage = new ABMPage(productoOriginal.Nombre, "Guardar", productoOriginal, mostrarCantidad: true, soloCantidad: true, deposito);

                //abmPage.Confirmado += async (s, productoAjustado) =>
                //{
                //    if (deposito == 0) deposito = UbicacionStock.Taller;
                //    if (productoOriginal.GetStock(deposito, Propietario.Dia) != productoAjustado.Cantidad1 || productoOriginal.GetStock(deposito, Propietario.Lpz) != productoAjustado.Cantidad2)
                //    {
                //        await _bdService.ActualizarStockAsync((int)deposito, productoOriginal.Id, productoAjustado.Cantidad1, productoAjustado.Cantidad2);
                //    }

                //    await CargarPaginaActualAsync();

                //    await DisplayAlert("Éxito", $"Stock actualizado:\nDía: {productoAjustado.Cantidad1}\nLPZ: {productoAjustado.Cantidad2}", "OK");
                //};

                //await Navigation.PushModalAsync(abmPage);
            }

        }
    }
}


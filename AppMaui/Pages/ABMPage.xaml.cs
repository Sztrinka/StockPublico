using AppMaui.Models;

namespace AppMaui.Pages;

public partial class ABMPage : ContentPage
{
    public string Titulo { get; set; }
    public string BotonTexto { get; set; }
    public bool MostrarCantidad { get; set; } = true;

    public event EventHandler<ProductoDTO> Confirmado;

    // Nueva propiedad para controlar si solo se permite modificar cantidades
    public bool SoloCantidad { get; set; } = false;

    // Controla visibilidad y habilitación de campos según flags
    public bool MostrarCamposDetalle => MostrarCantidad && !SoloCantidad;

    public ABMPage(string titulo, string botonTexto, Producto productoExistente = null, bool mostrarCantidad = true, bool soloCantidad = false, UbicacionStock deposito = 0)
    {
        InitializeComponent();

        BotonTexto = botonTexto;
        MostrarCantidad = mostrarCantidad;
        SoloCantidad = soloCantidad;

        string depositoStr = "";

       if(deposito == 0) deposito = UbicacionStock.Taller;
        else
        {
            switch (deposito)
            {
                default:
                case UbicacionStock.Taller:
                    break;
                case UbicacionStock.Movil1:
                    depositoStr = "MOVIL 1: ";
                    break;
                case UbicacionStock.Movil2:
                    depositoStr = "MOVIL 2: ";
                    break;
                case UbicacionStock.Movil3:
                    depositoStr = "MOVIL 3: ";
                    break;
            }
        }

        Titulo = $"{depositoStr}{titulo}";

        BindingContext = this;

        if (productoExistente != null)
        {
            NombreEntry.Text = productoExistente.Nombre;
            ObservacionEntry.Text = productoExistente.Observacion;
            CantidadEntry.Text = productoExistente.GetStock(deposito, Propietario.Dia).ToString();
            CantidadEntry2.Text = productoExistente.GetStock(deposito, Propietario.Lpz).ToString();
        }

        // Si es solo ajuste de cantidades, deshabilito el resto de los campos para que no puedan modificarse
        if (SoloCantidad)
        {
            NombreEntry.IsEnabled = false;
            NombreEntry.IsVisible = false;
            ObservacionEntry.IsEnabled = false;
            ObservacionEntry.IsVisible = false;
        }
    }

    private async void OnCancelarClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void OnConfirmarClicked(object sender, EventArgs e)
    {
        // Validación de cantidades si se muestran
        if (MostrarCantidad)
        {
            if (!string.IsNullOrWhiteSpace(CantidadEntry.Text) && !int.TryParse(CantidadEntry.Text, out int cantidad))
            {
                await DisplayAlert("Error", "Por favor ingresá una cantidad válida para Stock Día.", "Aceptar");
                return;
            }

            if (!string.IsNullOrWhiteSpace(CantidadEntry2.Text) && !int.TryParse(CantidadEntry2.Text, out int cantidad2))
            {
                await DisplayAlert("Error", "Por favor ingresá una cantidad válida para Stock LPZ.", "Aceptar");
                return;
            }
        }

        if (MostrarCamposDetalle)
        {
            if (string.IsNullOrWhiteSpace(NombreEntry.Text))
            {
                await DisplayAlert("Error", "Por favor completá todos los campos obligatorios.", "Aceptar");
                return;
            }
        }

        var producto = new ProductoDTO
        {
            Nombre = NombreEntry.Text,
            Observacion = ObservacionEntry.Text ?? "",
            Cantidad1 = int.TryParse(CantidadEntry.Text, out int c) ? c : 0,
            Cantidad2 = int.TryParse(CantidadEntry2.Text, out int c2) ? c2 : 0
        };

        Confirmado?.Invoke(this, producto);

        await Navigation.PopModalAsync();
    }
}
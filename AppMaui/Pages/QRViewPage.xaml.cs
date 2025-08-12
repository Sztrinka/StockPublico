using System;
using System.IO;
using AppMaui.Models;
using Microsoft.Maui.Controls;
using QRCoder;
#if WINDOWS
using AppMaui.Platforms.Windows;
#endif

namespace AppMaui.Pages;

public partial class QRViewPage : ContentPage
{
    private readonly byte[] _qrBytesDia;
    private readonly byte[] _qrBytesLpz;

    public QRViewPage(ProductoDTO producto)
    {
        InitializeComponent();

        // Mostrar nombre del producto arriba
        LblNombreProducto.Text = producto.Nombre;

        // Generar QR para Stock Día
        string dataDia = $"producto_id:{producto.Id};propietario:dia;";
        var qrDia = GenerarQR(dataDia);

        // Generar QR para Stock LPZ
        string dataLpz = $"producto_id:{producto.Id};propietario:lpz;";
        var qrLpz = GenerarQR(dataLpz);

        // Asignar imágenes
        QrImageDia.Source = qrDia.Image;
        _qrBytesDia = qrDia.Bytes;

        QrImageLpz.Source = qrLpz.Image;
        _qrBytesLpz = qrLpz.Bytes;

        double escalaGuardada = Preferences.Get("slider_escala", 100.0);
        SliderEscala.Value = Math.Round(escalaGuardada);
        LblEscalaValue.Text = $"Escala: {(int)SliderEscala.Value}%";
    }

    private (ImageSource Image, byte[] Bytes) GenerarQR(string data)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        byte[] qrBytes = qrCode.GetGraphic(10, System.Drawing.Color.Black, System.Drawing.Color.White, false);
        var image = ImageSource.FromStream(() => new MemoryStream(qrBytes));
        return (image, qrBytes);
    }

    // imprimir solo el QR de Stock Día
    private void OnImprimirQRDiaClicked(object sender, EventArgs e)
    {
#if WINDOWS
        double escala = ObtenerEscalaDesdeSlider();
        Preferences.Set("slider_escala", SliderEscala.Value);
        ImpresoraHelper.ImprimirQRIndividual(_qrBytesDia, LblNombreProducto.Text, "DIA", escala);
#endif
    }

    // imprimir solo el QR de Stock LPZ
    private void OnImprimirQRLPZClicked(object sender, EventArgs e)
    {
#if WINDOWS
        double escala = ObtenerEscalaDesdeSlider();
        Preferences.Set("slider_escala", SliderEscala.Value);
        ImpresoraHelper.ImprimirQRIndividual(_qrBytesLpz, LblNombreProducto.Text,"LPZ", escala);
#endif
    }
    private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
    {
        int entero = (int)Math.Round(e.NewValue);
        SliderEscala.Value = entero; // Forzamos valor entero
        LblEscalaValue.Text = $"Escala: {entero}%";
    }

    private double ObtenerEscalaDesdeSlider()
    {
        int porcentaje = (int)Math.Round(SliderEscala.Value);
        return porcentaje / 100.0;
    }
}
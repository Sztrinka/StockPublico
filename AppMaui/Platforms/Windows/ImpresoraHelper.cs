#if WINDOWS
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Runtime.Versioning;
using System.Windows.Forms;
using Microsoft.Maui.Controls;
using Application = Microsoft.Maui.Controls.Application;
using SDBitmap = System.Drawing.Bitmap;
using SDFont = System.Drawing.Font;
using SDGraphicsUnit = System.Drawing.GraphicsUnit;
using SDImage = System.Drawing.Image;
using SDSize = System.Drawing.Size;
using SDSizeF = System.Drawing.SizeF;

namespace AppMaui.Platforms.Windows;

[SupportedOSPlatform("windows")]
public static class ImpresoraHelper
{
    public static async void ImprimirQRIndividual(
    byte[] qrBytes,
    string nombreProducto,
    string nombreDeposito,
    double escala = 1.0)
    {
        try
        {
            using var qrImg = SDImage.FromStream(new MemoryStream(qrBytes));

            const int dpi = 203;
            int widthPxBase = (int)(8.0 * dpi / 2.54);   // 640 px
            int heightPxBase = (int)(5.0 * dpi / 2.54);  // 400 px

            int widthPx = (int)(widthPxBase * escala);
            int heightPx = (int)(heightPxBase * escala);

            using var etiquetaBitmap = new SDBitmap(widthPx, heightPx);
            etiquetaBitmap.SetResolution(dpi, dpi);

            using (var g = Graphics.FromImage(etiquetaBitmap))
            {
                g.Clear(System.Drawing.Color.White);
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                // Escalado proporcional de fuentes
                var fontProd = new SDFont("Arial", (float)(16 * escala), FontStyle.Bold);
                var fontDepo = new SDFont("Arial", (float)(36 * escala), FontStyle.Bold);

                // Nombre del producto arriba
                var prodSize = g.MeasureString(nombreProducto, fontProd);
                float yProd = 5 * (float)escala;
                g.DrawString(nombreProducto, fontProd, Brushes.Black, (widthPx - prodSize.Width) / 2, yProd);

                float yContenido = yProd + prodSize.Height + (5 * (float)escala);
                int altoContenido = heightPx - (int)yContenido - (int)(5 * escala);

                // QR alineado a la izquierda
                int qrSize = altoContenido;
                int xQR = (int)(10 * escala);
                int yQR = (int)yContenido;

                g.DrawImage(qrImg, new Rectangle(xQR, yQR, qrSize, qrSize));

                // Depósito a la derecha
                var depoSize = g.MeasureString(nombreDeposito, fontDepo);
                int xDepo = widthPx - (int)depoSize.Width - (int)(10 * escala);
                int yDepo = yQR + (qrSize - (int)depoSize.Height) / 2;

                g.DrawString(nombreDeposito.ToUpper(), fontDepo, Brushes.Black, xDepo, yDepo);
            }

            // Imprimir
            var pd = new PrintDocument();

            pd.PrintPage += (s, e) =>
            {
                e.PageSettings.Margins = new Margins(0, 0, 0, 0);
                e.Graphics.DrawImage(etiquetaBitmap, new Rectangle(0, 0, widthPx, heightPx));
            };

            var dlg = new PrintDialog { Document = pd };
            if (dlg.ShowDialog() == DialogResult.OK)
                pd.Print();

            // Opcional para debug visual:
            etiquetaBitmap.Save("C:\\Temp\\EtiquetaPreview.png", System.Drawing.Imaging.ImageFormat.Png);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error al imprimir: {ex.Message}", "OK");
        }
    }
}
#endif
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace AppMaui.Popups
{
    public partial class MenuOpcionesPopup : Popup
    {
        public MenuOpcionesPopup()
        {
            // Fondo del popup completamente transparente
            Color = Colors.Transparent;

            var fondo = new Frame
            {
                CornerRadius = 16,
                BackgroundColor = Color.FromArgb("#1E1E1E"),
                Padding = 10,
                HasShadow = false,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                WidthRequest = 220,
                Content = new VerticalStackLayout
                {
                    Spacing = 10,
                    Children =
                    {
                        new Label
                        {
                            Text = "⚙️ Configuración",
                            FontSize = 16,
                            TextColor = Colors.LightGray,
                            FontFamily = DeviceInfo.Platform == DevicePlatform.Android ? "sans-serif" : null,
                            HorizontalTextAlignment = TextAlignment.Center
                        },
                        new BoxView
                        {
                            HeightRequest = 1,
                            Color = Color.FromArgb("#444")
                        },
                        new Label
                        {
                            Text = "🚪 Cerrar sesión",
                            FontSize = 16,
                            TextColor = Colors.White,
                            HorizontalTextAlignment = TextAlignment.Center,
                            FontFamily = DeviceInfo.Platform == DevicePlatform.Android ? "sans-serif" : null,
                            Padding = new Thickness(10),
                            GestureRecognizers =
                            {
                                new TapGestureRecognizer
                                {
                                    Command = new Command(() =>
                                    {
                                        Preferences.Remove("estoy_logueado");
                                        Preferences.Remove("numero_de_movil");
                                        Preferences.Remove("soy_admin");
                                        this.Close();
                                        Shell.Current.GoToAsync("//LoginPage");
                                    })
                                }
                            }
                        }
                    }
                }
            };

            Content = new Grid
            {
                BackgroundColor = Colors.Transparent,
                Children = { fondo }
            };
        }
    }
}
using AppMaui.Pages;
using AppMaui.Models;

#if ANDROID
using Android.Views.InputMethods;
using Android.Content;
#endif
#if WINDOWS
using Microsoft.Maui.Controls;
#endif

namespace AppMaui;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();

#if WINDOWS
        // Ajustes visuales solo para PC
        MainLayout.HeightRequest = -1;
        MainLayout.HorizontalOptions = LayoutOptions.Center;
        MainLayout.WidthRequest = 450;
        MainLayout.Spacing = 20;
        MainLayout.Padding = 0;
        recordarmeView.IsVisible = true;
#else
        MainLayout.Padding = 20;
#endif

    }

    private async void LoginButton_Clicked(object sender, EventArgs e)
    {
        string usuario = usuarioEntry.Text?.Trim();
        string contraseña = passwordEntry.Text;

        // Desactivar botón e iniciar overlay
        loginButton.IsEnabled = false;
        loadingOverlay.IsVisible = true;

        try
        {
            OcultarTeclado();
            // ─────────────────────
            // 2. Credenciales
            // ─────────────────────
            bool esAdmin = string.Equals(usuario, "admin", StringComparison.OrdinalIgnoreCase) &&
                              contraseña == "lpz1234";

            bool esMovil = (string.Equals(usuario, "movil1", StringComparison.OrdinalIgnoreCase) && contraseña == "movil1") ||
                              (string.Equals(usuario, "movil2", StringComparison.OrdinalIgnoreCase) && contraseña == "movil2") ||
                              (string.Equals(usuario, "movil3", StringComparison.OrdinalIgnoreCase) && contraseña == "movil3");
#if WINDOWS
        if (esAdmin)
        {
            if(recordarmeCheck.IsChecked) Preferences.Set("estoy_logueado", true);
                await Shell.Current.GoToAsync("//stock"); // Para PC
            }
            else
            {
                await DisplayAlert("Error", "Usuario o contraseña incorrectos", "OK");
            }
#elif ANDROID
            if (esAdmin || esMovil)
            {
                if (esAdmin)
                {
                    Preferences.Set("estoy_logueado", true);
                    Preferences.Set("soy_admin", true);
                    await Shell.Current.GoToAsync("//stockcelu");
                }
                else if (esMovil)
                {
                    Preferences.Set("estoy_logueado", true);
                    switch (usuario!.ToLower())
                    {
                        case "movil1":
                            Preferences.Set("numero_de_movil", (int)UbicacionStock.Movil1); break;
                        case "movil2":
                            Preferences.Set("numero_de_movil", (int)UbicacionStock.Movil2); break;
                        case "movil3":
                            Preferences.Set("numero_de_movil", (int)UbicacionStock.Movil3); break;
                        default:
                            throw new Exception("No se pudo seleccionar el móvil.");
                    }
                    await Shell.Current.GoToAsync("//menu");
                }
            }

            else
            {
                await DisplayAlert("Error", "Usuario o contraseña incorrectos", "OK");
            }
#endif

        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un problema: {ex.Message}", "OK");
        }
        finally
        {
            loadingOverlay.IsVisible = false;
            loginButton.IsEnabled = true;
        }
    }

    private void OcultarTeclado()
    {
#if ANDROID
        var activity = Platform.CurrentActivity;
        var inputMethodManager = activity.GetSystemService(Context.InputMethodService) as InputMethodManager;
        var view = activity.CurrentFocus;

        if (view != null)
        {
            inputMethodManager.HideSoftInputFromWindow(view.WindowToken, HideSoftInputFlags.None);
            view.ClearFocus();
        }
#endif
    }

    private void passwordEntry_Completed(object sender, EventArgs e)
    {
        LoginButton_Clicked(sender, e);
    }
}







//    private async void LoginButton_Clicked(object sender, EventArgs e)
//    {
//        string usuario = usuarioEntry.Text?.Trim();
//        string contraseña = passwordEntry.Text;

//        // Desactivar botón e iniciar overlay
//        loginButton.IsEnabled = false;
//        loadingOverlay.IsVisible = true;

//        try
//        {
//            OcultarTeclado();
//#if WINDOWS
//if (string.Equals(usuario, "admin", StringComparison.OrdinalIgnoreCase) && contraseña == "lpz1234")
//            {
//                if(recordarmeCheck.IsChecked) Preferences.Set("estoy_logueado", true);
//                await Shell.Current.GoToAsync("//stock"); // Para PC
//            }
//            else
//            {
//                await DisplayAlert("Error", "Usuario o contraseña incorrectos", "OK");
//            }
//#elif ANDROID
//            if ((string.Equals(usuario, "movil1", StringComparison.OrdinalIgnoreCase) && contraseña == "movil1") ||
//                (string.Equals(usuario, "movil2", StringComparison.OrdinalIgnoreCase) && contraseña == "movil2") ||
//                (string.Equals(usuario, "movil3", StringComparison.OrdinalIgnoreCase) && contraseña == "movil3"))
//            {
//                Preferences.Set("estoy_logueado", true);
//                switch (usuario!.ToLower())
//                {
//                    case "movil1":
//                        Preferences.Set("numero_de_movil", (int)UbicacionStock.Movil1); break;
//                    case "movil2":
//                        Preferences.Set("numero_de_movil", (int)UbicacionStock.Movil2); break;
//                    case "movil3":
//                        Preferences.Set("numero_de_movil", (int)UbicacionStock.Movil3); break;
//                    default:
//                        throw new Exception("No se pudo seleccionar el móvil.");
//                }
//            }
//            else
//            {
//                await DisplayAlert("Error", "Usuario o contraseña incorrectos", "OK");
//            }
//#endif
//        }
//        catch (Exception ex)
//        {
//            await DisplayAlert("Error", $"Ocurrió un problema: {ex.Message}", "OK");
//        }
//        finally
//        {
//            loadingOverlay.IsVisible = false;
//            loginButton.IsEnabled = true;
//        }
//    }
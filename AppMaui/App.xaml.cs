
namespace AppMaui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();
    }

    protected override async void OnStart()
    {
        base.OnStart();
#if WINDOWS
        if (Preferences.Get("estoy_logueado", false))
        {
            await Shell.Current.GoToAsync("//stock");
        }
#elif ANDROID
        if (Preferences.Get("estoy_logueado", false))
        {
            if (Preferences.Get("soy_admin", false))
                await Shell.Current.GoToAsync("//stockcelu");
            if(Preferences.Get("numero_de_movil", -1) > 0)
                await Shell.Current.GoToAsync("//menu");
        }
#endif
    }
}


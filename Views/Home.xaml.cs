namespace TerraMarcadaV2.Views;

public partial class Home : ContentPage
{
	public Home()
	{
		InitializeComponent();
	}

    private async Task<(PermissionStatus locationStatus, PermissionStatus bluetoothStatus, PermissionStatus cameraStatus)> RequestPermissionsAsync()
    {
        // Solicitar permissão de localização (necessária para escanear dispositivos Bluetooth)
        var locationStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        // Solicitar permissão de Bluetooth (Android 12+)
        var bluetoothStatus = await Permissions.RequestAsync<Permissions.Bluetooth>();

        var cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();

        //var btScanStatus = await Permissions.RequestAsync < Permissions.Blu>();

        // Retornando os dois estados de permissão como uma tupla
        return (locationStatus, bluetoothStatus, cameraStatus);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await Task.Delay(1000); // 10000 milissegundos = 10 segundos


        var (locationStatus, bluetoothStatus, cameraStatus) = await RequestPermissionsAsync();

        if (locationStatus == PermissionStatus.Granted && bluetoothStatus == PermissionStatus.Granted && cameraStatus == PermissionStatus.Granted)
        {
            //await Shell.Current.DisplayAlert(
            //    "Permissões debug",
            //    "debug debug",
            //    "OK");
            //Console.WriteLine("Permissões concedidas.");
            // Permissões concedidas, pode continuar com a inicialização do aplicativo
        }
        else
        {
            var deniedPermissions = new List<string>();
            if (locationStatus != PermissionStatus.Granted)
                deniedPermissions.Add("Localização");
            if (bluetoothStatus != PermissionStatus.Granted)
                deniedPermissions.Add("Bluetooth");
            if (cameraStatus != PermissionStatus.Granted)
                deniedPermissions.Add("Câmera");

            var deniedPermissionsMessage = string.Join(", ", deniedPermissions);

            // Exibir mensagem de aviso se as permissões forem negadas
            await Shell.Current.DisplayAlert(
                "Permissões Necessárias",
                $"Algumas funcionalidade não estarão disponíveis porque as permissões: {deniedPermissionsMessage} foram negadas. As permissões são necessárias para o funcionamento adequado do aplicativo.",
                "OK");

            // Desabilitar os botões caso as permissões não tenham sido concedidas
            MapPageBtn.IsEnabled = false;
            GnssPageBtn.IsEnabled = false;
        }

        // Habilitar ou desabilitar botões conforme as permissões
        MapPageBtn.IsEnabled = locationStatus == PermissionStatus.Granted;
        GnssPageBtn.IsEnabled = bluetoothStatus == PermissionStatus.Granted;
        GeoCameraBtn.IsEnabled = cameraStatus == PermissionStatus.Granted;
    }

    private async void OnMapPageClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///Mapa");
    }

    private async void OnGNSSPageClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///ColetoraGnss");
    }

    private async void OnGeoCameraPageClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///GeoCamera");
    }

    private async void OnExtraPageClicked(object sender, EventArgs e)
    {
        
    }
}
namespace TerraMarcadaV2.Views;

public partial class Home : ContentPage
{
	public Home()
	{
		InitializeComponent();
	}

    private async Task<(PermissionStatus locationStatus, PermissionStatus bluetoothStatus, PermissionStatus cameraStatus)> RequestPermissionsAsync()
    {
        // Solicitar permiss�o de localiza��o (necess�ria para escanear dispositivos Bluetooth)
        var locationStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        // Solicitar permiss�o de Bluetooth (Android 12+)
        var bluetoothStatus = await Permissions.RequestAsync<Permissions.Bluetooth>();

        var cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();

        //var btScanStatus = await Permissions.RequestAsync < Permissions.Blu>();

        // Retornando os dois estados de permiss�o como uma tupla
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
            //    "Permiss�es debug",
            //    "debug debug",
            //    "OK");
            //Console.WriteLine("Permiss�es concedidas.");
            // Permiss�es concedidas, pode continuar com a inicializa��o do aplicativo
        }
        else
        {
            var deniedPermissions = new List<string>();
            if (locationStatus != PermissionStatus.Granted)
                deniedPermissions.Add("Localiza��o");
            if (bluetoothStatus != PermissionStatus.Granted)
                deniedPermissions.Add("Bluetooth");
            if (cameraStatus != PermissionStatus.Granted)
                deniedPermissions.Add("C�mera");

            var deniedPermissionsMessage = string.Join(", ", deniedPermissions);

            // Exibir mensagem de aviso se as permiss�es forem negadas
            await Shell.Current.DisplayAlert(
                "Permiss�es Necess�rias",
                $"Algumas funcionalidade n�o estar�o dispon�veis porque as permiss�es: {deniedPermissionsMessage} foram negadas. As permiss�es s�o necess�rias para o funcionamento adequado do aplicativo.",
                "OK");

            // Desabilitar os bot�es caso as permiss�es n�o tenham sido concedidas
            MapPageBtn.IsEnabled = false;
            GnssPageBtn.IsEnabled = false;
        }

        // Habilitar ou desabilitar bot�es conforme as permiss�es
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
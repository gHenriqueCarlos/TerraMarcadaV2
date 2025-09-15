using TerraMarcadaV2.Services;
using TerraMarcadaV2.ViewModels;
using TerraMarcadaV2.Views;

namespace TerraMarcadaV2.Views;

public partial class SelectDevicePage : ContentPage
{
    private readonly SelectDeviceViewModel _vm;
    //private readonly BluetoothService _bt = ServiceHelper.GetService<BluetoothService>();

    public SelectDevicePage()
    {
        InitializeComponent();

        // Passa a inst�ncia do BluetoothService para o ViewModel
        var btService = ServiceHelper.GetService<BluetoothService>();
        //_bt = btService;

        // Passa o BluetoothService para o ViewModel
        _vm = new SelectDeviceViewModel(btService);
        BindingContext = _vm;
    }

    // Este m�todo ser� chamado assim que a p�gina aparecer na tela
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Tentar carregar os dispositivos assim que a p�gina aparecer
        await LoadDevicesAsync();
    }

    // M�todo para carregar dispositivos Bluetooth no ViewModel
    private async Task LoadDevicesAsync()
    {
        var _bt = ServiceHelper.GetService<BluetoothService>();
        try
        {
            var isBluetoothEnabled = await _bt.CheckBluetoothStatusAsync();
            if (!isBluetoothEnabled)
            {
                StatusLabel.Text = "Bluetooth est� desativado. Ative o Bluetooth nas configura��es do dispositivo.";
                StatusLabel.IsVisible = true;
                return;
            }

            var hasPermissions = await _bt.CheckPermissionsAsync();
            if (!hasPermissions)
            {
                StatusLabel.Text = "Permiss�es necess�rias n�o concedidas. Conceda as permiss�es de Bluetooth e Localiza��o nas configura��es.";
                StatusLabel.IsVisible = true;
                return;
            }

            await _vm.LoadDevicesAsync();

            if (_vm.Devices.Count == 0)
            {
                StatusLabel.Text = "Nenhum dispositivo encontrado. Verifique se o Bluetooth est� ativado e os dispositivos est�o dentro do alcance.";
                StatusLabel.IsVisible = true;
            }
            else
            {
                StatusLabel.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Erro ao buscar dispositivos: {ex.Message}. Tente novamente.";
            StatusLabel.IsVisible = true;
        }
    }

    // M�todo chamado quando o bot�o de "Buscar" � pressionado
    private async void OnBuscarClicked(object sender, EventArgs e)
    {
        await LoadDevicesAsync();
    }

    // M�todo chamado quando o usu�rio seleciona um dispositivo da lista
    private async void OnDeviceSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is BluetoothDeviceItem item)
        {
            var btService = ServiceHelper.GetService<BluetoothService>();  // Acessa o BluetoothService do ServiceHelper
            bool ok = await btService.ConnectAsync(item.Address);
            if (ok)
            {
                await DisplayAlert("Conectado", $"Conectado a {item.Name}", "OK");
                // Navegar para a tela GNSS
                await Shell.Current.GoToAsync("GnssPage");
            }
            else
            {
                await DisplayAlert("Erro", "N�o foi poss�vel conectar.", "OK");
            }
        }
    }
}

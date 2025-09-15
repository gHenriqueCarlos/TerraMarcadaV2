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

        // Passa a instância do BluetoothService para o ViewModel
        var btService = ServiceHelper.GetService<BluetoothService>();
        //_bt = btService;

        // Passa o BluetoothService para o ViewModel
        _vm = new SelectDeviceViewModel(btService);
        BindingContext = _vm;
    }

    // Este método será chamado assim que a página aparecer na tela
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Tentar carregar os dispositivos assim que a página aparecer
        await LoadDevicesAsync();
    }

    // Método para carregar dispositivos Bluetooth no ViewModel
    private async Task LoadDevicesAsync()
    {
        var _bt = ServiceHelper.GetService<BluetoothService>();
        try
        {
            var isBluetoothEnabled = await _bt.CheckBluetoothStatusAsync();
            if (!isBluetoothEnabled)
            {
                StatusLabel.Text = "Bluetooth está desativado. Ative o Bluetooth nas configurações do dispositivo.";
                StatusLabel.IsVisible = true;
                return;
            }

            var hasPermissions = await _bt.CheckPermissionsAsync();
            if (!hasPermissions)
            {
                StatusLabel.Text = "Permissões necessárias não concedidas. Conceda as permissões de Bluetooth e Localização nas configurações.";
                StatusLabel.IsVisible = true;
                return;
            }

            await _vm.LoadDevicesAsync();

            if (_vm.Devices.Count == 0)
            {
                StatusLabel.Text = "Nenhum dispositivo encontrado. Verifique se o Bluetooth está ativado e os dispositivos estão dentro do alcance.";
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

    // Método chamado quando o botão de "Buscar" é pressionado
    private async void OnBuscarClicked(object sender, EventArgs e)
    {
        await LoadDevicesAsync();
    }

    // Método chamado quando o usuário seleciona um dispositivo da lista
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
                await DisplayAlert("Erro", "Não foi possível conectar.", "OK");
            }
        }
    }
}

using System.Collections.ObjectModel;
using TerraMarcadaV2.Services;
using System.Threading.Tasks;

namespace TerraMarcadaV2.ViewModels
{
    public class SelectDeviceViewModel
    {
        private readonly BluetoothService _bt;

        public ObservableCollection<BluetoothDeviceItem> Devices { get; set; } = new();

        // Construtor que recebe a instância do BluetoothService
        public SelectDeviceViewModel(BluetoothService bt)
        {
            _bt = bt;
        }

        // Método assíncrono para carregar dispositivos Bluetooth
        public async Task LoadDevicesAsync()
        {
            Devices.Clear();  // Limpa a lista de dispositivos antes de carregar novos

            // Descobre os dispositivos Bluetooth
            var foundDevices = await _bt.DiscoverDevicesAsync();

            // Se dispositivos foram encontrados, adiciona à coleção
            foreach (var device in foundDevices)
            {
                Devices.Add(new BluetoothDeviceItem { Name = device.Name, Address = device.Address });
            }
            await Shell.Current.DisplayAlert("Dispositivos Encontrados", $"{Devices.Count} dispositivos encontrados.", "OK");
        }
    }

    public class BluetoothDeviceItem
    {
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
    }
}

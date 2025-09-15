using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TerraMarcadaV2.Services
{
    public class BluetoothService
    {
        private BluetoothClient? _client;
        private StreamReader? _reader;

        // Propriedade para verificar se o Bluetooth está conectado
        public bool IsConnected => _client?.Connected ?? false;

        public event Action<string>? OnNmeaReceived;

        // Método para verificar o status do Bluetooth
        public async Task<bool> CheckBluetoothStatusAsync()
        {
            try
            {
                var bluetoothRadio = BluetoothRadio.Default;

                if (bluetoothRadio != null)
                {
                    return true;  // Bluetooth está ativado
                }
                else
                {
                    Console.WriteLine("Bluetooth não está ativado.");
                    return false;  // Bluetooth não está ativado
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao verificar o status do Bluetooth: {ex.Message}");
                return false;
            }
        }


        // Método para verificar as permissões de Bluetooth e localização
        public async Task<bool> CheckPermissionsAsync()
        {
            var locationPermission = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            var bluetoothPermission = await Permissions.CheckStatusAsync<Permissions.Bluetooth>();

            return locationPermission == PermissionStatus.Granted && bluetoothPermission == PermissionStatus.Granted;
        }

        // Método assíncrono para descobrir dispositivos Bluetooth
        public async Task<List<(string Name, string Address)>> DiscoverDevicesAsync()
        {
            var devices = new List<(string Name, string Address)>();

            try
            {
                var isBluetoothEnabled = await CheckBluetoothStatusAsync();
                if (!isBluetoothEnabled)
                {
                    Console.WriteLine("Bluetooth está desativado.");
                    await Shell.Current.DisplayAlert("Bluetooth Desativado", "Por favor, ative o Bluetooth nas configurações do dispositivo.", "OK");
                    return devices;  // Retorna uma lista vazia se o Bluetooth não estiver ativado
                }

                var hasPermissions = await CheckPermissionsAsync();
                if (!hasPermissions)
                {
                    Console.WriteLine("Permissões de Bluetooth ou Localização não concedidas.");
                    await Shell.Current.DisplayAlert("Permissões Necessárias", "Por favor, conceda as permissões de Bluetooth e Localização nas configurações do aplicativo.", "OK");
                    return devices;  // Retorna uma lista vazia se as permissões não foram concedidas
                }

                // Criação do BluetoothClient
                _client = new BluetoothClient();
                var foundDevices = _client.DiscoverDevices();  // Descobre dispositivos Bluetooth próximos

                foreach (var device in foundDevices)
                {
                    devices.Add((device.DeviceName, device.DeviceAddress.ToString()));
                }

                await Shell.Current.DisplayAlert("Bluetooth(DiscoverDevicesAsync)", $"Lista: {devices.Count}", "OK");

                return devices;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar dispositivos Bluetooth: {ex.Message}");
                await Shell.Current.DisplayAlert("Erro", $"Erro ao buscar dispositivos Bluetooth: {ex.Message}", "OK");
                return devices;
            }
        }

        // Método assíncrono para conectar a um dispositivo
        public async Task<bool> ConnectAsync(string address)
        {
            try
            {
                var btAddr = BluetoothAddress.Parse(address);
                _client ??= new BluetoothClient();
                var endPoint = new BluetoothEndPoint(btAddr, InTheHand.Net.Bluetooth.BluetoothService.SerialPort);

                // Conecta ao dispositivo via Bluetooth
                _client.Connect(endPoint);
                var stream = _client.GetStream();
                _reader = new StreamReader(stream, Encoding.ASCII);

                _ = Task.Run(async () =>
                {
                    while (_reader != null)
                    {
                        var line = await _reader.ReadLineAsync();
                        if (!string.IsNullOrEmpty(line) && line.StartsWith("$"))
                            OnNmeaReceived?.Invoke(line);
                    }
                });

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao conectar: {ex.Message}");
                return false;
            }
        }

        // Método para desconectar do dispositivo Bluetooth
        public void Disconnect()
        {
            _reader?.Dispose();
            _client?.Close();
            _client = null;
            _reader = null;
        }
    }
}

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TerraMarcadaV2.Models;
using TerraMarcadaV2.Services;

namespace TerraMarcadaV2.ViewModels
{
    public class GnssViewModel : INotifyPropertyChanged
    {
        private readonly BluetoothService _bt;
        private readonly NmeaParser _parser = new();

        public GnssData Data => _parser.Current;

        private bool _connected;
        public bool Connected
        {
            get => _connected;
            set
            {
                _connected = value;
                OnPropertyChanged();
            }
        }

        public ICommand ConnectCommand => new Command(async () => await Connect());

        public event PropertyChangedEventHandler? PropertyChanged;

        public GnssViewModel(BluetoothService bt)
        {
            _bt = bt;  // Usando a instância compartilhada de BluetoothService
            _bt ??= ServiceHelper.GetService<BluetoothService>();

            Connected =_bt.IsConnected;
            _bt.OnNmeaReceived += line =>
            {
                _parser.Parse(line);
                OnPropertyChanged(nameof(Data));
            };
        }

        public async Task Connect()
        {
            bool connectionResult = await _bt.ConnectAsync("HiperSR");

            if (!connectionResult)
            {
                // Se a conexão falhar, exibir uma mensagem detalhada
                await Shell.Current.DisplayAlert("Erro", "Não foi possível conectar ao dispositivo HiperSR. Verifique se o Bluetooth está ativado e se as permissões foram concedidas.", "OK");
                await Shell.Current.GoToAsync("SelectDevicePage");
            }
            else
            {
                Connected = true;
            }
        }


        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

using TerraMarcadaV2.Services;
using TerraMarcadaV2.ViewModels;

namespace TerraMarcadaV2.Views;

public partial class GnssPage : ContentPage
{
    private readonly BluetoothService _bt;

    public GnssPage()
    {
        InitializeComponent();

        _bt = ServiceHelper.GetService<BluetoothService>();
        BindingContext = new GnssViewModel(_bt);
    }

    // M�todo para desconectar o Bluetooth ao sair da p�gina
    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (_bt != null && _bt.IsConnected)
        {
            _bt.Disconnect();
        }
    }
}


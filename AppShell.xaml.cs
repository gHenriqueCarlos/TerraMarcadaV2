using TerraMarcadaV2.Views;

namespace TerraMarcadaV2
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            //Routing.RegisterRoute("SelectDevicePage", typeof(SelectDevicePage));
            //Routing.RegisterRoute("ColetoraGnss", typeof(GnssPage));
            Routing.RegisterRoute("GeoCamera", typeof(GeoCamera));
        }
    }
}

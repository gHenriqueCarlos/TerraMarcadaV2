using TerraMarcadaV2.Services;

namespace TerraMarcadaV2
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            this.UserAppTheme = AppTheme.Light;
            var databaseService = ServiceHelper.GetService<DatabaseService>();

            // Inicializa sem bloquear o UI thread
            //Task.Run(async () => await DatabaseService.InitializeAsync());
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}
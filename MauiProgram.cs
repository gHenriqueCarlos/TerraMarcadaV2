using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.DependencyInjection;
using Maui.GoogleMaps.Clustering.Hosting;
using Maui.GoogleMaps.Hosting;
using Microsoft.Extensions.Logging;
using TerraMarcadaV2.Services;
using TerraMarcadaV2.ViewModels;

namespace TerraMarcadaV2
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseMauiCommunityToolkitCamera()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "tmdatav4.db3");
            builder.Services.AddSingleton(new DatabaseService(dbPath));
            //builder.Services.AddTransient(MapDataViewModel());

            builder.Services.AddSingleton<MapDataViewModel>();

#if ANDROID
            builder.UseGoogleMaps();
#elif IOS
            //builder.UseGoogleMaps(Variables.GOOGLE_MAPS_IOS_API_KEY);
            builder.UseGoogleMaps("AIzaSyBikk1CUzHGvaT4d8uPby3Gy67gBCCuIFA");
#endif

            builder.UseGoogleMapsClustering();

#if DEBUG
            builder.Logging.AddDebug();
#endif


            var app = builder.Build();

            // Configure Ioc
            Ioc.Default.ConfigureServices(app.Services);
            ServiceHelper.Initialize(app.Services);

            return app;
        }
    }
}

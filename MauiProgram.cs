using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.DependencyInjection;
using Maui.GoogleMaps.Clustering.Hosting;
using Maui.GoogleMaps.Hosting;
using Microsoft.Extensions.Logging;
using TerraMarcadaV2.Services;
using TerraMarcadaV2.ViewModels;
using TerraMarcadaV2.Views;
using SkiaSharp.Views.Maui.Controls.Hosting;
namespace TerraMarcadaV2
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()
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

            builder.Services.AddSingleton<BluetoothService>();

            // Registrar o ViewModel se necessário (se o ViewModel não for automaticamente instanciado na página)
            builder.Services.AddSingleton<GnssViewModel>();
            builder.Services.AddSingleton<SelectDeviceViewModel>();

            // Registrar as páginas para o Shell (isso não é estritamente necessário se você usa rotas no Shell)
            builder.Services.AddTransient<GnssPage>();
            builder.Services.AddTransient<SelectDevicePage>();

#if ANDROID
            builder.Services.AddSingleton<IPhotoSaver, PhotoSaver_Android>(); // #if ANDROID
#elif IOS
            builder.Services.AddSingleton<IPhotoSaver, PhotoSaver_iOS>();     // #if IOS
#else
            //builder.Services.AddSingleton<IPhotoSaver, PhotoSaver_Windows>();
#endif

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

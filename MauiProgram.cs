using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.DependencyInjection;
using Maui.GoogleMaps.Clustering.Hosting;
using Maui.GoogleMaps.Hosting;
using Microsoft.Extensions.Logging;
using TerraMarcadaV2.Services;
using TerraMarcadaV2.ViewModels;
using TerraMarcadaV2.Views;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Microsoft.Extensions.Configuration;
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
            builder.Services.AddSingleton<MapDataViewModel>();

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
            builder.UseGoogleMaps("your_api_key");
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

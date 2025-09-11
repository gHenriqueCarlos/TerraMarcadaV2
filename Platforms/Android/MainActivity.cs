using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using Android.Widget;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using TerraMarcadaV2.Services;
using TerraMarcadaV2.ViewModels;

namespace TerraMarcadaV2
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        private MapDataViewModel mapDataViewModel;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Se o seu app usa injeção de dependências
            mapDataViewModel = ServiceHelper.GetService<MapDataViewModel>();

            // Verificar permissões para o Android 10 ou superior (Android Q)
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) != Permission.Granted)
                {
                    ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.ReadExternalStorage }, 1);
                }
            }

            // Se for chamado com um arquivo KML
            if (Intent?.Action == Android.Content.Intent.ActionView)
            {
                var uri = Intent.Data;
                if (uri != null && uri.Scheme == "file")
                {
                    var filePath = uri.Path;
                    if (!string.IsNullOrEmpty(filePath) && filePath.EndsWith(".kml", StringComparison.OrdinalIgnoreCase))
                    {
                        // Verifique se o caminho é acessível
                        var file = new Java.IO.File(filePath);
                        if (file.Exists())
                        {
                            // Chama o método de importação
                            ImportKML(filePath);
                        }
                        else
                        {
                            // Mostrar erro caso o arquivo não exista
                            Toast.MakeText(this, "Arquivo não encontrado ou inacessível", ToastLength.Short).Show();
                        }
                    }
                }
                else if (uri != null)
                {
                    // Para o caso de o URI ser de um ContentProvider, por exemplo
                    string filePath = GetRealPathFromURI(uri);
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        ImportKML(filePath);
                    }
                }
            }
        }

        // Para lidar com ContentProviders e outros tipos de URI (caso o arquivo esteja em outra localização):
        private string GetRealPathFromURI(Android.Net.Uri contentUri)
        {
            string[] proj = { Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data };
            var cursor = ContentResolver.Query(contentUri, proj, null, null, null);
            cursor.MoveToFirst();
            int columnIndex = cursor.GetColumnIndexOrThrow(Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data);
            string path = cursor.GetString(columnIndex);
            cursor.Close();
            return path;
        }


        // Função para importar o arquivo KML
        private async void ImportKML(string filePath)
        {
            try
            {
                if (mapDataViewModel != null)
                {
                    await mapDataViewModel.ImportKML(filePath);
                }
            }
            catch (Exception ex)
            {
                // Tratar o erro caso ocorra durante o processo
                Log.Error("KMLImportError", ex.Message);
            }
        }
    }
}

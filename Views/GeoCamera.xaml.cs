using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Core; // CameraInfo, CameraPosition, CameraFlashMode
using Microsoft.Maui.Devices.Sensors;
using TerraMarcadaV2.Services;
using System.Globalization;
using CommunityToolkit.Maui.Core.Primitives;

namespace TerraMarcadaV2.Views
{
    public partial class GeoCamera : ContentPage
    {
        readonly IDispatcherTimer _timer;
        Location _lastLoc;
        double? _accuracy, _heading, _speed;
        DateTimeOffset _fixTs;

        CancellationTokenSource? _previewCts;
        bool _useRear = true;
        double _currentZoom = 1.0;

        public GeoCamera()
        {
            InitializeComponent();

            _timer = Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);  // Aumentando o intervalo
            _timer.Tick += async (_, __) => await RefreshLocationAsync();

        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await EnsurePermissionsAsync();

            _previewCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            try
            {
                await Camera.StartCameraPreview(_previewCts.Token);
            }
            catch { }
            _timer.Start();
            await RefreshLocationAsync();
            UpdateHud();
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _timer.Stop();
            try { Camera.StopCameraPreview(); } catch { }
            _previewCts?.Cancel();
            _previewCts?.Dispose();
            _previewCts = null;
        }

        async Task EnsurePermissionsAsync()
        {
            var cam = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (cam != PermissionStatus.Granted)
            {
                var result = await Permissions.RequestAsync<Permissions.Camera>();
                if (result != PermissionStatus.Granted)
                {
                    await DisplayAlert("Permissão Negada", "O aplicativo precisa de permissão para acessar a câmera.", "OK");
                    return;
                }
            }

            var loc = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (loc != PermissionStatus.Granted)
            {
                loc = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (loc != PermissionStatus.Granted)
                {
                    await DisplayAlert("Permissão Negada", "O aplicativo precisa de permissão para acessar sua localização.", "OK");
                    return;
                }
            }

#if ANDROID
            if (DeviceInfo.Version.Major <= 9)
                _ = await Permissions.RequestAsync<Permissions.StorageWrite>();
#endif
        }
        async Task RefreshLocationAsync()
        {
            try
            {
                var req = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
                var loc = await Geolocation.Default.GetLocationAsync(req);
                if (loc != null)
                {
                    _lastLoc = loc;
                    _accuracy = loc.Accuracy;
                    _heading = loc.Course;
                    _speed = loc.Speed;
                    _fixTs = DateTimeOffset.UtcNow;
                    UpdateHud();
                }
            }
            catch { /* sem pânico, tenta no próximo tick */ }
        }

        void UpdateHud()
        {
            var ci = new CultureInfo("pt-BR");
            var ts = _fixTs == default ? DateTimeOffset.UtcNow : _fixTs.ToLocalTime();
            LblDate.Text = ts.ToString("dd 'de' MMMM 'de' yyyy, 'às' HH:mm:ss", ci);

            if (_lastLoc == null)
            {
                LblDMS.Text = "Obtendo localização…";
                LblDec.Text = "";
                LblExtra.Text = "";
                return;
            }

            string latDms = CoordsService.CoordToDMS(_lastLoc.Latitude, true);
            string lonDms = CoordsService.CoordToDMS(_lastLoc.Longitude, false);
            LblDMS.Text = $"{latDms}  {lonDms}";
            LblDec.Text = $"LAT: {_lastLoc.Latitude:F6}  LON: {_lastLoc.Longitude:F6}";
            LblExtra.Text = $"±{_accuracy?.ToString("F1") ?? "?"} m  " +
                            $"{(_heading.HasValue ? $"HDG: {_heading:F0}°  " : "")}" +
                            $"{(_speed.HasValue ? $"SPD: {_speed * 3.6:F1} km/h" : "")}";
        }

        async Task SelectCameraAsync(CameraPosition pos)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                var list = await Camera.GetAvailableCameras(cts.Token);
                Camera.SelectedCamera = list?.FirstOrDefault(c => c.Position == pos);
            }
            catch { }
        }
        async void ToggleCamera_Clicked(object sender, EventArgs e)
        {
            _useRear = !_useRear;
            await SelectCameraAsync(_useRear ? CameraPosition.Rear : CameraPosition.Front);
        }

        async void Capture_Clicked(object sender, EventArgs e)
        {
            try
            {
                // Ativa o indicador de carregamento
                CaptureIndicator.IsRunning = true;
                CaptureIndicator.IsVisible = true;

                byte[] photoStream = await CaptureImageWithOverlay();
                if (photoStream == null)
                {
                    await DisplayAlert("Erro", "A captura da foto falhou.", "OK");
                    return;
                }

                string fileName = $"TM_{DateTime.UtcNow:yyyyMMdd_HHmmss}.jpg";
                await PhotoSaver.SaveToGalleryAsync(photoStream, fileName, "TerraMarcada");

                // Desativa o indicador de carregamento
                CaptureIndicator.IsRunning = false;
                CaptureIndicator.IsVisible = false;
            }
            catch (Exception ex)
            {
                // Desativa o indicador de carregamento em caso de erro
                CaptureIndicator.IsRunning = false;
                CaptureIndicator.IsVisible = false;
                await DisplayAlert("Erro ao capturar", ex.Message, "OK");
            }
        }


        async Task<byte[]> CaptureImageWithOverlay()
        {
            using var captureImageCTS = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            using var stream = await Camera.CaptureImage(captureImageCTS.Token);  // Garantindo que o stream seja descartado automaticamente
            if (stream == null) return null;

            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var raw = ms.ToArray();  // Converte o stream para byte[]

            var when = _fixTs == default ? DateTimeOffset.UtcNow : _fixTs;
            var withHud = await OverlayServiceGeo.EscreverOverlayBasico(
                raw,
                _lastLoc?.Latitude ?? 0, _lastLoc?.Longitude ?? 0,
                when,
                _accuracy, _heading, _speed);

            return withHud;
        }

        void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            if (e.Status == GestureStatus.Running)
            {
                // Calcula o novo nível de zoom
                _currentZoom *= e.Scale;
                _currentZoom = Math.Max(1.0, Math.Min(3.0, _currentZoom)); // Limita o zoom entre 1x e 3x

                // Aplica o zoom ao CameraView
                //Camera.ZoomToAsync(_currentZoom, 100);
                Camera.ZoomFactor = (float)_currentZoom;
            }
        }

        void ToggleFlash_Clicked(object sender, EventArgs e)
        {
            if(Camera.CameraFlashMode == CameraFlashMode.Off)
            {
                Camera.CameraFlashMode = CameraFlashMode.On;
                BtnFlash.Text = "⚡ On";
            }
            else if(Camera.CameraFlashMode == CameraFlashMode.On)
            {
                Camera.CameraFlashMode = CameraFlashMode.Auto;
                BtnFlash.Text = "⚡ Auto";
            }
            else
            {
                Camera.CameraFlashMode = CameraFlashMode.Off;
                BtnFlash.Text = "⚡ Off";
            }
            //var modes = new[] { CameraFlashMode.Off, CameraFlashMode.Auto, CameraFlashMode.On };
            //_flashIndex = (_flashIndex + 1) % modes.Length;
            //Camera.CameraFlashMode = modes[_flashIndex];
        }
    }
}
#if IOS
using Foundation;
using Photos;
using TerraMarcadaV2.Services;
using UIKit;

public class PhotoSaver_iOS : IPhotoSaver
{
    public Task SaveToGalleryAsync(byte[] data, string fileName, string albumName)
    {
        var tcs = new TaskCompletionSource();
        PHPhotoLibrary.SharedPhotoLibrary.PerformChanges(() =>
        {
            using var nsData = NSData.FromArray(data);
            using var img = new UIImage(nsData);
            PHAssetChangeRequest.FromImage(img);
        }, (success, err) =>
        {
            if (success) tcs.SetResult();
            else tcs.SetException(new Exception(err?.LocalizedDescription ?? "Erro ao salvar na Fototeca"));
        });
        return tcs.Task;
    }
}
#endif

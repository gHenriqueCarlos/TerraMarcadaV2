#if ANDROID
using Android.Content;
using Android.Provider;
using TerraMarcadaV2.Services;
using Application = Android.App.Application;

public class PhotoSaver_Android : IPhotoSaver
{
    public async Task SaveToGalleryAsync(byte[] data, string fileName, string albumName)
    {
        var resolver = Application.Context.ContentResolver!;
        var values = new ContentValues();
        values.Put(MediaStore.IMediaColumns.DisplayName, fileName);
        values.Put(MediaStore.IMediaColumns.MimeType, "image/jpeg");
        values.Put(MediaStore.IMediaColumns.RelativePath, $"Pictures/{albumName}");

        var uri = resolver.Insert(MediaStore.Images.Media.ExternalContentUri!, values)
                  ?? throw new Exception("Falha ao inserir no MediaStore.");

        using var outStream = resolver.OpenOutputStream(uri)!;
        await outStream.WriteAsync(data, 0, data.Length);
        await outStream.FlushAsync();
    }
}
#endif

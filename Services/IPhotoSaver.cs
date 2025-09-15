using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraMarcadaV2.Services
{
    public interface IPhotoSaver
    {
        Task SaveToGalleryAsync(byte[] data, string fileName, string albumName);
    }

    public static class PhotoSaver
    {
        public static IPhotoSaver Instance =>
            ServiceHelper.GetService<IPhotoSaver>();

        public static Task SaveToGalleryAsync(byte[] data, string fileName, string albumName) =>
            Instance.SaveToGalleryAsync(data, fileName, albumName);
    }

}

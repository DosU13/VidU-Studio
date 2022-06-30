using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VidU.data;
using VidU_Studio.util;
using Windows.Media.Editing;
using Windows.Storage;
using Windows.UI;

namespace VidU_Studio.viewmodel
{
    public class MediaViewModel: BindableBase
    {
        public Media data;

        internal static async Task<MediaViewModel> Create(Media data)
        {
            if(data == null) return new MediaViewModel();
            StorageFile f = null;
            try { f = await StorageFile.GetFileFromPathAsync(data.Path); }catch (Exception) { }
            return new MediaViewModel(data, f);
        }

        public MediaViewModel(Media media, StorageFile file = null)
        {
            data = media;
            File = file;
            if(file == null) Task.Run(async () => File = await StorageFile.GetFileFromPathAsync(data.Path));
            if (!IsImage)
            {
                trimStart = (data as Video).TrimFromStart;
                volume = (data as Video).Volume;
            }
        }

        public MediaViewModel(StorageFile storageFile = null)
        {
            if (storageFile == null) return;
            if (storageFile.IsVideo())
            {
                data = new Video();
                trimStart = (data as Video).TrimFromStart;
                volume = (data as Video).Volume;
            }
            else data = new Image();
            data.Path = storageFile.Path;
            file = storageFile;
        }

        private StorageFile file = null;
        public StorageFile File
        {
            get => file;
            set
            {
                SetProperty(ref file, value);
                if (value != null) data.Path = value.Path;
                else data.Path = null;
            }
        }

        public bool IsImage => data is Image;

        private double trimStart = 0;
        public double TrimStart
        {
            get => trimStart;
            set {
                SetProperty(ref trimStart, value);
                if (!IsImage) (data as Video).TrimFromStart = value;
            }
        }

        private int volume = 50;
        public int Volume
        {
            get => volume;
            set
            {
                SetProperty(ref volume, value);
                if (!IsImage) (data as Video).Volume = value;
            }
        }

        internal async Task<MediaClip> CreateMediaClip(double duration)
        {
            if (File == null) return MediaClip.CreateFromColor(
                Color.FromArgb(0,0,0,0), TimeSpan.FromSeconds(duration));
            if (IsImage) return await MediaClip.CreateFromImageFileAsync(File, TimeSpan.FromSeconds(duration));
            else
            {
                var mediaClip = await MediaClip.CreateFromFileAsync(File);
                mediaClip.TrimTimeFromStart = TimeSpan.FromSeconds(TrimStart);
                mediaClip.TrimTimeFromEnd = mediaClip.OriginalDuration 
                    - mediaClip.TrimTimeFromStart - TimeSpan.FromSeconds(duration);
                mediaClip.Volume = 2 * (Volume / 100.0);
                return mediaClip;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VidU.data;
using VidU_Studio.util;
using Windows.Media.Editing;
using Windows.Storage;

namespace VidU_Studio.viewmodel
{
    public class MediaViewModel: BindableBase
    {
        public Media data;

        public MediaViewModel(Media media)
        {
            data = media;
            Task.Run(async () => File = await StorageFile.GetFileFromPathAsync(data.Path));
            if (!IsImage)
            {
                trimStart = (data as Video).TrimFromStart;
                volume = (data as Video).Volume;
            }
        }

        public MediaViewModel(StorageFile storageFile)
        {
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
                data.Path = value.Path;
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

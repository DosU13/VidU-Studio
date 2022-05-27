using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VidU.data;
using VidU_Studio.util;
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
            if (IsImage) _trimStart = 0;
            else _trimStart = (data as Video).TrimFromStart;
        }

        public MediaViewModel(StorageFile storageFile)
        {
            if (storageFile.IsVideo()) data = new Video();
            else data = new Image();
            data.Path = storageFile.Path;
            _trimStart = 0;
            _file = storageFile;
        }

        private StorageFile _file = null;
        public StorageFile File
        {
            get => _file;
            set
            {
                SetProperty(ref _file, value);
                data.Path = value.Path;
            }
        }

        public bool IsImage => data is Image;

        private double _trimStart;

        public double TrimStart
        {
            get => _trimStart;
            set {
                SetProperty(ref _trimStart, value);
                if (!IsImage) (data as Video).TrimFromStart = value;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VidU.data;
using VidU_Studio.util;
using Windows.Storage;

namespace VidU_Studio.viewmodel
{
    public class SingleClipViewModel: ClipViewModel
    {
        public SingleClip data;
        BaseClip ClipViewModel.Data => data;

        public SingleClipViewModel(SingleClip singleClip)
        {
            data = singleClip;
            Task.Run(async () => File = await StorageFile.GetFileFromPathAsync(data.Media.Path));
        }

        public StorageFile _file = null;
        public StorageFile File { get => _file;
            set {
                if (value == null) return;
                _file = value;
                if (value.IsVideo()) { if (data.Media is Image) data.Media = new Video(); }
                else if (data.Media is Video) data.Media = new Image();
                data.Media.Path = value.Path;
                RaisePropertyChanged(nameof(File)); }}

        public StorageFile Thumbnail => File;

        public bool IsImage => data.Media is Image;

        public double Duration
        {
            get => data.StartPos;
            set {
                data.StartPos = value;
                RaisePropertyChanged(nameof(Duration)); 
            }
        }

        public int Volume
        {
            get
            {
                if (data.Media is Image) return 0;
                else return (data.Media as Video).Volume;
            }
            set
            {
                if (data.Media is Image) return;
                else
                {
                    if ((data.Media as Video).Volume == value) return;
                    (data.Media as Video).Volume = value; 
                    RaisePropertyChanged(nameof(Volume));
                }
            }
        }

        public double TrimStart
        {
            get
            {
                if (data.Media is Image) return 0;
                else return (data.Media as Video).TrimFromStart;
            }
            set
            {
                if (data.Media is Image) return;
                else
                {
                    (data.Media as Video).TrimFromStart = value;
                    RaisePropertyChanged(nameof(TrimStart));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
